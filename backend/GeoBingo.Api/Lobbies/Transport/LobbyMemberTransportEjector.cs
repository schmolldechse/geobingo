using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeoBingo.Api.Hubs;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Contracts.Lobbies;
using GeoBingo.Contracts.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace GeoBingo.Api.Lobbies.Transport;

internal sealed class LobbyMemberTransportEjector
{
    private readonly IHubContext<GameHub, IGameClient> hubContext;
    private readonly LobbyConnectionRegistry connectionRegistry;

    public LobbyMemberTransportEjector(
        IHubContext<GameHub, IGameClient> hubContext,
        LobbyConnectionRegistry connectionRegistry)
    {
        this.hubContext = hubContext
            ?? throw new ArgumentNullException(nameof(hubContext));
        this.connectionRegistry = connectionRegistry
            ?? throw new ArgumentNullException(
                nameof(connectionRegistry));
    }

    public async ValueTask EjectAsync(
        Guid lobbyId,
        Guid userId,
        LobbyEndedReason reason,
        long stateVersion,
        CancellationToken cancellationToken)
    {
        var connections = connectionRegistry.GetConnections(
            lobbyId,
            userId);
        if (connections.Count == 0)
        {
            return;
        }

        await hubContext.Clients
            .Clients(connections.ToArray())
            .ReceiveLobbyEnded(
                new LobbyEnded
                {
                    LobbyId = lobbyId,
                    Reason = reason,
                    StateVersion = stateVersion
                })
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var connectionId in connections)
        {
            await hubContext.Groups.RemoveFromGroupAsync(
                    connectionId,
                    LobbyHubGroups.Lobby(lobbyId),
                    cancellationToken)
                .ConfigureAwait(false);
            await hubContext.Groups.RemoveFromGroupAsync(
                    connectionId,
                    LobbyHubGroups.Personal(lobbyId, userId),
                    cancellationToken)
                .ConfigureAwait(false);
            connectionRegistry.Disassociate(connectionId);
        }
    }
}
