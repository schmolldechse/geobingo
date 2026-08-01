using System;
using System.Linq;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.Lobbies;

namespace GeoBingo.Api.Lobbies.Mapping;

internal sealed class LobbyRoundResultsProjectionFactory
{
    public LobbyRoundResultsView CreateForMember(
        LobbyRuntimeState state,
        Guid userId,
        RequestRoundResultsRequest request)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);
        if (!state.Members.ContainsKey(userId))
        {
            throw new LobbyRuntimeException(
                ErrorCode.NOT_A_MEMBER,
                "The current user is not a lobby member.");
        }

        var results = state.CompletedRoundResults
            .FirstOrDefault(result => result.RoundId == request.RoundId);
        if (results is null)
        {
            if (state.CurrentRound?.RoundId == request.RoundId)
            {
                throw new LobbyRuntimeException(
                    ErrorCode.ROUND_NOT_COMPLETED,
                    "The requested round is not complete.");
            }

            throw new LobbyRuntimeException(
                ErrorCode.ROUND_NOT_FOUND,
                "The requested round was not found.");
        }

        return new LobbyRoundResultsView
        {
            LobbyId = state.LobbyId,
            StateVersion = state.StateVersion,
            Results = results
        };
    }
}
