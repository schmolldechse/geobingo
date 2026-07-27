using System;

namespace GeoBingo.GameModes.Abstractions;

public interface IGameModeRoundState
{
    Guid RoundId { get; }
}
