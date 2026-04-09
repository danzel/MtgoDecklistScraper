using System.Text.Json.Serialization;

namespace MtgoDecklistModels;

public class MtgoCard
{
    [JsonPropertyName("qty")]
    public string? Qty { get; set; }

    [JsonPropertyName("sideboard")]
    public string? Sideboard { get; set; }

    [JsonPropertyName("docid")]
    public string? DocId { get; set; }

    [JsonPropertyName("card_attributes")]
    public CardAttributes? CardAttributes { get; set; }
}
