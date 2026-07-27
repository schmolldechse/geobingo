using System;
using GeoBingo.Api.Lobbies.Runtime;
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
        services.AddSingleton<LobbyProjectionPublisher>();
        services.AddSingleton<ILobbyRegistry, LobbyRegistry>();
        services.AddSingleton<LobbyOperationDispatcher>();
        services.AddSingleton<LobbyMembershipPolicy>();
        services.AddSingleton<LobbyConnectionLifecycle>();
        return services;
    }
}
