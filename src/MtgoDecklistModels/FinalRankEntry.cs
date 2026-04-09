using System.Text.Json.Serialization;

namespace MtgoDecklistModels;

public class FinalRankEntry
{
    [JsonPropertyName("loginid")]
    public string? LoginId { get; set; }

    [JsonPropertyName("rank")]
    public string? Rank { get; set; }
}
