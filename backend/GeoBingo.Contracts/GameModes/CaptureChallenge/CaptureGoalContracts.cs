using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.GameModes.CaptureChallenge;

[TranspilationSource]
[Description("Editable Capture Challenge goal data.")]
public sealed record CaptureGoalInput
{
    [JsonPropertyName("title")]
    [Required]
    [MinLength(1)]
    [MaxLength(80)]
    [Description("The trimmed goal title.")]
    public required string Title { get; init; }

    [JsonPropertyName("description")]
    [MaxLength(500)]
    [Description("An optional goal explanation.")]
    public string? Description { get; init; }

    [JsonPropertyName("scoreFactor")]
    [Range(typeof(decimal), "0", "10")]
    [HalfStep]
    [Description("The optional score factor applied to this goal. An omitted value defaults to 1; explicit values use increments of 0.5.")]
    public decimal? ScoreFactor { get; init; }
}
