using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.Lobbies;

[TranspilationSource]
[Description("General settings of an in-memory lobby.")]
public sealed record LobbySettings
{
    [JsonPropertyName("maxPlayers")]
    [Range(2, 32)]
    [Description("The maximum number of active lobby members.")]
    public int MaxPlayers { get; init; } = 12;
}
