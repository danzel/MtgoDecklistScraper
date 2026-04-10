using Microsoft.Extensions.Logging;

namespace MtgoDecklistScraperNet.Services;

public class MtgoScraper
{
    private readonly MtgoClient _client;
    private readonly MtgoParser _parser;
    private readonly EventSaver _saver;
    private readonly ILogger<MtgoScraper> _logger;

    public MtgoScraper(MtgoClient client, MtgoParser parser, EventSaver saver, ILogger<MtgoScraper> logger)
    {
        _client = client;
        _parser = parser;
        _saver = saver;
        _logger = logger;
    }

    public async Task RunAsync(int? year = null, int? month = null, CancellationToken ct = default)
    {
        if (year.HasValue)
        {
            _logger.LogInformation("Fetching index for {Year}/{Month}...", year.Value, month!.Value.ToString("D2"));
        }
        else
        {
            _logger.LogInformation("Fetching index...");
        }

        List<string> links;
        try
        {
            var indexHtml = await _client.FetchIndexAsync(year, month, ct);
            links = _parser.ParseEventLinks(indexHtml);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to fetch or parse index");
            throw;
        }

        for (var i = 0; i < links.Count; i++)
        {
            var link = links[i];

            if (_saver.EventExists(link))
            {
                _logger.LogInformation("Skipping {RelativeUrl} (already saved)", link);
                continue;
            }

            _logger.LogInformation("Fetching event {RelativeUrl}...", link);

            try
            {
                var eventHtml = await _client.FetchEventPageAsync(link, ct);
                var mtgoEvent = _parser.ParseEventData(eventHtml);

                if (mtgoEvent is null)
                {
                    _logger.LogWarning("Could not parse event data from {RelativeUrl}", link);
                    continue;
                }

                await _saver.SaveEventAsync(link, mtgoEvent, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to fetch event {RelativeUrl}", link);
            }

            // Be polite to the server
            if (i < links.Count - 1)
            {
                await Task.Delay(500, ct);
            }
        }

        _saver.WriteIndexFiles();
        _logger.LogInformation("Done.");
    }
}
