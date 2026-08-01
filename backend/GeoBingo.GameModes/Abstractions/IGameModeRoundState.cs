using System;

namespace GeoBingo.GameModes.Abstractions;

public interface IGameModeRoundState
{
    Guid RoundId { get; }

    int RoundNumber { get; }

    string ModeKey { get; }

    Version ModeVersion { get; }

    DateTimeOffset CreatedAt { get; }
}
