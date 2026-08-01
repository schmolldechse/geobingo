using System;
using GeoBingo.GameModes.Abstractions;
using GeoBingo.GameModes.CaptureChallenge;
using GeoBingo.GameModes.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace GeoBingo.GameModes;

public static class DependencyInjection
{
    public static IServiceCollection AddGeoBingoGameModes(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<CaptureChallengeModule>();
        services.AddSingleton<IGameModeModule>(provider => provider.GetRequiredService<CaptureChallengeModule>());
        services.AddSingleton<CaptureChallengeOperations>();
        services.AddSingleton<IGameModeRegistry, GameModeRegistry>();
        return services;
    }
}
