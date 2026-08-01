using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.Lobbies;

[TranspilationSource]
[Description("Signals that an in-memory lobby membership or lobby permanently ended.")]
public sealed record LobbyEnded
{
    [JsonPropertyName("lobbyId")]
    [Description("The lobby identifier.")]
    public required Guid LobbyId { get; init; }

    [JsonPropertyName("reason")]
    [Description("The permanent termination reason.")]
    public required LobbyEndedReason Reason { get; init; }

    [JsonPropertyName("stateVersion")]
    [Description("The last known lobby state version.")]
    public required long StateVersion { get; init; }
}
