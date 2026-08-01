using System.ComponentModel;
using System.Text.Json.Serialization;

namespace GeoBingo.Contracts.Authentication;

[Description("The current GeoBingo authentication session.")]
public sealed record Session
{
    [JsonPropertyName("authenticated")]
    [Description("Whether the GeoBingo authentication cookie is valid.")]
    public required bool Authenticated { get; init; }

    [JsonPropertyName("user")]
    [Description("Authenticated public user information.")]
    public SessionUser? User { get; init; }
}
