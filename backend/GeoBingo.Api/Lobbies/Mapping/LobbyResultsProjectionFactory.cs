using System;
using System.Collections.Generic;
using System.Linq;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.Lobbies;

namespace GeoBingo.Api.Lobbies.Mapping;

internal sealed class LobbyResultsProjectionFactory
{
    public LobbyResultsView CreateForMember(
        LobbyRuntimeState state,
        Guid userId,
        RequestResultsRequest request)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);
        if (!state.Members.ContainsKey(userId))
        {
            throw new LobbyRuntimeException(
                ErrorCode.NOT_A_MEMBER,
                "The current user is not a lobby member.");
        }

        return Create(state, request);
    }

    public IReadOnlyList<LobbyResultsView> CreateCompletionViews(
        LobbyRuntimeState state,
        Guid completedRoundId)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (completedRoundId == Guid.Empty
            || state.CompletedRoundResults.All(
                result => result.RoundId != completedRoundId))
        {
            throw new InvalidOperationException(
                "The completed round must exist before its results are projected.");
        }

        return
        [
            Create(
                state,
                new RequestResultsRequest
                {
                    Scope = ResultsScope.LAST_COMPLETED_ROUND
                }),
            Create(
                state,
                new RequestResultsRequest
                {
                    Scope = ResultsScope.CUMULATIVE
                })
        ];
    }

    private static LobbyResultsView Create(
        LobbyRuntimeState state,
        RequestResultsRequest request)
    {
        if (request.Scope == ResultsScope.SPECIFIC_ROUND
            && request.RoundId is null
            || request.Scope != ResultsScope.SPECIFIC_ROUND
            && request.RoundId is not null)
        {
            throw new LobbyRuntimeException(
                ErrorCode.VALIDATION_FAILED,
                "The result scope and round identifier do not match.");
        }

        CompletedRoundResults? roundResults =
            request.Scope switch
            {
                ResultsScope.CUMULATIVE => null,
                ResultsScope.LAST_COMPLETED_ROUND =>
                    state.CompletedRoundResults.LastOrDefault()
                    ?? throw new LobbyRuntimeException(
                        ErrorCode.ROUND_NOT_FOUND,
                        "No completed round is available."),
                ResultsScope.SPECIFIC_ROUND =>
                    state.CompletedRoundResults
                        .FirstOrDefault(result => result.RoundId == request.RoundId)
                    ?? ResolveMissingRound(state, request.RoundId),
                _ => throw new LobbyRuntimeException(
                    ErrorCode.VALIDATION_FAILED,
                    "The result scope is invalid.")
            };

        return new LobbyResultsView
        {
            LobbyId = state.LobbyId,
            StateVersion = state.StateVersion,
            Scope = request.Scope,
            RoundId = roundResults?.RoundId,
            RoundNumber = roundResults?.RoundNumber,
            CompletedAt = roundResults?.CompletedAt,
            RoundResults = roundResults,
            CumulativeResults = state.CumulativeResults.ToArray()
        };
    }

    private static CompletedRoundResults ResolveMissingRound(
        LobbyRuntimeState state,
        Guid? roundId)
    {
        if (roundId is Guid requestedRoundId
            && state.CurrentRound?.RoundId == requestedRoundId)
        {
            throw new LobbyRuntimeException(
                ErrorCode.ROUND_NOT_COMPLETED,
                "The requested round is not complete.");
        }

        throw new LobbyRuntimeException(
            ErrorCode.ROUND_NOT_FOUND,
            "The requested round was not found.");
    }
}
