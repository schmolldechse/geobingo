using System;
using GeoBingo.Contracts.Lobbies;

namespace GeoBingo.GameModes.Abstractions;

public sealed record LobbyOperationContext(
    Guid LobbyId,
    Guid ActorUserId,
    Guid HostUserId,
    bool ActorMayMutate,
    LobbyStatus LobbyStatus,
    string SelectedModeKey,
    Version SelectedModeVersion,
    DateTimeOffset Now);
