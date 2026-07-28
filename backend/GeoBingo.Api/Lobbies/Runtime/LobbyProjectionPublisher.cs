using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeoBingo.Api.Lobbies.Mapping;
using GeoBingo.Contracts.Lobbies;

namespace GeoBingo.Api.Lobbies.Runtime;

internal sealed record LobbyProjectionBatch(
    LobbySnapshot Snapshot,
    IReadOnlyDictionary<Guid, PersonalProjection>
        PersonalProjections,
    bool IsClosingOrClosed);

internal interface ILobbyProjectionSink
{
    ValueTask PublishAsync(
        LobbyProjectionBatch batch,
        CancellationToken cancellationToken);
}

internal sealed class LobbyProjectionPublisher
{
    private readonly LobbyProjectionMapper projectionMapper;
    private readonly IReadOnlyList<ILobbyProjectionSink> sinks;

    public LobbyProjectionPublisher(
        LobbyProjectionMapper projectionMapper,
        IEnumerable<ILobbyProjectionSink> sinks)
    {
        this.projectionMapper = projectionMapper
            ?? throw new ArgumentNullException(
                nameof(projectionMapper));
        ArgumentNullException.ThrowIfNull(sinks);
        this.sinks = Array.AsReadOnly(sinks.ToArray());
    }

    public async ValueTask PublishAsync(
        LobbyRuntimeState state,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.IsClosing || state.IsClosed)
        {
            return;
        }

        var batch = projectionMapper.CreateBatch(state);

        List<Exception>? publicationFailures = null;
        foreach (var sink in sinks)
        {
            try
            {
                await sink
                    .PublishAsync(batch, cancellationToken)
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
                "One or more lobby projection sinks failed.",
                publicationFailures);
        }
    }

}
