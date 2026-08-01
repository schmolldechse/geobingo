using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.Lobbies;
using Microsoft.Extensions.Logging;

namespace GeoBingo.Api.Lobbies.Runtime;

internal enum LobbyOperationDisposition
{
    APPLIED,
    IGNORED,
    REJECTED
}

internal sealed record LobbyOperationFailure(
    ErrorCode Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Errors);

internal sealed record LobbyOperationCompletion(
    LobbyOperationDisposition Disposition,
    long StateVersion,
    LobbyOperationFailure? Failure);

internal sealed record LobbyMutationOutcome
{
    private LobbyMutationOutcome(
        LobbyOperationDisposition disposition,
        LobbyOperationFailure? failure)
    {
        Disposition = disposition;
        Failure = failure;
    }

    public LobbyOperationDisposition Disposition { get; }

    public LobbyOperationFailure? Failure { get; }

    public static LobbyMutationOutcome Applied { get; } =
        new(LobbyOperationDisposition.APPLIED, null);

    public static LobbyMutationOutcome Ignored { get; } =
        new(LobbyOperationDisposition.IGNORED, null);

    public static LobbyMutationOutcome Rejected(
        LobbyOperationFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new LobbyMutationOutcome(
            LobbyOperationDisposition.REJECTED,
            failure);
    }
}

public sealed class LobbyRuntime : IAsyncDisposable
{
    private readonly object lifecycleSyncRoot = new();
    private readonly LobbyRuntimeState state;
    private readonly LobbyProjectionPublisher projectionPublisher;
    private readonly LobbyOperationQueue operationQueue;
    private readonly LobbyDeadlineScheduler deadlineScheduler;
    private readonly ILogger<LobbyRuntime> logger;
    private readonly Func<
        LobbyRuntime,
        LobbyEndedReason,
        long,
        CancellationToken,
        ValueTask> closedCallback;
    private LobbyRuntimeSummary summary;
    private Task? completionTask;
    private Task? disposalTask;

    internal LobbyRuntime(
        LobbyRuntimeState state,
        LobbyProjectionPublisher projectionPublisher,
        TimeProvider timeProvider,
        ILogger<LobbyRuntime> logger,
        Func<
            LobbyRuntime,
            LobbyEndedReason,
            long,
            CancellationToken,
            ValueTask> closedCallback)
    {
        this.state = state ?? throw new ArgumentNullException(nameof(state));
        this.projectionPublisher = projectionPublisher ?? throw new ArgumentNullException(nameof(projectionPublisher));

        ArgumentNullException.ThrowIfNull(timeProvider);

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.closedCallback = closedCallback ?? throw new ArgumentNullException(nameof(closedCallback));

        operationQueue = new LobbyOperationQueue();
        deadlineScheduler = new LobbyDeadlineScheduler(timeProvider, logger);
        summary = state.CreateSummary();
    }

    public Guid LobbyId => state.LobbyId;

    public string Code => state.Code;

    internal LobbyRuntimeSummary ReadSummary() => Volatile.Read(ref summary);

    internal ValueTask<LobbyOperationCompletion> EnqueueMutationAsync(
        string operationName,
        LobbyActor actor,
        Func<LobbyRuntimeState, LobbyActor, CancellationToken,
            ValueTask<LobbyMutationOutcome>> mutation,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(mutation);

        return operationQueue.EnqueueAsync(
            runtimeCancellationToken => ExecuteMutationAsync(
                operationName,
                actor,
                mutation,
                runtimeCancellationToken),
            cancellationToken);
    }

    internal ValueTask<TResult> EnqueueReadAsync<TResult>(
        Func<LobbyRuntimeState, CancellationToken, ValueTask<TResult>> read,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(read);
        return operationQueue.EnqueueAsync(
            runtimeCancellationToken =>
                read(state, runtimeCancellationToken),
            cancellationToken);
    }

    internal void ScheduleDeadline(
        LobbyDeadlineKey key,
        DateTimeOffset deadline,
        Func<LobbyRuntimeState, LobbyDeadlineKey, CancellationToken,
            ValueTask<LobbyMutationOutcome>> mutation)
    {
        ArgumentNullException.ThrowIfNull(mutation);

        deadlineScheduler.Schedule(
            key,
            deadline,
            async (invocation, _) =>
            {
                await EnqueueMutationAsync(
                        $"deadline:{invocation.Key.Kind}",
                        LobbyActor.System,
                        (runtimeState, _, runtimeCancellationToken) =>
                        {
                            if (!deadlineScheduler.IsCurrent(invocation))
                            {
                                return ValueTask.FromResult(
                                    LobbyMutationOutcome.Ignored);
                            }

                            return mutation(
                                runtimeState,
                                invocation.Key,
                                runtimeCancellationToken);
                        },
                        CancellationToken.None)
                    .ConfigureAwait(false);
            });
    }

    internal bool CancelDeadline(LobbyDeadlineKey key) =>
        deadlineScheduler.Cancel(key);

    public void CancelAllDeadlines() =>
        deadlineScheduler.CancelAll();

