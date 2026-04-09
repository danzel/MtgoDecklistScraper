namespace MtgoDecklistScraperNet.Services;

public class MtgoScraper
{
    private readonly MtgoClient _client;
    private readonly EventSaver _saver;

    public MtgoScraper(MtgoClient client, EventSaver saver)
    {
        _client = client;
        _saver = saver;
    }

    public async Task RunAsync(int? year = null, int? month = null, CancellationToken ct = default)
    {
        Console.WriteLine(year.HasValue
            ? $"Fetching MTGO decklists for {year}/{month:D2}..."
            : "Fetching MTGO decklists...");

        var indexHtml = await _client.FetchIndexAsync(year, month, ct);
        var links = MtgoParser.ParseEventLinks(indexHtml);

        Console.WriteLine($"Found {links.Count} events.");

        for (var i = 0; i < links.Count; i++)
        {
            var link = links[i];
            Console.WriteLine($"[{i + 1}/{links.Count}] Fetching {link}...");

            try
            {
                var eventHtml = await _client.FetchEventPageAsync(link, ct);
                var mtgoEvent = MtgoParser.ParseEventData(eventHtml);

                if (mtgoEvent is null)
                {
                    Console.WriteLine($"  Warning: Could not parse event data from {link}");
                    continue;
                }

                await _saver.SaveEventAsync(link, mtgoEvent, ct);
                Console.WriteLine($"  Saved {mtgoEvent.EventName ?? link} ({mtgoEvent.Decklists.Count} decks)");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error fetching {link}: {ex.Message}");
            }

            // Be polite to the server
            if (i < links.Count - 1)
            {
                await Task.Delay(500, ct);
            }
        }

        Console.WriteLine("Done.");
    }
}
