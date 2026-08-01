using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Data;
using GeoBingo.GameModes.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GeoBingo.Api.Health;

internal sealed class GeoBingoReadinessHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IGameModeRegistry gameModeRegistry;
    private readonly ILobbyRegistry lobbyRegistry;

    public GeoBingoReadinessHealthCheck(
        IServiceScopeFactory scopeFactory,
        IGameModeRegistry gameModeRegistry,
        ILobbyRegistry lobbyRegistry)
    {
        this.scopeFactory = scopeFactory
            ?? throw new ArgumentNullException(
                nameof(scopeFactory));
        this.gameModeRegistry = gameModeRegistry
            ?? throw new ArgumentNullException(
                nameof(gameModeRegistry));
        this.lobbyRegistry = lobbyRegistry
            ?? throw new ArgumentNullException(
                nameof(lobbyRegistry));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["gameModes"] = gameModeRegistry.Modes.Count,
            ["acceptingLobbyCreations"] =
                lobbyRegistry.IsAcceptingCreations
        };

        if (gameModeRegistry.Modes.Count == 0)
        {
            return HealthCheckResult.Unhealthy(
                "No game mode is registered.",
                data: data);
        }

        if (!lobbyRegistry.IsAcceptingCreations)
        {
            return HealthCheckResult.Unhealthy(
                "The application is shutting down.",
                data: data);
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var dataContext =
            scope.ServiceProvider.GetRequiredService<DataContext>();
        if (!await dataContext.Database
                .CanConnectAsync(cancellationToken)
                .ConfigureAwait(false))
        {
            return HealthCheckResult.Unhealthy(
                "The database is unavailable.",
                data: data);
        }

        return HealthCheckResult.Healthy(
            "GeoBingo is ready.",
            data);
    }
}
