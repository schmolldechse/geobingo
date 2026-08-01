using System;
using System.Collections.Generic;
using GeoBingo.GameModes.Abstractions;

namespace GeoBingo.GameModes.Registry;

public interface IGameModeRegistry
{
    IReadOnlyList<IGameModeModule> Modes { get; }

    IGameModeModule Default { get; }

    bool TryGet(string key, out IGameModeModule? module);

    IGameModeModule GetRequired(string key);

    IGameModeModule GetRequired(string key, Version version);
}
