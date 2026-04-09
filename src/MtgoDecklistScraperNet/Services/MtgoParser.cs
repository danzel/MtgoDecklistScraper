using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MtgoDecklistModels;

namespace MtgoDecklistScraperNet.Services;

public static partial class MtgoParser
{
    private const string DataStartMarker = "window.MTGO.decklists.data = ";

    [GeneratedRegex(@"window\.MTGO\.decklists\.type")]
    private static partial Regex EndMarkerRegex();

    public static List<string> ParseEventLinks(string indexHtml)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(indexHtml);

        var links = doc.DocumentNode
            .SelectNodes("//a[@href]")
            ?.Select(a => a.GetAttributeValue("href", ""))
            .Where(href => href.StartsWith("/decklist/", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToList() ?? [];

        return links;
    }

    public static MtgoEvent? ParseEventData(string eventHtml)
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

        // Try to find the end marker: window.MTGO.decklists.type
        var endMatch = EndMarkerRegex().Match(html, startIndex);
        if (endMatch.Success)
        {
            // Walk backwards from the end marker to find the semicolon and trim
            var segment = html.AsSpan(startIndex, endMatch.Index - startIndex);
            // Trim trailing whitespace and semicolon
            segment = segment.TrimEnd();
            if (segment.Length > 0 && segment[^1] == ';')
            {
                segment = segment[..^1];
            }
            segment = segment.TrimEnd();
            return segment.ToString();
        }

        // Fallback: brace-depth counting from startIndex
        return ExtractJsonByBraceDepth(html, startIndex);
    }

    private static string? ExtractJsonByBraceDepth(string html, int startIndex)
    {
        if (startIndex >= html.Length || html[startIndex] != '{')
        {
            return null;
        }

        var depth = 0;
        var inString = false;
        var escape = false;

        for (var i = startIndex; i < html.Length; i++)
        {
            var c = html[i];

            if (escape)
            {
                escape = false;
                continue;
            }

            if (c == '\\' && inString)
            {
                escape = true;
                continue;
            }

            if (c == '"')
            {
                inString = !inString;
                continue;
            }

            if (inString)
            {
                continue;
            }

            if (c == '{')
            {
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return html.Substring(startIndex, i - startIndex + 1);
                }
            }
        }

        return null;
    }
}
