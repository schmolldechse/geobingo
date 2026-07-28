using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using GeoBingo.Contracts.Common;
using Tapper;

namespace GeoBingo.Contracts.SignalR;

[TranspilationSource]
[Description("A stable application error returned by a SignalR operation or event.")]
public sealed record SignalRError
{
    [JsonPropertyName("code")]
    [Description("The machine-readable application error code.")]
    public required ErrorCode Code { get; init; }

    [JsonPropertyName("retryable")]
    [Description("Whether retrying after refreshing state may be meaningful.")]
    public bool Retryable { get; init; }

    [JsonPropertyName("details")]
    [Required]
    [MaxLength(500)]
    [Description("A safe human-readable summary.")]
    public required string Details { get; init; }

    [JsonPropertyName("currentStateVersion")]
    [Description("The latest lobby state version when the lobby is still available.")]
    public long? CurrentStateVersion { get; init; }

    [JsonPropertyName("correlationId")]
    [Required]
    [MaxLength(64)]
    [Description("The operation correlation identifier.")]
    public required string CorrelationId { get; init; }

    [JsonPropertyName("traceId")]
    [Required]
    [MaxLength(32)]
    [Description("The current distributed-trace identifier.")]
    public required string TraceId { get; init; }

    [JsonPropertyName("errors")]
    [Description("Field-specific validation messages keyed by JSON property path.")]
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }
}
