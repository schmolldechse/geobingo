using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tapper;

namespace GeoBingo.Contracts.SignalR;

[TranspilationSource]
[Description("A stable application error returned by a SignalR operation or event.")]
public sealed record SignalRError
{
    [JsonPropertyName("retryable")]
    [Description("Whether retrying after refreshing state may be meaningful.")]
    public bool Retryable { get; init; }

    [JsonPropertyName("details")]
    [Required]
    [Description("A safe human-readable summary.")]
    public required string Details { get; init; }

    [JsonPropertyName("correlationId")]
    [Required]
    [Description("The operation correlation identifier.")]
    public required string CorrelationId { get; init; }
}
