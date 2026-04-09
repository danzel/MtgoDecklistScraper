using System.IO.Abstractions;
using System.Text.Json;
using System.Text.RegularExpressions;
using MtgoDecklistModels;

namespace MtgoDecklistScraperNet.Services;

public partial class EventSaver
{
    private readonly IFileSystem _fileSystem;
    private readonly string _outputRoot;

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    [GeneratedRegex(@"(\d{4})-(\d{2})-\d{2}")]
    private static partial Regex DateInSlugRegex();

    public EventSaver(IFileSystem fileSystem, string outputRoot)
    {
        _fileSystem = fileSystem;
        _outputRoot = outputRoot;
    }

    public async Task SaveEventAsync(string relativeUrl, MtgoEvent mtgoEvent, CancellationToken ct = default)
    {
        var (year, month, filename) = ParseOutputPath(relativeUrl);
        var dir = _fileSystem.Path.Combine(_outputRoot, year, month);
        _fileSystem.Directory.CreateDirectory(dir);
        var filePath = _fileSystem.Path.Combine(dir, filename + ".json");
        var json = JsonSerializer.Serialize(mtgoEvent, WriteOptions);
        await _fileSystem.File.WriteAllTextAsync(filePath, json, ct);
    }

    public static (string Year, string Month, string Filename) ParseOutputPath(string relativeUrl)
    {
        // Strip /decklist/ prefix
        var slug = relativeUrl;
        if (slug.StartsWith("/decklist/", StringComparison.OrdinalIgnoreCase))
        {
            slug = slug["/decklist/".Length..];
        }

        var dateMatch = DateInSlugRegex().Match(slug);
        if (dateMatch.Success)
        {
            return (dateMatch.Groups[1].Value, dateMatch.Groups[2].Value, slug);
        }

        // Fallback: use current date
        var now = DateTime.UtcNow;
        return (now.Year.ToString(), now.Month.ToString("D2"), slug);
    }
}
