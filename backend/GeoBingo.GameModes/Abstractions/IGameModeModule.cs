using System;

namespace GeoBingo.GameModes.Abstractions;

public interface IGameModeModule
{
    string Key { get; }

    string DisplayName { get; }

    string Description { get; }

    Version Version { get; }

    int Order { get; }

    IGameModeLobbyState CreateDefaultLobbyState();

    GameModeValidationResult ValidateLobbyState(IGameModeLobbyState state, GameModeValidationContext context);

    GameModeRoundCreation CreateRound(IGameModeLobbyState state, GameModeRoundCreationContext context);

    GameModeProjectionPair CreateProjections(GameModeProjectionContext context);

    GameModeAdvanceOutcome Advance(GameModeAdvanceContext context);
}
