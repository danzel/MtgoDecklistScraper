using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using MtgoDecklistModels;
using MtgoDecklistScraperNet.Services;

namespace MtgoDecklistScraperNet.Tests;

public class EventSaverTests
{
    private static EventSaver CreateSaver(MockFileSystem fs, string outputRoot = "/output") =>
        new(fs, outputRoot, NullLogger<EventSaver>.Instance);

    [Fact]
    public async Task SaveEventAsync_CreatesCorrectDirectoryAndFile()
    {
        var fs = new MockFileSystem();
        var saver = CreateSaver(fs);

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
        var saver = CreateSaver(fs);

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

    [Fact]
    public void ParseOutputPath_ThrowsOnInvalidUrl()
    {
        Assert.Throws<ArgumentException>(() => EventSaver.ParseOutputPath("/decklist/no-date-here"));
    }

    [Fact]
    public void EventExists_ReturnsTrueWhenFileExists()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/output/2024/01/cmodern-challenge-2024-01-1512345678.json"] = new MockFileData("{}")
        });
        var saver = CreateSaver(fs);

        Assert.True(saver.EventExists("/decklist/cmodern-challenge-2024-01-1512345678"));
    }

    [Fact]
    public void EventExists_ReturnsFalseWhenFileMissing()
    {
        var fs = new MockFileSystem();
        var saver = CreateSaver(fs);

        Assert.False(saver.EventExists("/decklist/cmodern-challenge-2024-01-1512345678"));
    }

    [Fact]
    public void WriteIndexFiles_CreatesMonthsJson()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/output/2024/01/event-a-2024-01-15.json"] = new MockFileData("{}"),
            ["/output/2024/03/event-b-2024-03-10.json"] = new MockFileData("{}"),
            ["/output/2025/06/event-c-2025-06-01.json"] = new MockFileData("{}")
        });
        var saver = CreateSaver(fs);

        saver.WriteIndexFiles();

        Assert.True(fs.File.Exists("/output/months.json"));
        var months = JsonSerializer.Deserialize<List<string>>(fs.File.ReadAllText("/output/months.json"));
        Assert.NotNull(months);
        Assert.Equal(["2024/01", "2024/03", "2025/06"], months);
    }

    [Fact]
    public void WriteIndexFiles_CreatesEventsJson()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/output/2024/01/event-b-2024-01-20.json"] = new MockFileData("{}"),
            ["/output/2024/01/event-a-2024-01-15.json"] = new MockFileData("{}")
        });
        var saver = CreateSaver(fs);

        saver.WriteIndexFiles();

        Assert.True(fs.File.Exists("/output/2024/01/events.json"));
        var events = JsonSerializer.Deserialize<List<string>>(fs.File.ReadAllText("/output/2024/01/events.json"));
        Assert.NotNull(events);
        Assert.Equal(["event-a-2024-01-15.json", "event-b-2024-01-20.json"], events);
    }

    [Fact]
    public void WriteIndexFiles_ExcludesEventsJsonFromEventsList()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/output/2024/01/event-a-2024-01-15.json"] = new MockFileData("{}"),
            ["/output/2024/01/events.json"] = new MockFileData("[]")
        });
        var saver = CreateSaver(fs);

        saver.WriteIndexFiles();

        var events = JsonSerializer.Deserialize<List<string>>(fs.File.ReadAllText("/output/2024/01/events.json"));
        Assert.NotNull(events);
        Assert.Single(events);
        Assert.Equal("event-a-2024-01-15.json", events[0]);
    }
}
