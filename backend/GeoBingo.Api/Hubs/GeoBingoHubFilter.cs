using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Contracts.SignalR;
using GeoBingo.Observability.Logging;
using GeoBingo.Observability.Metrics;
using GeoBingo.Observability.Tracing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GeoBingo.Api.Hubs;

internal sealed class GeoBingoHubFilter : IHubFilter
{
    private readonly HubContractValidator validator;
    private readonly SignalRErrorMapper errorMapper;
    private readonly HubInvocationMetadataAccessor metadataAccessor;
    private readonly LobbyConnectionRegistry connectionRegistry;
    private readonly GameMetrics gameMetrics;
    private readonly GameActivitySource gameActivitySource;
    private readonly ILogger<GeoBingoHubFilter> logger;

    public GeoBingoHubFilter(
        HubContractValidator validator,
        SignalRErrorMapper errorMapper,
        HubInvocationMetadataAccessor metadataAccessor,
        LobbyConnectionRegistry connectionRegistry,
        GameMetrics gameMetrics,
        GameActivitySource gameActivitySource,
        ILogger<GeoBingoHubFilter> logger)
    {
        this.validator = validator
            ?? throw new ArgumentNullException(nameof(validator));
        this.errorMapper = errorMapper
            ?? throw new ArgumentNullException(nameof(errorMapper));
        this.metadataAccessor = metadataAccessor
            ?? throw new ArgumentNullException(
                nameof(metadataAccessor));
        this.connectionRegistry = connectionRegistry
            ?? throw new ArgumentNullException(
                nameof(connectionRegistry));
        this.gameMetrics = gameMetrics
            ?? throw new ArgumentNullException(nameof(gameMetrics));
        this.gameActivitySource = gameActivitySource
            ?? throw new ArgumentNullException(
                nameof(gameActivitySource));
        this.logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var startedAt = Stopwatch.GetTimestamp();
        var correlationId = Guid.NewGuid().ToString("N");
        var userId = invocationContext.Context.User
            ?.FindFirstValue(ClaimTypes.NameIdentifier);
        connectionRegistry.TryGet(
            invocationContext.Context.ConnectionId,
            out var connectionContext);
        using var activity = gameActivitySource.StartActivity(
            GameActivitySource.HubOperation);
        activity?.SetTag(
            "hub.operation",
            invocationContext.HubMethodName);
        activity?.SetTag(
            "signalr.connection.id",
            invocationContext.Context.ConnectionId);
        activity?.SetTag("enduser.id", userId);
        activity?.SetTag(
            "geobingo.lobby.id",
            connectionContext?.LobbyId);
        activity?.SetTag("correlation.id", correlationId);
        var traceId = (activity ?? Activity.Current)
            ?.TraceId.ToString()
            ?? ActivityTraceId.CreateRandom().ToString();
        using var metadataScope = metadataAccessor.Push(
            new HubInvocationMetadata(correlationId, traceId));
        using var loggingScope = logger.BeginScope(
            new Dictionary<string, object?>
            {
                [LoggingConstants.OperationName] =
                    invocationContext.HubMethodName,
                [LoggingConstants.ConnectionId] =
                    invocationContext.Context.ConnectionId,
                [LoggingConstants.UserId] = userId,
                [LoggingConstants.LobbyId] =
                    connectionContext?.LobbyId,
                [LoggingConstants.CorrelationId] =
                    correlationId,
                [LoggingConstants.TraceId] = traceId
            });

        HubOperationResult result;
        try
        {
            var validationErrors = validator.Validate(
                invocationContext.HubMethodArguments);
            if (validationErrors.Count > 0)
            {
                result = errorMapper.Validation(validationErrors);
            }
            else
            {
                var returned = await next(invocationContext)
                    .ConfigureAwait(false);
                result = returned as HubOperationResult
                    ?? throw new InvalidOperationException(
                        "Every public game hub operation must return HubOperationResult.");
            }
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Hub operation failed");
            result = errorMapper.FromException(exception);
        }

        gameMetrics.HubOperationCompleted(
            invocationContext.HubMethodName,
            result.Success,
            result.Error?.Code.ToString(),
            Stopwatch.GetElapsedTime(startedAt));
        return result;
    }
}
