using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.GameModes.CaptureChallenge;

[TranspilationSource]
[Description("Casts the caller's first vote for the current assignment.")]
public sealed record CastVoteRequest
{
    [JsonPropertyName("assignmentId")]
    [Description("The voting assignment currently shown to the caller.")]
    public required Guid AssignmentId { get; init; }

    [JsonPropertyName("value")]
    [Description("The selected vote value.")]
    public required VoteValue Value { get; init; }
}

[TranspilationSource]
[Description("Changes the caller's existing vote for a completed assignment.")]
public sealed record ChangeVoteRequest
{
    [JsonPropertyName("assignmentId")]
    [Description("The previously completed voting assignment.")]
    public required Guid AssignmentId { get; init; }

    [JsonPropertyName("value")]
    [Description("The replacement vote value.")]
    public required VoteValue Value { get; init; }
}
