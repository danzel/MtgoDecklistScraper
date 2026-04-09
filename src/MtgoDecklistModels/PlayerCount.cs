using System.Text.Json.Serialization;

namespace MtgoDecklistModels;

public class PlayerCount
{
    [JsonPropertyName("players")]
    public string? Players { get; set; }
}
