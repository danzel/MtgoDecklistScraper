using System.CommandLine;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using MtgoDecklistScraperNet.Services;

var yearOption = new Option<int?>("--year", "Year to fetch decklists for (e.g. 2026)");
var monthOption = new Option<int?>("--month", "Month to fetch decklists for (1-12)");

var rootCommand = new RootCommand("Scrape MTGO decklists from mtgo.com")
{
    yearOption,
    monthOption
};

rootCommand.AddValidator(result =>
{
    var year = result.GetValueForOption(yearOption);
    var month = result.GetValueForOption(monthOption);

    if (year.HasValue != month.HasValue)
    {
        result.ErrorMessage = "Both --year and --month must be specified together, or neither.";
    }

    if (month.HasValue && (month.Value < 1 || month.Value > 12))
    {
        result.ErrorMessage = "--month must be between 1 and 12.";
    }
});

rootCommand.SetHandler(async (int? year, int? month) =>
{
    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    });

    var httpClient = new HttpClient();
    var fileSystem = new FileSystem();
    var client = new MtgoClient(httpClient);
    var parser = new MtgoParser(loggerFactory.CreateLogger<MtgoParser>());
    var saver = new EventSaver(fileSystem, "output", loggerFactory.CreateLogger<EventSaver>());
    var scraper = new MtgoScraper(client, parser, saver, loggerFactory.CreateLogger<MtgoScraper>());

    await scraper.RunAsync(year, month);
}, yearOption, monthOption);

return await rootCommand.InvokeAsync(args);
