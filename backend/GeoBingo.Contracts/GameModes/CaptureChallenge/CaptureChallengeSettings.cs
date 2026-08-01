using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.GameModes.CaptureChallenge;

[TranspilationSource]
[Description("Settings for Capture Challenge rounds.")]
public sealed record CaptureChallengeSettings
{
    [JsonPropertyName("captureDurationSeconds")]
    [Range(180, 3600)]
    [Description("The duration of the capture phase in seconds.")]
    public required int CaptureDurationSeconds { get; init; }

    [JsonPropertyName("secondsPerVote")]
    [Range(5, 60)]
    [Description("The fixed voting time budget for each presented capture in seconds.")]
    public required int SecondsPerVote { get; init; }
}
