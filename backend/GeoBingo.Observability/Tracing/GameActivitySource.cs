using System;
using System.Diagnostics;

namespace GeoBingo.Observability.Tracing;

public sealed class GameActivitySource : IDisposable
{
    public const string HubOperation = "geobingo.hub.operation";
    public const string LobbyLifecycle = "geobingo.lobby.lifecycle";
    public const string RoundLifecycle = "geobingo.round.lifecycle";
    public const string PhaseTransition = "geobingo.phase.transition";
    public const string Shutdown = "geobingo.shutdown";

    private readonly ActivitySource activitySource = new(GeoBingoTelemetry.ActivitySourceName, GeoBingoTelemetry.ServiceVersion);

    public Activity? StartActivity(
        string activityName,
        ActivityKind kind = ActivityKind.Internal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activityName);
        return activitySource.StartActivity(activityName, kind);
    }

    public void Dispose() => activitySource.Dispose();
}
