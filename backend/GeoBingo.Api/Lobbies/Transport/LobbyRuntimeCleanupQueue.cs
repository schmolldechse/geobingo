using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GeoBingo.Api.Lobbies.Runtime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GeoBingo.Api.Lobbies.Transport;

internal sealed class LobbyRuntimeCleanupQueue
    : BackgroundService
{
    private readonly Channel<LobbyRuntime> queue =
        Channel.CreateUnbounded<LobbyRuntime>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
    private readonly ILogger<LobbyRuntimeCleanupQueue> logger;

    public LobbyRuntimeCleanupQueue(
        ILogger<LobbyRuntimeCleanupQueue> logger)
    {
        this.logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Enqueue(LobbyRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        if (!queue.Writer.TryWrite(runtime))
        {
            throw new InvalidOperationException(
                "The lobby cleanup queue is unavailable.");
        }
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var runtime in queue.Reader
                               .ReadAllAsync(stoppingToken)
                               .ConfigureAwait(false))
            {
                try
                {
                    await runtime.CompleteAsync()
                        .ConfigureAwait(false);
                    await runtime.DisposeAsync()
                        .ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Failed to dispose closed lobby runtime {LobbyId}",
                        runtime.LobbyId);
                }
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
