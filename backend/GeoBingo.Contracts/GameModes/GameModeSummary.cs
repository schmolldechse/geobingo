using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.GameModes;

[TranspilationSource]
[Description("Public metadata for a registered GeoBingo game mode.")]
public sealed record GameModeSummary
{
    [JsonPropertyName("key")]
    [Required]
    [MaxLength(64)]
    [RegularExpression("^[a-z][a-z0-9_]*$")]
    [Description("The stable lower-case game-mode key.")]
    public required string Key { get; init; }

    [JsonPropertyName("displayName")]
    [Required]
    [MaxLength(64)]
    [Description("The human-readable game-mode name.")]
    public required string DisplayName { get; init; }

    [JsonPropertyName("description")]
    [Required]
    [MaxLength(500)]
    [Description("A concise explanation of the game mode.")]
    public required string Description { get; init; }

    [JsonPropertyName("version")]
    [Required]
    [MaxLength(32)]
    [Description("The normalized public game-mode version.")]
    public required string Version { get; init; }

    [JsonPropertyName("order")]
    [Description("The explicit stable display and default-selection order.")]
    public required int Order { get; init; }
}
