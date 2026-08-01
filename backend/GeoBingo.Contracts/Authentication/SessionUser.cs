using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GeoBingo.Contracts.Authentication;

[Description("The public user projection embedded in an authenticated session.")]
public sealed record SessionUser
{
    [JsonPropertyName("handle")]
    [Required]
    [MaxLength(64)]
    [Description("Unique public player handle.")]
    public required string Handle { get; init; }

    [JsonPropertyName("displayName")]
    [Required]
    [MaxLength(64)]
    [Description("Current display name snapshot.")]
    public required string DisplayName { get; init; }

    [JsonPropertyName("avatarUrl")]
    [Url]
    [Description("URL of the player's avatar image.")]
    public string? AvatarUrl { get; init; }
}
