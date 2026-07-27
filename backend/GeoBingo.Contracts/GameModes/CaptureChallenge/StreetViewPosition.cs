using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.GameModes.CaptureChallenge;

[TranspilationSource]
[Description("A Google Street View panorama position used by GeoBingo.")]
public sealed record StreetViewPosition
{
    [JsonPropertyName("panoramaId")]
    [Required]
    [MaxLength(512)]
    [Description("The Google Street View panorama identifier.")]
    public required string PanoramaId { get; init; }

    [JsonPropertyName("latitude")]
    [Range(-90d, 90d)]
    [Description("Latitude in decimal degrees.")]
    public required double Latitude { get; init; }

    [JsonPropertyName("longitude")]
    [Range(-180d, 180d)]
    [Description("Longitude in decimal degrees.")]
    public required double Longitude { get; init; }

    [JsonPropertyName("heading")]
    [Description("Normalized heading in the half-open range [0, 360).")]
    public required double Heading { get; init; }

    [JsonPropertyName("pitch")]
    [Range(-90d, 90d)]
    [Description("Vertical pitch in degrees.")]
    public required double Pitch { get; init; }

    [JsonPropertyName("zoom")]
    [Range(0d, 5d)]
    [Description("Application-constrained Street View zoom value.")]
    public required double Zoom { get; init; }
}