    public ValueTask CompleteAsync()
    {
        lock (lifecycleSyncRoot)
        {
            completionTask ??= CompleteCoreAsync();
            return new ValueTask(completionTask);
        }
    }

    public ValueTask DisposeAsync()
    {
        lock (lifecycleSyncRoot)
        {
            disposalTask ??= DisposeCoreAsync();
            return new ValueTask(disposalTask);
        }
    }

    private async ValueTask<LobbyOperationCompletion> ExecuteMutationAsync(
        string operationName,
        LobbyActor actor,
        Func<LobbyRuntimeState, LobbyActor, CancellationToken,
            ValueTask<LobbyMutationOutcome>> mutation,
        CancellationToken cancellationToken)
    {
        using var loggingScope = logger.BeginScope(
            "Lobby operation {OperationName}",
            operationName);

        var actorFailure = ValidateActor(actor);
        if (actorFailure is not null)
        {
            return Rejected(actorFailure);
        }

        if (state.IsClosed || state.IsClosing && !actor.IsSystem)
        {
            return Rejected(
                new LobbyOperationFailure(
                    ErrorCode.LOBBY_CLOSED,
                    "The lobby is closed.",
                    Errors: null));
        }

        var outcome = await mutation(state, actor, cancellationToken)
            .ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(outcome);
        ValidateOutcome(outcome, actor);

        if (outcome.Disposition == LobbyOperationDisposition.REJECTED)
        {
            return Rejected(outcome.Failure!);
        }

        if (outcome.Disposition == LobbyOperationDisposition.IGNORED)
        {
            return new LobbyOperationCompletion(
                LobbyOperationDisposition.IGNORED,
                state.StateVersion,
                Failure: null);
        }

        if (state.Members.Count == 0 && !state.IsClosing)
        {
            state.BeginClosing(LobbyEndedReason.NO_MEMBERS);
        }

        if (state.IsClosing && !state.IsClosed)
        {
            deadlineScheduler.CancelAll();
            state.ClearSensitiveState();
            state.MarkClosed();
        }

        state.IncrementStateVersion();
        Volatile.Write(ref summary, state.CreateSummary());

        try
        {
            await projectionPublisher
                .PublishAsync(state, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException exception)
            when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                exception,
                "Lobby projection publication was canceled after operation {OperationName} committed state version {StateVersion}",
                operationName,
                state.StateVersion);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Lobby projection publication failed after operation {OperationName} committed state version {StateVersion}",
                operationName,
                state.StateVersion);
        }

        if (state.IsClosed
            && state.CloseReason is LobbyEndedReason closeReason)
        {
            try
            {
                await closedCallback(
                        this,
                        closeReason,
                        state.StateVersion,
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Lobby termination callback failed for state version {StateVersion}",
                    state.StateVersion);
            }
        }

        return new LobbyOperationCompletion(
            LobbyOperationDisposition.APPLIED,
            state.StateVersion,
            Failure: null);
    }

    private LobbyOperationCompletion Rejected(
        LobbyOperationFailure failure) =>
        new(
            LobbyOperationDisposition.REJECTED,
            state.StateVersion,
            failure);

    private static LobbyOperationFailure? ValidateActor(LobbyActor actor)
    {
        if (actor.IsSystem)
        {
            return actor.UserId is null && actor.ConnectionId is null
                ? null
                : new LobbyOperationFailure(
                    ErrorCode.FORBIDDEN,
                    "The system actor context is invalid.",
                    Errors: null);
        }

        return actor.UserId is not null
            && actor.UserId != Guid.Empty
            && !string.IsNullOrWhiteSpace(actor.ConnectionId)
                ? null
                : new LobbyOperationFailure(
                    ErrorCode.AUTH_REQUIRED,
                    "An authenticated lobby actor is required.",
                    Errors: null);
    }

    private static void ValidateOutcome(
        LobbyMutationOutcome outcome,
        LobbyActor actor)
    {
        if (outcome.Disposition == LobbyOperationDisposition.REJECTED
            && outcome.Failure is null)
        {
            throw new InvalidOperationException(
                "A rejected lobby operation requires a failure.");
        }

        if (outcome.Disposition != LobbyOperationDisposition.REJECTED
            && outcome.Failure is not null)
        {
            throw new InvalidOperationException(
                "An accepted lobby operation cannot contain a failure.");
        }

        if (outcome.Disposition == LobbyOperationDisposition.IGNORED
            && !actor.IsSystem)
        {
            throw new InvalidOperationException(
                "Only a trusted system operation may be ignored.");
        }

    }

    private async Task CompleteCoreAsync()
    {
        if (!ReadSummary().IsClosed)
        {
            throw new InvalidOperationException(
                "A lobby runtime can only complete after the lobby is closed.");
        }

        deadlineScheduler.CancelAll();
        await operationQueue.CompleteAsync().ConfigureAwait(false);
        await deadlineScheduler.DisposeAsync().ConfigureAwait(false);
    }

    private async Task DisposeCoreAsync()
    {
        deadlineScheduler.CancelAll();
        await deadlineScheduler.DisposeAsync().ConfigureAwait(false);
        await operationQueue.DisposeAsync().ConfigureAwait(false);
    }
}
