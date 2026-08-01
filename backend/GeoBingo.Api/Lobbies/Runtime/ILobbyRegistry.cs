using System;
using System.Collections.Generic;

namespace GeoBingo.Api.Lobbies.Runtime;

public interface ILobbyRegistry
{
    bool IsAcceptingCreations { get; }

    LobbyRuntime Create(LobbyCreation creation);

    bool TryGet(Guid lobbyId, out LobbyRuntime? runtime);

    bool TryResolveCode(string code, out Guid lobbyId);

    IReadOnlyList<LobbyRuntime> GetActiveRuntimes();

    void StopAcceptingCreations();

    bool TryRemoveClosed(LobbyRuntime runtime);
}
