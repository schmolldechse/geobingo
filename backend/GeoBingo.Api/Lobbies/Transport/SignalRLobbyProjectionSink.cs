using System;
using System.Threading;
using System.Threading.Tasks;
using GeoBingo.Api.Hubs;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Contracts.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace GeoBingo.Api.Lobbies.Transport;

internal sealed class SignalRLobbyProjectionSink
    : ILobbyProjectionSink
{
    private readonly IHubContext<GameHub, IGameClient> hubContext;

    public SignalRLobbyProjectionSink(
        IHubContext<GameHub, IGameClient> hubContext)
    {
        this.hubContext = hubContext
            ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public async ValueTask PublishAsync(
        LobbyProjectionBatch batch,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(batch);
        if (batch.IsClosingOrClosed)
        {
            return;
        }

        await hubContext.Clients
            .Group(LobbyHubGroups.Lobby(
                batch.Snapshot.LobbyId))
            .ReceiveLobbySnapshot(batch.Snapshot)
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var entry in batch.PersonalProjections)
        {
            await hubContext.Clients
                .Group(LobbyHubGroups.Personal(
                    batch.Snapshot.LobbyId,
                    entry.Key))
                .ReceivePersonalProjection(entry.Value)
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
