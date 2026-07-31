using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoBingo.Api.Hubs;
using GeoBingo.Api.Lobbies.Mapping;
using GeoBingo.Contracts.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace GeoBingo.Api.Lobbies.Runtime;

internal sealed class LobbyResultsPublisher(
    LobbyResultsProjectionFactory projectionFactory,
    IHubContext<GameHub, IGameClient> hubContext)
{
    private readonly LobbyResultsProjectionFactory projectionFactory =
        projectionFactory
        ?? throw new ArgumentNullException(nameof(projectionFactory));
    private readonly IHubContext<GameHub, IGameClient> hubContext =
        hubContext
        ?? throw new ArgumentNullException(nameof(hubContext));

    public async ValueTask PublishCompletedRoundAsync(
        LobbyRuntimeState state,
        Guid completedRoundId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);
        List<Exception>? publicationFailures = null;
        foreach (var view in projectionFactory.CreateCompletionViews(
                     state,
                     completedRoundId))
        {
            try
            {
                await hubContext.Clients
                    .Group(LobbyHubGroups.Lobby(state.LobbyId))
                    .ReceiveResults(view)
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                publicationFailures ??= [];
                publicationFailures.Add(exception);
            }
        }

        if (publicationFailures is not null)
        {
            throw new AggregateException(
                "One or more lobby result views could not be published.",
                publicationFailures);
        }
    }
}
