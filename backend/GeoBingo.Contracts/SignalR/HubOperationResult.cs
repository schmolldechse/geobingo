using System.ComponentModel;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.SignalR;

[TranspilationSource]
[Description("The uniform result returned by every client-to-server hub operation.")]
public sealed record HubOperationResult
{
    [JsonPropertyName("success")]
    [Description("Whether the operation was successful.")]
    public bool Success { get; init; }

    [JsonPropertyName("stateVersion")]
    [Description("The latest lobby state version when one is available.")]
    public long? StateVersion { get; init; }

    [JsonPropertyName("error")]
    [Description("The error details if the operation failed.")]
    public SignalRError? Error { get; init; }
}
