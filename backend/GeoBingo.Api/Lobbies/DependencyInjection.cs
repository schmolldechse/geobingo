using System;
using GeoBingo.Api.Lobbies.Mapping;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Api.Lobbies.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace GeoBingo.Api.Lobbies;

public static class DependencyInjection
{
    public static IServiceCollection AddGeoBingoLobbyRuntime(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<LobbyConnectionRegistry>();
        services.AddSingleton<LobbyProjectionMapper>();
        services.AddSingleton<LobbyRoundResultsProjectionFactory>();
        services.AddSingleton<LobbyProjectionPublisher>();
        services.AddSingleton<
            ILobbyProjectionSink,
            SignalRLobbyProjectionSink>();
        services.AddSingleton<LobbyTerminationPublisher>();
        services.AddSingleton<LobbyMemberTransportEjector>();
        services.AddSingleton<LobbyRuntimeCleanupQueue>();
        services.AddHostedService(provider =>
            provider.GetRequiredService<
                LobbyRuntimeCleanupQueue>());
        services.AddSingleton<ILobbyRegistry, LobbyRegistry>();
        services.AddSingleton<LobbyOperationDispatcher>();
        services.AddSingleton<LobbyMembershipPolicy>();
        services.AddSingleton<LobbyConnectionLifecycle>();
        services.AddSingleton<LobbyRestMapper>();
        return services;
    }
}
