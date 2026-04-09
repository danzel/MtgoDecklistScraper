using System.Text.Json.Serialization;

namespace MtgoDecklistModels;

public class WinLossRecord
{
    [JsonPropertyName("wins")]
    public string? Wins { get; set; }

    [JsonPropertyName("losses")]
    public string? Losses { get; set; }
}
