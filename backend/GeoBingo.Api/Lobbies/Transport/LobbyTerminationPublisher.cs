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

internal sealed class LobbyTerminationPublisher
{
    private readonly IHubContext<GameHub, IGameClient> hubContext;
    private readonly LobbyConnectionRegistry connectionRegistry;

    public LobbyTerminationPublisher(
        IHubContext<GameHub, IGameClient> hubContext,
        LobbyConnectionRegistry connectionRegistry)
    {
        this.hubContext = hubContext
            ?? throw new ArgumentNullException(nameof(hubContext));
        this.connectionRegistry = connectionRegistry
            ?? throw new ArgumentNullException(
                nameof(connectionRegistry));
    }

    public async ValueTask PublishAsync(
        Guid lobbyId,
        LobbyEndedReason reason,
        long stateVersion,
        CancellationToken cancellationToken)
    {
        var connections =
            connectionRegistry.RemoveLobby(lobbyId);
        if (connections.Count == 0)
        {
            return;
        }

        var connectionIds = connections
            .Select(context => context.ConnectionId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var transportReason =
            reason == LobbyEndedReason.NO_MEMBERS
                && connections.Count > 0
                    ? LobbyEndedReason.LEFT
                    : reason;
        var message = new LobbyEnded
        {
            LobbyId = lobbyId,
            Reason = transportReason,
            StateVersion = stateVersion
        };

        await hubContext.Clients
            .Clients(connectionIds)
            .ReceiveLobbyEnded(message)
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var connection in connections)
        {
            await hubContext.Groups.RemoveFromGroupAsync(
                    connection.ConnectionId,
                    LobbyHubGroups.Lobby(lobbyId),
                    cancellationToken)
                .ConfigureAwait(false);
            await hubContext.Groups.RemoveFromGroupAsync(
                    connection.ConnectionId,
                    LobbyHubGroups.Personal(
                        lobbyId,
                        connection.UserId),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
