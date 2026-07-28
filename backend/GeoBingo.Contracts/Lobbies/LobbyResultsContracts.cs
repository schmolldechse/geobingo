using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.Lobbies;

[TranspilationSource]
public sealed record CaptureResult
{
    [JsonPropertyName("captureId")]
    public required Guid CaptureId { get; init; }

    [JsonPropertyName("goalId")]
    public required Guid GoalId { get; init; }

    [JsonPropertyName("ownerUserId")]
    public required Guid OwnerUserId { get; init; }

    [JsonPropertyName("good")]
    public required int Good { get; init; }

    [JsonPropertyName("bad")]
    public required int Bad { get; init; }

    [JsonPropertyName("eligible")]
    public required int Eligible { get; init; }

    [JsonPropertyName("scoreFactor")]
    public required decimal ScoreFactor { get; init; }

    [JsonPropertyName("score")]
    public required decimal Score { get; init; }

    [JsonPropertyName("noEligibleVoters")]
    public required bool NoEligibleVoters { get; init; }
}

[TranspilationSource]
public sealed record PlayerRoundResult
{
    [JsonPropertyName("userId")]
    public required Guid UserId { get; init; }

    [JsonPropertyName("score")]
    public required decimal Score { get; init; }

    [JsonPropertyName("rank")]
    public required int Rank { get; init; }
}

[TranspilationSource]
public sealed record CompletedRoundResults
{
    [JsonPropertyName("roundId")]
    public required Guid RoundId { get; init; }

    [JsonPropertyName("roundNumber")]
    public required int RoundNumber { get; init; }

    [JsonPropertyName("completedAt")]
    public required DateTimeOffset CompletedAt { get; init; }

    [JsonPropertyName("captures")]
    public required IReadOnlyList<CaptureResult> Captures { get; init; }

    [JsonPropertyName("players")]
    public required IReadOnlyList<PlayerRoundResult> Players { get; init; }
}

[TranspilationSource]
public sealed record CompletedRoundSummary
{
    [JsonPropertyName("roundId")]
    public required Guid RoundId { get; init; }

    [JsonPropertyName("roundNumber")]
    public required int RoundNumber { get; init; }

    [JsonPropertyName("completedAt")]
    public required DateTimeOffset CompletedAt { get; init; }
}

[TranspilationSource]
public sealed record CumulativePlayerResult
{
    [JsonPropertyName("userId")]
    public required Guid UserId { get; init; }

    [JsonPropertyName("score")]
    public required decimal Score { get; init; }

    [JsonPropertyName("rank")]
    public required int Rank { get; init; }
}

[TranspilationSource]
[Description("A member-authorized result projection for the active lobby runtime.")]
public sealed record LobbyResultsView
{
    [JsonPropertyName("lobbyId")]
    public required Guid LobbyId { get; init; }

    [JsonPropertyName("stateVersion")]
    public required long StateVersion { get; init; }

    [JsonPropertyName("scope")]
    public required ResultsScope Scope { get; init; }

    [JsonPropertyName("roundId")]
    public Guid? RoundId { get; init; }

    [JsonPropertyName("roundNumber")]
    public int? RoundNumber { get; init; }

    [JsonPropertyName("completedAt")]
    public DateTimeOffset? CompletedAt { get; init; }

    [JsonPropertyName("roundResults")]
    public CompletedRoundResults? RoundResults { get; init; }

    [JsonPropertyName("cumulativeResults")]
    public required IReadOnlyList<CumulativePlayerResult> CumulativeResults { get; init; }
}
