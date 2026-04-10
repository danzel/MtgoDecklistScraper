using System.IO.Abstractions;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using MtgoDecklistModels;

namespace MtgoDecklistScraperNet.Services;

public partial class EventSaver
{
    private readonly IFileSystem _fileSystem;
    private readonly string _outputRoot;
    private readonly ILogger<EventSaver> _logger;

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    [GeneratedRegex(@"(\d{4})-(\d{2})-\d{2}")]
    private static partial Regex DateInSlugRegex();

    public EventSaver(IFileSystem fileSystem, string outputRoot, ILogger<EventSaver> logger)
    {
        _fileSystem = fileSystem;
        _outputRoot = outputRoot;
        _logger = logger;
    }

    public bool EventExists(string relativeUrl)
    {
        var (year, month, filename) = ParseOutputPath(relativeUrl);
        var filePath = _fileSystem.Path.Combine(_outputRoot, year, month, filename + ".json");
        return _fileSystem.File.Exists(filePath);
    }

    public async Task SaveEventAsync(string relativeUrl, MtgoEvent mtgoEvent, CancellationToken ct = default)
    {
        var (year, month, filename) = ParseOutputPath(relativeUrl);
        var dir = _fileSystem.Path.Combine(_outputRoot, year, month);
        _fileSystem.Directory.CreateDirectory(dir);
        var filePath = _fileSystem.Path.Combine(dir, filename + ".json");
        var json = JsonSerializer.Serialize(mtgoEvent, WriteOptions);
        await _fileSystem.File.WriteAllTextAsync(filePath, json, ct);
        _logger.LogInformation("Saved event {RelativeUrl}", relativeUrl);
    }

    public void WriteIndexFiles()
    {
        if (!_fileSystem.Directory.Exists(_outputRoot))
        {
            return;
        }

        var months = new List<string>();

        foreach (var yearDir in _fileSystem.Directory.GetDirectories(_outputRoot).Order())
        {
            var yearName = _fileSystem.Path.GetFileName(yearDir);
            foreach (var monthDir in _fileSystem.Directory.GetDirectories(yearDir).Order())
            {
                var monthName = _fileSystem.Path.GetFileName(monthDir);
                var eventFiles = _fileSystem.Directory.GetFiles(monthDir, "*.json")
                    .Select(f => _fileSystem.Path.GetFileName(f))
                    .Where(f => f != "events.json")
                    .Order()
                    .ToList();

                if (eventFiles.Count == 0)
                {
                    continue;
                }

                months.Add($"{yearName}/{monthName}");

                var eventsJsonPath = _fileSystem.Path.Combine(monthDir, "events.json");
                var eventsJson = JsonSerializer.Serialize(eventFiles, WriteOptions);
                _fileSystem.File.WriteAllText(eventsJsonPath, eventsJson);
            }
        }

        var monthsJsonPath = _fileSystem.Path.Combine(_outputRoot, "months.json");
        var monthsJson = JsonSerializer.Serialize(months, WriteOptions);
        _fileSystem.File.WriteAllText(monthsJsonPath, monthsJson);

        _logger.LogInformation("Updated index files: {MonthCount} months", months.Count);
    }

    public static (string Year, string Month, string Filename) ParseOutputPath(string relativeUrl)
    {
        var slug = relativeUrl;
        if (slug.StartsWith("/decklist/", StringComparison.OrdinalIgnoreCase))
        {
            slug = slug["/decklist/".Length..];
        }

        var dateMatch = DateInSlugRegex().Match(slug);
        if (!dateMatch.Success)
        {
            throw new ArgumentException($"Could not extract date from URL: {relativeUrl}", nameof(relativeUrl));
        }

        return (dateMatch.Groups[1].Value, dateMatch.Groups[2].Value, slug);
    }
}
