using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GeoBingo.Api.Lobbies.Runtime;

internal readonly record struct LobbyDeadlineInvocation(
    LobbyDeadlineKey Key,
    long Generation);

internal sealed class LobbyDeadlineScheduler : IAsyncDisposable
{
    private readonly object syncRoot = new();
    private readonly Dictionary<LobbyDeadlineKey, DeadlineRegistration> registrations = [];
    private readonly HashSet<DeadlineRegistration> activeRegistrations = [];
    private readonly TimeProvider timeProvider;
    private readonly ILogger logger;
    private long nextGeneration;
    private bool disposed;
    private Task? disposalTask;

    public LobbyDeadlineScheduler(
        TimeProvider timeProvider,
        ILogger logger)
    {
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Schedule(
        LobbyDeadlineKey key,
        DateTimeOffset deadline,
        Func<LobbyDeadlineInvocation, CancellationToken, ValueTask>
            enqueueCallback)
    {
        ValidateKey(key);
        ArgumentNullException.ThrowIfNull(enqueueCallback);

        DeadlineRegistration registration;
        DeadlineRegistration? replacedRegistration;
        lock (syncRoot)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            registration = new DeadlineRegistration(
                key,
                checked(++nextGeneration));
            registrations.Remove(key, out replacedRegistration);
            registrations.Add(key, registration);
            activeRegistrations.Add(registration);

            _ = RunDeadlineAsync(
                registration,
                deadline,
                enqueueCallback);
        }

        replacedRegistration?.Cancel();
    }

    public bool IsCurrent(LobbyDeadlineInvocation invocation)
    {
        lock (syncRoot)
        {
            return !disposed
                && registrations.TryGetValue(
                    invocation.Key,
                    out var registration)
                && registration.Generation == invocation.Generation
                && !registration.IsCancellationRequested;
        }
    }

    public bool Cancel(LobbyDeadlineKey key)
    {
        DeadlineRegistration? registration;
        lock (syncRoot)
        {
            if (!registrations.Remove(key, out registration))
                return false;
        }

        registration.Cancel();
        return true;
    }

    public void CancelAll()
    {
        DeadlineRegistration[] registrationSnapshot;
        lock (syncRoot)
        {
            registrationSnapshot = [.. registrations.Values];
            registrations.Clear();
        }

        foreach (var registration in registrationSnapshot)
        {
            registration.Cancel();
        }
    }

    public ValueTask DisposeAsync()
    {
        lock (syncRoot)
        {
            disposalTask ??= DisposeCoreAsync();
            return new ValueTask(disposalTask);
        }
    }

    private async Task RunDeadlineAsync(
        DeadlineRegistration registration,
        DateTimeOffset deadline,
        Func<LobbyDeadlineInvocation, CancellationToken, ValueTask>
            enqueueCallback)
    {
        try
        {
            var delay = deadline - timeProvider.GetUtcNow();
            if (delay < TimeSpan.Zero)
                delay = TimeSpan.Zero;

            await Task.Delay(
                    delay,
                    timeProvider,
                    registration.Token)
                .ConfigureAwait(false);

            var invocation = new LobbyDeadlineInvocation(
                registration.Key,
                registration.Generation);
            if (!IsCurrent(invocation))
                return;

            await enqueueCallback(invocation, registration.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
            when (registration.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Lobby deadline callback {DeadlineKind} generation {DeadlineGeneration} failed",
                registration.Key.Kind,
                registration.Generation);
        }
        finally
        {
            lock (syncRoot)
            {
                if (registrations.TryGetValue(
                        registration.Key,
                        out var currentRegistration)
                    && ReferenceEquals(
                        currentRegistration,
                        registration))
                {
                    registrations.Remove(registration.Key);
                }

                activeRegistrations.Remove(registration);
            }

            registration.Dispose();
            registration.MarkCompleted();
        }
    }

    private async Task DisposeCoreAsync()
    {
        DeadlineRegistration[] registrationSnapshot;
        lock (syncRoot)
        {
            if (disposed)
                return;

            disposed = true;
            registrations.Clear();
            registrationSnapshot = [.. activeRegistrations];
        }

        foreach (var registration in registrationSnapshot)
        {
            registration.Cancel();
        }

        await Task.WhenAll(
                registrationSnapshot.Select(
                    registration => registration.Completion))
            .ConfigureAwait(false);
    }

    private static void ValidateKey(LobbyDeadlineKey key)
    {
        switch (key.Kind)
        {
            case LobbyDeadlineKind.PREPARATION:
            case LobbyDeadlineKind.MODE:
                if (key.RoundId is null || key.RoundId == Guid.Empty)
                {
                    throw new ArgumentException(
                        "Phase deadlines require a non-empty round identifier.",
                        nameof(key));
                }

                if (key.UserId is not null)
                {
                    throw new ArgumentException(
                        "Phase deadlines cannot target a user.",
                        nameof(key));
                }

                break;
            case LobbyDeadlineKind.DISCONNECT_GRACE:
                if (key.UserId is null || key.UserId == Guid.Empty)
                {
                    throw new ArgumentException(
                        "Disconnect deadlines require a non-empty user identifier.",
                        nameof(key));
                }

                if (key.RoundId is not null)
                {
                    throw new ArgumentException(
                        "Disconnect deadlines cannot target a round.",
                        nameof(key));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(key),
                    "The lobby deadline kind is unsupported.");
        }
    }

    private sealed class DeadlineRegistration : IDisposable
    {
        private readonly object syncRoot = new();
        private readonly CancellationTokenSource cancellationTokenSource =
            new();
        private readonly TaskCompletionSource completionSource =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private bool disposed;

        public DeadlineRegistration(
            LobbyDeadlineKey key,
            long generation)
        {
            Key = key;
            Generation = generation;
        }

        public LobbyDeadlineKey Key { get; }

        public long Generation { get; }

        public CancellationToken Token =>
            cancellationTokenSource.Token;

        public bool IsCancellationRequested =>
            cancellationTokenSource.IsCancellationRequested;

        public Task Completion => completionSource.Task;

        public void Cancel()
        {
            lock (syncRoot)
            {
                if (!disposed)
                    cancellationTokenSource.Cancel();
            }
        }

        public void MarkCompleted() =>
            completionSource.TrySetResult();

        public void Dispose()
        {
            lock (syncRoot)
            {
                if (disposed)
                    return;

                disposed = true;
                cancellationTokenSource.Dispose();
            }
        }
    }
}
