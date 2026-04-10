using Microsoft.Extensions.Logging.Abstractions;
using MtgoDecklistScraperNet.Services;

namespace MtgoDecklistScraperNet.Tests;

public class MtgoParserTests
{
    private readonly MtgoParser _parser = new(NullLogger<MtgoParser>.Instance);

    [Fact]
    public void ParseEventLinks_ExtractsAllDecklistLinks()
    {
        var html = FixtureHelper.Load("index.html");

        var links = _parser.ParseEventLinks(html);

        Assert.NotEmpty(links);
        Assert.All(links, l => Assert.StartsWith("/decklist/", l));
    }

    [Fact]
    public void ParseEventLinks_DeduplicatesLinks()
    {
        var html = """
            <html><body>
              <a href="/decklist/modern-challenge-2024-01-15">Modern Challenge</a>
              <a href="/decklist/pioneer-league-2024-01-15">Pioneer League</a>
              <a href="/decklist/modern-challenge-2024-01-15">Duplicate</a>
            </body></html>
            """;

        var links = _parser.ParseEventLinks(html);

        Assert.Equal(2, links.Count);
    }

    [Fact]
    public void ParseEventLinks_IgnoresNonDecklistLinks()
    {
        var html = """
            <html><body>
              <a href="/about">About</a>
              <a href="/getting-started">Getting Started</a>
              <a href="https://youtube.com">YouTube</a>
              <a href="/decklist/modern-challenge-2024-01-15">Modern Challenge</a>
            </body></html>
            """;

        var links = _parser.ParseEventLinks(html);

        Assert.Single(links);
        Assert.Equal("/decklist/modern-challenge-2024-01-15", links[0]);
    }

    [Fact]
    public void ParseEventData_ParsesLeagueEvent()
    {
        var html = FixtureHelper.Load("league-event.html");

        var result = _parser.ParseEventData(html);

        Assert.NotNull(result);
        Assert.Equal("Pauper League", result.Name);
        Assert.NotNull(result.PlayEventId);
        Assert.NotEmpty(result.Decklists);

        var firstDeck = result.Decklists[0];
        Assert.NotNull(firstDeck.Player);
        Assert.NotEmpty(firstDeck.MainDeck);

        var firstCard = firstDeck.MainDeck[0];
        Assert.NotNull(firstCard.CardAttributes?.CardName);
        Assert.NotNull(firstCard.Qty);
    }

    [Fact]
    public void ParseEventData_ParsesTournamentEvent()
    {
        var html = FixtureHelper.Load("tournament-event.html");

        var result = _parser.ParseEventData(html);

        Assert.NotNull(result);
        Assert.Equal("Modern Challenge 64", result.Description);
        Assert.NotNull(result.Format);
        Assert.NotNull(result.StartTime);
        Assert.NotEmpty(result.Decklists);

        var firstDeck = result.Decklists[0];
        Assert.NotNull(firstDeck.Player);
        Assert.NotEmpty(firstDeck.MainDeck);
    }

    [Fact]
    public void ParseEventData_ReturnsNull_WhenNoDataFound()
    {
        var result = _parser.ParseEventData("<html><body>No data here</body></html>");

        Assert.Null(result);
    }

    [Fact]
    public void ParseEventData_LeagueHasWinLossRecords()
    {
        var html = FixtureHelper.Load("league-event.html");

        var result = _parser.ParseEventData(html);

        Assert.NotNull(result);
        var deckWithWins = result.Decklists.FirstOrDefault(d => d.Wins is not null);
        Assert.NotNull(deckWithWins);
        Assert.NotNull(deckWithWins.Wins!.Wins);
        Assert.NotNull(deckWithWins.Wins.Losses);
    }

    [Fact]
    public void ParseEventData_TournamentHasSideboardCards()
    {
        var html = FixtureHelper.Load("tournament-event.html");

        var result = _parser.ParseEventData(html);

        Assert.NotNull(result);
        var deckWithSideboard = result.Decklists.FirstOrDefault(d => d.SideboardDeck.Count > 0);
        Assert.NotNull(deckWithSideboard);
        Assert.NotEmpty(deckWithSideboard.SideboardDeck);
    }

    [Fact]
    public void ExtractJson_ReturnsValidJson()
    {
        var html = FixtureHelper.Load("tournament-event.html");

        var json = MtgoParser.ExtractJson(html);

        Assert.NotNull(json);
        Assert.StartsWith("{", json);
        Assert.EndsWith("}", json);
    }

    [Fact]
    public void ExtractJson_ThrowsWhenEndMarkerMissing()
    {
        var html = """
            <html><body>
            <script>
            window.MTGO.decklists.data = {"some":"data"};
            </script>
            </body></html>
            """;

        Assert.Throws<InvalidOperationException>(() => MtgoParser.ExtractJson(html));
    }
}
