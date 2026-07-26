using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;

namespace GeoBingo.Observability.Metrics;

public sealed class GameMetrics : IDisposable
{
    private const string LobbyStatusTag = "lobby.status";
    private const string LobbyCloseReasonTag = "lobby.close_reason";
    private const string RoundStatusTag = "round.status";
    private const string HubOperationTag = "hub.operation";
    private const string HubOutcomeTag = "hub.outcome";
    private const string ErrorCodeTag = "error.code";
    private const string GameModeTag = "game.mode";
    private const string PhaseTransitionTag = "phase.transition";

    private readonly Meter meter = new(
        GeoBingoTelemetry.MeterName,
        GeoBingoTelemetry.ServiceVersion);
    private readonly UpDownCounter<long> activeSignalRConnections;
    private readonly UpDownCounter<long> activeLobbies;
    private readonly UpDownCounter<long> activeCaptureChallengeRounds;
    private readonly Counter<long> createdLobbies;
    private readonly Counter<long> closedLobbies;
    private readonly Counter<long> hubOperations;
    private readonly Histogram<double> hubOperationDuration;
    private readonly Counter<long> startedRounds;
    private readonly Counter<long> completedRounds;
    private readonly Histogram<double> phaseTransitionDelay;
    private readonly Counter<long> shutdownClosedLobbies;
    private readonly ObservableGauge<long> currentLobbies;
    private long currentLobbyCount;

    public GameMetrics()
    {
        activeSignalRConnections = meter.CreateUpDownCounter<long>(
            "geobingo.signalr.connections.active",
            unit: "{connection}");
        activeLobbies = meter.CreateUpDownCounter<long>(
            "geobingo.lobbies.active",
            unit: "{lobby}");
        activeCaptureChallengeRounds =
            meter.CreateUpDownCounter<long>(
                "geobingo.capture_challenge.rounds.active",
                unit: "{round}");
        createdLobbies = meter.CreateCounter<long>(
            "geobingo.lobbies.created",
            unit: "{lobby}");
        closedLobbies = meter.CreateCounter<long>(
            "geobingo.lobbies.closed",
            unit: "{lobby}");
        hubOperations = meter.CreateCounter<long>(
            "geobingo.hub.operations",
            unit: "{operation}");
        hubOperationDuration = meter.CreateHistogram<double>(
            "geobingo.hub.operation.duration",
            unit: "s");
        startedRounds = meter.CreateCounter<long>(
            "geobingo.rounds.started",
            unit: "{round}");
        completedRounds = meter.CreateCounter<long>(
            "geobingo.rounds.completed",
            unit: "{round}");
        phaseTransitionDelay = meter.CreateHistogram<double>(
            "geobingo.phase.transition.delay",
            unit: "s");
        shutdownClosedLobbies = meter.CreateCounter<long>(
            "geobingo.shutdown.lobbies.closed",
            unit: "{lobby}");
        currentLobbies = meter.CreateObservableGauge(
            "geobingo.lobbies.current",
            () => Interlocked.Read(ref currentLobbyCount),
            unit: "{lobby}");
    }

    public void SignalRConnectionOpened()
        => activeSignalRConnections.Add(1);

    public void SignalRConnectionClosed()
        => activeSignalRConnections.Add(-1);

    public void LobbyCreated(
        string status,
        long currentLobbyCount)
    {
        var tags = new TagList
        {
            { LobbyStatusTag, status }
        };

        createdLobbies.Add(1, tags);
        activeLobbies.Add(1, tags);
        SetCurrentLobbyCount(currentLobbyCount);
    }

    public void LobbyStatusChanged(
        string previousStatus,
        string currentStatus)
    {
        var previousTags = new TagList
        {
            { LobbyStatusTag, previousStatus }
        };
        var currentTags = new TagList
        {
            { LobbyStatusTag, currentStatus }
        };

        activeLobbies.Add(-1, previousTags);
        activeLobbies.Add(1, currentTags);
    }

    public void LobbyClosed(
        string status,
        string reason,
        long currentLobbyCount)
    {
        var tags = new TagList
        {
            { LobbyStatusTag, status },
            { LobbyCloseReasonTag, reason }
        };
        var statusTags = new TagList
        {
            { LobbyStatusTag, status }
        };

        closedLobbies.Add(1, tags);
        activeLobbies.Add(-1, statusTags);
        SetCurrentLobbyCount(currentLobbyCount);
    }

    public void SetCurrentLobbyCount(long count)
        => Interlocked.Exchange(
            ref currentLobbyCount,
            Math.Max(0, count));

    public void HubOperationCompleted(
        string operationName,
        bool accepted,
        string? errorCode,
        TimeSpan duration)
    {
        var tags = new TagList
        {
            { HubOperationTag, operationName },
            { HubOutcomeTag, accepted ? "accepted" : "rejected" }
        };

        if (!accepted && !string.IsNullOrWhiteSpace(errorCode))
            tags.Add(ErrorCodeTag, errorCode);

        hubOperations.Add(1, tags);
        hubOperationDuration.Record(
            Math.Max(0d, duration.TotalSeconds),
            tags);
    }

    public void RoundStarted(string modeKey)
    {
        var tags = new TagList
        {
            { GameModeTag, modeKey }
        };

        startedRounds.Add(1, tags);
    }

    public void RoundCompleted(string modeKey)
    {
        var tags = new TagList
        {
            { GameModeTag, modeKey }
        };

        completedRounds.Add(1, tags);
    }

    public void CaptureChallengeRoundActivated(string status)
    {
        var tags = new TagList
        {
            { RoundStatusTag, status }
        };

        activeCaptureChallengeRounds.Add(1, tags);
    }

    public void CaptureChallengeStatusChanged(
        string previousStatus,
        string currentStatus)
    {
        var previousTags = new TagList
        {
            { RoundStatusTag, previousStatus }
        };
        var currentTags = new TagList
        {
            { RoundStatusTag, currentStatus }
        };

        activeCaptureChallengeRounds.Add(-1, previousTags);
        activeCaptureChallengeRounds.Add(1, currentTags);
    }

    public void CaptureChallengeRoundDeactivated(string status)
    {
        var tags = new TagList
        {
            { RoundStatusTag, status }
        };

        activeCaptureChallengeRounds.Add(-1, tags);
    }

    public void RecordPhaseTransitionDelay(
        string transitionName,
        TimeSpan delay)
    {
        var tags = new TagList
        {
            { PhaseTransitionTag, transitionName }
        };

        phaseTransitionDelay.Record(
            Math.Max(0d, delay.TotalSeconds),
            tags);
    }

    public void ShutdownLobbiesClosed(long count)
    {
        if (count > 0)
            shutdownClosedLobbies.Add(count);
    }

    public void Dispose()
        => meter.Dispose();
}
