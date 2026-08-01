using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GeoBingo.Contracts.Authentication;

[Description("An external authentication provider.")]
public sealed record AuthProvider
{
    [JsonPropertyName("key")]
    [MaxLength(64)]
    [Required]
    [Description("Stable lower-case authentication provider key.")]
    public required string Key { get; init; }

    [JsonPropertyName("displayName")]
    [MaxLength(64)]
    [Required]
    [Description("Human-readable provider name.")]
    public required string DisplayName { get; init; }
}
