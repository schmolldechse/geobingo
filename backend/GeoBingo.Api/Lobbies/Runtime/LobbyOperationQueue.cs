using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace GeoBingo.Api.Lobbies.Runtime;

internal sealed class LobbyOperationQueue : IAsyncDisposable
{
    private readonly object lifecycleSyncRoot = new();
    private readonly Channel<IQueuedLobbyOperation> operations;
    private readonly CancellationTokenSource runtimeCancellationTokenSource = new();
    private readonly Task readerTask;
    private bool acceptingOperations = true;
    private int disposeStarted;

    public LobbyOperationQueue()
    {
        operations = Channel.CreateUnbounded<IQueuedLobbyOperation>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });
        readerTask = ProcessOperationsAsync();
    }

    public ValueTask<TResult> EnqueueAsync<TResult>(
        Func<CancellationToken, ValueTask<TResult>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();

        var queuedOperation = new QueuedLobbyOperation<TResult>(operation);
        lock (lifecycleSyncRoot)
        {
            if (!acceptingOperations
                || !operations.Writer.TryWrite(queuedOperation))
            {
                throw new InvalidOperationException(
                    "The lobby operation queue no longer accepts operations.");
            }
        }

        return WaitForCompletionAsync(
            queuedOperation.Completion,
            cancellationToken);
    }

    public async ValueTask CompleteAsync()
    {
        lock (lifecycleSyncRoot)
        {
            if (acceptingOperations)
            {
                acceptingOperations = false;
                operations.Writer.TryComplete();
            }
        }

        await readerTask.ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref disposeStarted, 1) != 0)
        {
            await readerTask.ConfigureAwait(false);
            return;
        }

        lock (lifecycleSyncRoot)
        {
            acceptingOperations = false;
            operations.Writer.TryComplete();
        }

        await runtimeCancellationTokenSource
            .CancelAsync()
            .ConfigureAwait(false);

        try
        {
            await readerTask.ConfigureAwait(false);
        }
        finally
        {
            runtimeCancellationTokenSource.Dispose();
        }
    }

    private async Task ProcessOperationsAsync()
    {
        try
        {
            await foreach (var queuedOperation in operations.Reader.ReadAllAsync(
                               runtimeCancellationTokenSource.Token))
            {
                await queuedOperation
                    .ExecuteAsync(runtimeCancellationTokenSource.Token)
                    .ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
            when (runtimeCancellationTokenSource.IsCancellationRequested)
        {
        }
        finally
        {
            while (operations.Reader.TryRead(out var queuedOperation))
            {
                queuedOperation.Cancel(
                    runtimeCancellationTokenSource.Token);
            }
        }
    }

    private static async ValueTask<TResult> WaitForCompletionAsync<TResult>(
        Task<TResult> completion,
        CancellationToken cancellationToken) =>
        await completion
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

    private interface IQueuedLobbyOperation
    {
        ValueTask ExecuteAsync(
            CancellationToken runtimeCancellationToken);

        void Cancel(CancellationToken cancellationToken);
    }

    private sealed class QueuedLobbyOperation<TResult>(
        Func<CancellationToken, ValueTask<TResult>> operation)
        : IQueuedLobbyOperation
    {
        private readonly TaskCompletionSource<TResult> completionSource =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<TResult> Completion => completionSource.Task;

        public async ValueTask ExecuteAsync(
            CancellationToken runtimeCancellationToken)
        {
            try
            {
                var result = await operation(runtimeCancellationToken)
                    .ConfigureAwait(false);
                completionSource.TrySetResult(result);
            }
            catch (OperationCanceledException exception)
            {
                completionSource.TrySetCanceled(exception.CancellationToken);
            }
            catch (Exception exception)
            {
                completionSource.TrySetException(exception);
            }
        }

        public void Cancel(CancellationToken cancellationToken) =>
            completionSource.TrySetCanceled(cancellationToken);
    }
}
