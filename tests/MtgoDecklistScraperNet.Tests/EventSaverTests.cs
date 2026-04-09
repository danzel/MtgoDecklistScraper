using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using MtgoDecklistModels;
using MtgoDecklistScraperNet.Services;

namespace MtgoDecklistScraperNet.Tests;

public class EventSaverTests
{
    [Fact]
    public async Task SaveEventAsync_CreatesCorrectDirectoryAndFile()
    {
        var fs = new MockFileSystem();
        var saver = new EventSaver(fs, "/output");

        var evt = new MtgoEvent
        {
            Name = "Test Event",
            Format = "modern",
            Decklists = [new MtgoDeck { Player = "TestPlayer" }]
        };

        await saver.SaveEventAsync("/decklist/cmodern-challenge-2024-01-1512345678", evt);

        Assert.True(fs.Directory.Exists("/output/2024/01"));
        var filePath = "/output/2024/01/cmodern-challenge-2024-01-1512345678.json";
        Assert.True(fs.File.Exists(filePath));
    }

    [Fact]
    public async Task SaveEventAsync_WritesValidJson()
    {
        var fs = new MockFileSystem();
        var saver = new EventSaver(fs, "/output");

        var evt = new MtgoEvent
        {
            Name = "Test Event",
            Decklists =
            [
                new MtgoDeck
                {
                    Player = "TestPlayer",
                    MainDeck =
                    [
                        new MtgoCard
                        {
                            Qty = "4",
                            Sideboard = "false",
                            CardAttributes = new CardAttributes { CardName = "Lightning Bolt" }
                        }
                    ]
                }
            ]
        };

        await saver.SaveEventAsync("/decklist/cmodern-challenge-2024-01-1512345678", evt);

        var filePath = "/output/2024/01/cmodern-challenge-2024-01-1512345678.json";
        var content = fs.File.ReadAllText(filePath);
        var deserialized = JsonSerializer.Deserialize<MtgoEvent>(content);

        Assert.NotNull(deserialized);
        Assert.Equal("Test Event", deserialized.Name);
        Assert.Equal("TestPlayer", deserialized.Decklists[0].Player);
        Assert.Equal("Lightning Bolt", deserialized.Decklists[0].MainDeck[0].CardAttributes?.CardName);
    }

    [Theory]
    [InlineData("/decklist/cmodern-challenge-2024-01-1512345678", "2024", "01", "cmodern-challenge-2024-01-1512345678")]
    [InlineData("/decklist/cpioneer-league-2024-12-0312345", "2024", "12", "cpioneer-league-2024-12-0312345")]
    [InlineData("/decklist/standard-league-2026-04-0810429", "2026", "04", "standard-league-2026-04-0810429")]
    [InlineData("/decklist/modern-challenge-64-2026-04-0712838169", "2026", "04", "modern-challenge-64-2026-04-0712838169")]
    public void ParseOutputPath_ExtractsComponents(string url, string expectedYear, string expectedMonth, string expectedFilename)
    {
        var (year, month, filename) = EventSaver.ParseOutputPath(url);

        Assert.Equal(expectedYear, year);
        Assert.Equal(expectedMonth, month);
        Assert.Equal(expectedFilename, filename);
    }
}
