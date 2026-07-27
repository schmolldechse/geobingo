using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeoBingo.GameModes.Abstractions;
using GeoBingo.GameModes.Registry;

namespace GeoBingo.Api.Lobbies.Runtime;

internal sealed record LobbyProjectionBatch(
    LobbyRuntimeSummary Lobby,
    GameModeProjectionPair PublicProjection,
    IReadOnlyDictionary<Guid, GameModeProjectionPair> PersonalProjections);

internal interface ILobbyProjectionSink
{
    ValueTask PublishAsync(
        LobbyProjectionBatch batch,
        CancellationToken cancellationToken);
}

internal sealed class LobbyProjectionPublisher
{
    private readonly IGameModeRegistry gameModeRegistry;
    private readonly IReadOnlyList<ILobbyProjectionSink> sinks;
    private readonly TimeProvider timeProvider;

    public LobbyProjectionPublisher(
        IGameModeRegistry gameModeRegistry,
        IEnumerable<ILobbyProjectionSink> sinks,
        TimeProvider timeProvider)
    {
        this.gameModeRegistry = gameModeRegistry
            ?? throw new ArgumentNullException(nameof(gameModeRegistry));
        ArgumentNullException.ThrowIfNull(sinks);
        this.sinks = Array.AsReadOnly(sinks.ToArray());
        this.timeProvider = timeProvider
            ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async ValueTask PublishAsync(
        LobbyRuntimeState state,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);

        var module = gameModeRegistry.GetRequired(
            state.SelectedModeKey,
            state.SelectedModeVersion);
        var now = timeProvider.GetUtcNow();
        var members = state.Members.Values
            .OrderBy(member => member.JoinOrder)
            .ToArray();

        var publicProjection = module.CreateProjections(
            CreateProjectionContext(
                state,
                members.Length,
                recipientUserId: null,
                now));
        if (publicProjection.CaptureChallengePersonal is not null)
        {
            throw new InvalidOperationException(
                "A public game-mode projection cannot contain personal state.");
        }

        var personalProjections =
            new Dictionary<Guid, GameModeProjectionPair>();
        foreach (var member in members)
        {
            personalProjections.Add(
                member.UserId,
                module.CreateProjections(
                    CreateProjectionContext(
                        state,
                        members.Length,
                        member.UserId,
                        now)));
        }

        var batch = new LobbyProjectionBatch(
            state.CreateSummary(),
            publicProjection,
            new ReadOnlyDictionary<Guid, GameModeProjectionPair>(
                personalProjections));

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

    private static GameModeProjectionContext CreateProjectionContext(
        LobbyRuntimeState state,
        int activeParticipantCount,
        Guid? recipientUserId,
        DateTimeOffset now) =>
        new(
            state.ModeLobbyState,
            state.CurrentRound,
            activeParticipantCount,
            recipientUserId,
            now);
}
