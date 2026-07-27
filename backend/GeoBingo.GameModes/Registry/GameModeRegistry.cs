using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GeoBingo.Contracts.Common;
using GeoBingo.GameModes.Abstractions;

namespace GeoBingo.GameModes.Registry;

public sealed class GameModeRegistry : IGameModeRegistry
{
    private static readonly Regex KeyPattern = new("^[a-z][a-z0-9_]*$", RegexOptions.CultureInvariant);

    private readonly IReadOnlyDictionary<string, IGameModeModule> _modesByKey;

    public GameModeRegistry(IEnumerable<IGameModeModule> modules)
    {
        ArgumentNullException.ThrowIfNull(modules);

        var configuredModules = modules.ToArray();
        if (configuredModules.Length == 0)
            throw new InvalidOperationException("At least one game mode must be registered.");

        if (configuredModules.Any(module => module is null))
            throw new InvalidOperationException("Registered game modes cannot contain null entries.");

        var keys = new HashSet<string>(StringComparer.Ordinal);
        var orders = new HashSet<int>();
        foreach (var module in configuredModules)
        {
            ValidateMetadata(module);
            if (!keys.Add(module.Key))
                throw new InvalidOperationException($"The game-mode key '{module.Key}' is registered more than once.");

            if (!orders.Add(module.Order))
                throw new InvalidOperationException($"The game-mode order '{module.Order}' is registered more than once.");

            ValidateDefaultStateFactory(module);
        }

        var orderedModes = configuredModules
            .OrderBy(module => module.Order)
            .ToArray();
        Modes = Array.AsReadOnly(orderedModes);
        Default = orderedModes[0];
        _modesByKey = orderedModes.ToDictionary(module => module.Key, StringComparer.Ordinal);
    }

    public IReadOnlyList<IGameModeModule> Modes { get; }

    public IGameModeModule Default { get; }

    public bool TryGet(string key, out IGameModeModule? module)
    {
        if (key is null)
        {
            module = null;
            return false;
        }

        return _modesByKey.TryGetValue(key, out module);
    }

    public IGameModeModule GetRequired(string key)
    {
        if (!TryGet(key, out var module) || module is null)
            throw new GameModeDomainException(ErrorCode.MODE_NOT_FOUND, "The requested game mode was not found.");
        return module;
    }

    public IGameModeModule GetRequired(string key, Version version)
    {
        ArgumentNullException.ThrowIfNull(version);

        var module = GetRequired(key);
        if (module.Version != version)
            throw new GameModeDomainException(ErrorCode.MODE_VERSION_MISMATCH, "The requested game-mode version is not supported.");

        return module;
    }

    private static void ValidateMetadata(IGameModeModule module)
    {
        if (string.IsNullOrWhiteSpace(module.Key)
            || module.Key.Length > 64
            || !KeyPattern.IsMatch(module.Key))
            throw new InvalidOperationException("Every game mode must use a key matching ^[a-z][a-z0-9_]*$ with at most 64 characters.");

        if (string.IsNullOrWhiteSpace(module.DisplayName)
            || module.DisplayName.Length > 64
            || !string.Equals(
                module.DisplayName,
                module.DisplayName.Trim(),
                StringComparison.Ordinal))
            throw new InvalidOperationException($"Game mode '{module.Key}' must have a trimmed display name with at most 64 characters.");

        if (string.IsNullOrWhiteSpace(module.Description)
            || module.Description.Length > 500
            || !string.Equals(
                module.Description,
                module.Description.Trim(),
                StringComparison.Ordinal))
            throw new InvalidOperationException($"Game mode '{module.Key}' must have a trimmed description with at most 500 characters.");

        if (module.Version is null
            || module.Version.Build < 0
            || module.Version.Revision >= 0)
            throw new InvalidOperationException($"Game mode '{module.Key}' must use a normalized three-part version without a revision.");

        _ = module.Version.ToString(3);
    }

    private static void ValidateDefaultStateFactory(IGameModeModule module)
    {
        _ = module.CreateDefaultLobbyState()
            ?? throw new InvalidOperationException(
                $"Game mode '{module.Key}' returned no default lobby state.");
    }
}
