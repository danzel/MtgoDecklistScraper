using System.Text.Json.Serialization;

namespace MtgoDecklistModels;

public class MtgoEvent
{
    [JsonPropertyName("decklists")]
    public List<MtgoDeck> Decklists { get; set; } = [];

    // League fields
    [JsonPropertyName("playeventid")]
    public string? PlayEventId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("publish_date")]
    public string? PublishDate { get; set; }

    [JsonPropertyName("instance_id")]
    public string? InstanceId { get; set; }

    [JsonPropertyName("site_name")]
    public string? SiteName { get; set; }

    // Tournament fields
    [JsonPropertyName("event_id")]
    public string? EventId { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("starttime")]
    public string? StartTime { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("inplayoffs")]
    public string? InPlayoffs { get; set; }

    // Shared
    [JsonPropertyName("final_rank")]
    public List<FinalRankEntry>? FinalRank { get; set; }

    [JsonPropertyName("player_count")]
    public PlayerCount? PlayerCount { get; set; }

    /// <summary>
    /// Gets the event name from whichever field is populated (Description for tournaments, Name for leagues).
    /// </summary>
    [JsonIgnore]
    public string? EventName => Description ?? Name;
}
