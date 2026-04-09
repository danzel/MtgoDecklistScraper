using System.Text.Json.Serialization;

namespace MtgoDecklistModels;

public class MtgoDeck
{
    [JsonPropertyName("player")]
    public string? Player { get; set; }

    [JsonPropertyName("loginid")]
    public string? LoginId { get; set; }

    [JsonPropertyName("instance_id")]
    public string? InstanceId { get; set; }

    [JsonPropertyName("main_deck")]
    public List<MtgoCard> MainDeck { get; set; } = [];

    [JsonPropertyName("sideboard_deck")]
    public List<MtgoCard> SideboardDeck { get; set; } = [];

    [JsonPropertyName("wins")]
    public WinLossRecord? Wins { get; set; }

    // Tournament-specific fields
    [JsonPropertyName("tournamentid")]
    public string? TournamentId { get; set; }

    [JsonPropertyName("decktournamentid")]
    public string? DeckTournamentId { get; set; }

    // League-specific fields
    [JsonPropertyName("loginplayeventcourseid")]
    public string? LoginPlayEventCourseId { get; set; }
}
