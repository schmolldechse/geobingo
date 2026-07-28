using System;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Api.Mapping;
using GeoBingo.Contracts.Lobbies;
using GeoBingo.GameModes.Registry;

namespace GeoBingo.Api.Lobbies.Mapping;

public sealed class LobbyRestMapper(
    IGameModeRegistry gameModeRegistry,
    GameModeMapper gameModeMapper
)
{
    internal CreateLobbyResponse MapCreated(LobbyRuntimeSummary summary) => new()
    {
        LobbyId = summary.LobbyId,
        Code = summary.Code,
        StateVersion = summary.StateVersion,
        SelectedMode = gameModeMapper.Map(
            gameModeRegistry.GetRequired(summary.SelectedModeKey, summary.SelectedModeVersion)),
        HubPath = "/hubs/game"
    };

    internal ResolveLobbyResponse MapResolved(LobbyRuntimeState state, Guid userId, bool acceptingCreations) => new()
    {
        LobbyId = state.LobbyId,
        Code = state.Code,
        Status = state.Status,
        SelectedMode = gameModeMapper.Map(
                gameModeRegistry.GetRequired(state.SelectedModeKey, state.SelectedModeVersion)),
        PlayerCount = state.Members.Count,
        MaxPlayers = state.Settings.MaxPlayers,
        Joinable = acceptingCreations && state.CanCreateMembership(userId),
        Reconnectable = state.CanReconnect(userId)
    };
}
