using System.Text.Json.Serialization;

namespace MtgoDecklistModels;

public class CardAttributes
{
    [JsonPropertyName("digitalobjectcatalogid")]
    public string? DigitalObjectCatalogId { get; set; }

    [JsonPropertyName("card_name")]
    public string? CardName { get; set; }

    [JsonPropertyName("cost")]
    public string? Cost { get; set; }

    [JsonPropertyName("rarity")]
    public string? Rarity { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("cardset")]
    public string? CardSet { get; set; }

    [JsonPropertyName("card_type")]
    public string? CardType { get; set; }

    [JsonPropertyName("colors")]
    public List<string>? Colors { get; set; }
}
