using MtgoDecklistScraperNet.Services;
using RichardSzalay.MockHttp;

namespace MtgoDecklistScraperNet.Tests;

public class MtgoClientTests
{
    [Fact]
    public async Task FetchIndexAsync_ReturnsHtml()
    {
        var indexHtml = FixtureHelper.Load("index.html");
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://www.mtgo.com/decklists")
                .Respond("text/html", indexHtml);
        var client = new MtgoClient(mockHttp.ToHttpClient());

        var result = await client.FetchIndexAsync();

        Assert.NotEmpty(result);
        Assert.Contains("/decklist/", result);
    }

    [Fact]
    public async Task FetchIndexAsync_UsesYearMonthUrl_WhenSpecified()
    {
        var indexHtml = FixtureHelper.Load("index.html");
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://www.mtgo.com/decklists/2026/04")
                .Respond("text/html", indexHtml);
        var client = new MtgoClient(mockHttp.ToHttpClient());

        var result = await client.FetchIndexAsync(2026, 4);

        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task FetchEventPageAsync_ReturnsHtml()
    {
        var eventHtml = FixtureHelper.Load("tournament-event.html");
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://www.mtgo.com/decklist/modern-challenge-64-2026-04-0712838169")
                .Respond("text/html", eventHtml);
        var client = new MtgoClient(mockHttp.ToHttpClient());

        var result = await client.FetchEventPageAsync("/decklist/modern-challenge-64-2026-04-0712838169");

        Assert.Contains("window.MTGO.decklists.data", result);
    }
}
