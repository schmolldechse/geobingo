using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.Lobbies;

[TranspilationSource]
[Description("Requests membership in the lobby identified by an eight-character code.")]
public sealed record JoinLobbyRequest
{
    [JsonPropertyName("code")]
    [Required]
    [MinLength(8)]
    [MaxLength(8)]
    [RegularExpression("^[A-Z0-9]{8}$")]
    [Description("The normalized eight-character lobby code.")]
    public required string Code { get; init; }
}

[TranspilationSource]
[Description("Removes an active member from the current lobby.")]
public sealed record RemovePlayerRequest
{
    [JsonPropertyName("userId")]
    [Description("The member to remove.")]
    public required Guid UserId { get; init; }

    [JsonPropertyName("kind")]
    [Description("Whether the member is kicked or banned.")]
    public required PlayerRemovalKind Kind { get; init; }
}

[TranspilationSource]
[Description("Transfers host ownership to another active member.")]
public sealed record TransferHostRequest
{
    [JsonPropertyName("userId")]
    [Description("The new host user identifier.")]
    public required Guid UserId { get; init; }
}

[TranspilationSource]
[Description("Selects a registered game mode for the current lobby.")]
public sealed record SelectGameModeRequest
{
    [JsonPropertyName("modeKey")]
    [Required]
    [MaxLength(64)]
    [RegularExpression("^[a-z][a-z0-9_]*$")]
    [Description("The stable lower-case registered mode key.")]
    public required string ModeKey { get; init; }

    [JsonPropertyName("confirmModeStateReset")]
    [Description("Confirms replacement of non-default waiting configuration.")]
    public bool ConfirmModeStateReset { get; init; }
}

[TranspilationSource]
[Description("Replaces the general settings of the current lobby.")]
public sealed record UpdateLobbySettingsRequest
{
    [JsonPropertyName("settings")]
    [Required]
    [Description("The complete replacement lobby settings.")]
    public required LobbySettings Settings { get; init; }
}

[TranspilationSource]
[Description("Selects which result projection should be sent to the caller.")]
public sealed record RequestResultsRequest
{
    [JsonPropertyName("scope")]
    [Description("The requested result scope.")]
    public required ResultsScope Scope { get; init; }

    [JsonPropertyName("roundId")]
    [Description("The round identifier required only for SPECIFIC_ROUND.")]
    public Guid? RoundId { get; init; }
}
