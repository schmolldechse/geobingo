using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GeoBingo.Contracts.GameModes;

namespace GeoBingo.Contracts.Lobbies;

[Description("The newly created in-memory lobby.")]
public sealed record CreateLobbyResponse
{
    [JsonPropertyName("lobbyId")]
    [Description("The lobby identifier.")]
    public required Guid LobbyId { get; init; }

    [JsonPropertyName("code")]
    [Required]
    [MinLength(8)]
    [MaxLength(8)]
    [RegularExpression("^[A-Z0-9]{8}$")]
    [Description("The normalized eight-character lobby code.")]
    public required string Code { get; init; }

    [JsonPropertyName("stateVersion")]
    [Description("The initial lobby state version.")]
    public required long StateVersion { get; init; }

    [JsonPropertyName("selectedMode")]
    [Required]
    [Description("The initially selected game mode.")]
    public required GameModeSummary SelectedMode { get; init; }

    [JsonPropertyName("hubPath")]
    [Required]
    [MaxLength(128)]
    [Description("The relative SignalR hub path.")]
    public required string HubPath { get; init; }
}

[Description("Minimal user-specific information for resolving a lobby code.")]
public sealed record ResolveLobbyResponse
{
    [JsonPropertyName("lobbyId")]
    [Description("The lobby identifier.")]
    public required Guid LobbyId { get; init; }

    [JsonPropertyName("code")]
    [Required]
    [MinLength(8)]
    [MaxLength(8)]
    [RegularExpression("^[A-Z0-9]{8}$")]
    [Description("The normalized eight-character lobby code.")]
    public required string Code { get; init; }

    [JsonPropertyName("status")]
    [Description("The current global lobby status.")]
    public required LobbyStatus Status { get; init; }

    [JsonPropertyName("selectedMode")]
    [Required]
    [Description("The selected game-mode metadata.")]
    public required GameModeSummary SelectedMode { get; init; }

    [JsonPropertyName("playerCount")]
    [Description("The current active member count.")]
    public required int PlayerCount { get; init; }

    [JsonPropertyName("maxPlayers")]
    [Description("The configured member capacity.")]
    public required int MaxPlayers { get; init; }

    [JsonPropertyName("joinable")]
    [Description("Whether the current user may create a new membership.")]
    public required bool Joinable { get; init; }

    [JsonPropertyName("reconnectable")]
    [Description("Whether the current user may reconnect an existing membership.")]
    public required bool Reconnectable { get; init; }
}
