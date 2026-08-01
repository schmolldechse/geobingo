using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.GameModes.CaptureChallenge;

[TranspilationSource]
[Description("Casts the caller's first vote for the current capture.")]
public sealed record CastVoteRequest
{
    [JsonPropertyName("captureId")]
    [Description("The capture currently shown to every voter.")]
    public required Guid CaptureId { get; init; }

    [JsonPropertyName("value")]
    [Description("The selected vote value.")]
    public required VoteValue Value { get; init; }
}

[TranspilationSource]
[Description("Changes the caller's vote for the current capture.")]
public sealed record ChangeVoteRequest
{
    [JsonPropertyName("captureId")]
    [Description("The capture currently shown to every voter.")]
    public required Guid CaptureId { get; init; }

    [JsonPropertyName("value")]
    [Description("The replacement vote value.")]
    public required VoteValue Value { get; init; }
}
