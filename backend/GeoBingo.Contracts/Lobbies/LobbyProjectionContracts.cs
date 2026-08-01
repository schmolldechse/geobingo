using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using GeoBingo.Contracts.GameModes;
using GeoBingo.Contracts.GameModes.CaptureChallenge;
using Tapper;

namespace GeoBingo.Contracts.Lobbies;

[TranspilationSource]
public sealed record LobbyMemberView
{
    [JsonPropertyName("userId")]
    public required Guid UserId { get; init; }

    [JsonPropertyName("handle")]
    public required string Handle { get; init; }

    [JsonPropertyName("displayName")]
    public required string DisplayName { get; init; }

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; init; }

    [JsonPropertyName("joinOrder")]
    public required int JoinOrder { get; init; }

    [JsonPropertyName("connected")]
    public required bool Connected { get; init; }
}

[TranspilationSource]
public sealed record CurrentRoundView
{
    [JsonPropertyName("roundId")]
    public required Guid RoundId { get; init; }

    [JsonPropertyName("roundNumber")]
    public required int RoundNumber { get; init; }
}

[TranspilationSource]
public sealed record GameModePublicProjection
{
    [JsonPropertyName("modeKey")]
    public required string ModeKey { get; init; }

    [JsonPropertyName("modeVersion")]
    public required string ModeVersion { get; init; }

    [JsonPropertyName("captureChallenge")]
    public CaptureChallengePublicProjection? CaptureChallenge { get; init; }
}

[TranspilationSource]
public sealed record PersonalProjection
{
    [JsonPropertyName("lobbyId")]
    public required Guid LobbyId { get; init; }

    [JsonPropertyName("userId")]
    public required Guid UserId { get; init; }

    [JsonPropertyName("stateVersion")]
    public required long StateVersion { get; init; }

    [JsonPropertyName("captureChallenge")]
    public CaptureChallengePersonalProjection? CaptureChallenge { get; init; }
}

[TranspilationSource]
[Description("The atomic public and member-specific projection for one lobby member.")]
public sealed record LobbyProjection
{
    [JsonPropertyName("snapshot")]
    public required LobbySnapshot Snapshot { get; init; }

    [JsonPropertyName("personalProjection")]
    public required PersonalProjection PersonalProjection { get; init; }
}

[TranspilationSource]
[Description("The authoritative public lobby projection.")]
public sealed record LobbySnapshot
{
    [JsonPropertyName("lobbyId")]
    public required Guid LobbyId { get; init; }

    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("stateVersion")]
    public required long StateVersion { get; init; }

    [JsonPropertyName("status")]
    public required LobbyStatus Status { get; init; }

    [JsonPropertyName("preparingEndsAt")]
    public DateTimeOffset? PreparingEndsAt { get; init; }

    [JsonPropertyName("hostUserId")]
    public required Guid HostUserId { get; init; }

    [JsonPropertyName("members")]
    public required IReadOnlyList<LobbyMemberView> Members { get; init; }

    [JsonPropertyName("settings")]
    public required LobbySettings Settings { get; init; }

    [JsonPropertyName("selectedMode")]
    public required GameModeSummary SelectedMode { get; init; }

    [JsonPropertyName("currentRound")]
    public CurrentRoundView? CurrentRound { get; init; }

    [JsonPropertyName("gameMode")]
    public required GameModePublicProjection GameMode { get; init; }

    [JsonPropertyName("completedRoundSummaries")]
    public required IReadOnlyList<CompletedRoundSummary> CompletedRoundSummaries { get; init; }

    [JsonPropertyName("lastCompletedRoundResults")]
    public CompletedRoundResults? LastCompletedRoundResults { get; init; }

    [JsonPropertyName("cumulativeResults")]
    public required IReadOnlyList<CumulativePlayerResult> CumulativeResults { get; init; }
}
