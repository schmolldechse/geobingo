using System;
using System.Threading;
using System.Threading.Tasks;
using GeoBingo.Api.Hubs;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Contracts.Lobbies;
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

        foreach (var entry in batch.PersonalProjections)
        {
            await hubContext.Clients
                .Group(LobbyHubGroups.Personal(
                    batch.Snapshot.LobbyId,
                    entry.Key))
                .ReceiveLobbyProjection(
                    new LobbyProjection
                    {
                        Snapshot = batch.Snapshot,
                        PersonalProjection = entry.Value
                    })
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
