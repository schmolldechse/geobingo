using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.Lobbies;

[TranspilationSource]
[Description("A transient game-mode notification; snapshots remain authoritative.")]
public sealed record GameModeEvent
{
    [JsonPropertyName("type")]
    [Description("The finite event type.")]
    public required GameModeEventType Type { get; init; }

    [JsonPropertyName("stateVersion")]
    [Description("The lobby state version associated with the event.")]
    public required long StateVersion { get; init; }

    [JsonPropertyName("occurredAt")]
    [Description("The event instant expressed with the application timezone offset.")]
    public required DateTimeOffset OccurredAt { get; init; }
}
