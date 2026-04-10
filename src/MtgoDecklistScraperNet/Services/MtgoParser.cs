using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using MtgoDecklistModels;

namespace MtgoDecklistScraperNet.Services;

public partial class MtgoParser
{
    private const string DataStartMarker = "window.MTGO.decklists.data = ";

    [GeneratedRegex(@"window\.MTGO\.decklists\.type")]
    private static partial Regex EndMarkerRegex();

    private readonly ILogger<MtgoParser> _logger;

    public MtgoParser(ILogger<MtgoParser> logger)
    {
        _logger = logger;
    }

    public List<string> ParseEventLinks(string indexHtml)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(indexHtml);

        var links = doc.DocumentNode
            .SelectNodes("//a[@href]")
            ?.Select(a => a.GetAttributeValue("href", ""))
            .Where(href => href.StartsWith("/decklist/", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToList() ?? [];

        _logger.LogInformation("Found {Count} event links", links.Count);
        return links;
    }

    public MtgoEvent? ParseEventData(string eventHtml)
    {
        var json = ExtractJson(eventHtml);
        if (json is null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<MtgoEvent>(json);
    }

    public static string? ExtractJson(string html)
    {
        var startIndex = html.IndexOf(DataStartMarker, StringComparison.Ordinal);
        if (startIndex < 0)
        {
            return null;
        }

        startIndex += DataStartMarker.Length;

        var endMatch = EndMarkerRegex().Match(html, startIndex);
        if (!endMatch.Success)
        {
            throw new InvalidOperationException(
                "Found start marker 'window.MTGO.decklists.data' but could not find end marker 'window.MTGO.decklists.type'");
        }

        var segment = html.AsSpan(startIndex, endMatch.Index - startIndex);
        segment = segment.TrimEnd();
        if (segment.Length > 0 && segment[^1] == ';')
        {
            segment = segment[..^1];
        }
        segment = segment.TrimEnd();
        return segment.ToString();
    }
}
