using System;
using System.Collections.Generic;
using System.Linq;
using GeoBingo.Contracts.Lobbies;

namespace GeoBingo.GameModes.CaptureChallenge;

internal static class CaptureChallengeScoring
{
    public static CompletedRoundResults CreateResults(
        CaptureChallengeRoundState state,
        DateTimeOffset completedAt)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.VotingEndsAt is not DateTimeOffset votingEndsAt
            || completedAt != votingEndsAt)
        {
            throw new InvalidOperationException(
                "Capture Challenge results must be finalized at the voting deadline.");
        }

        var captureResults = state.VotingCaptureIds
            .Select(state.FindCapture)
            .Select(
                capture => capture
                    ?? throw new InvalidOperationException(
                        "Every voting-order entry must reference a released capture."))
            .Select(capture => CreateCaptureResult(state, capture))
            .ToArray();
        var scoreByUserId = captureResults
            .GroupBy(result => result.OwnerUserId)
            .ToDictionary(
                group => group.Key,
                group => Math.Round(
                    group.Sum(result => result.Score),
                    2,
                    MidpointRounding.AwayFromZero));
        var orderedPlayers = state.Participants
            .Where(participant => state.EligibleVoterIds.Contains(participant.UserId))
            .OrderByDescending(
                participant => scoreByUserId.GetValueOrDefault(participant.UserId))
            .ThenBy(participant => participant.JoinOrder)
            .ToArray();
        var playerResults = CreatePlayerResults(
            orderedPlayers.Select(
                participant => new PlayerScore(
                    participant.UserId,
                    scoreByUserId.GetValueOrDefault(participant.UserId))));

        return new CompletedRoundResults
        {
            RoundId = state.RoundId,
            RoundNumber = state.RoundNumber,
            CompletedAt = completedAt,
            Captures = captureResults,
            Players = playerResults
        };
    }

    private static CaptureResult CreateCaptureResult(
        CaptureChallengeRoundState state,
        CaptureChallengeCapture capture)
    {
        var goal = state.FindGoal(capture.RoundGoalId)
            ?? throw new InvalidOperationException(
                "Every released capture must reference a round goal.");
        var eligible = state.CountEligibleVoters(capture);
        var votes = state.Votes
            .Where(vote => vote.CaptureId == capture.CaptureId)
            .ToArray();
        var good = votes.Count(
            vote => vote.Value
                == Contracts.GameModes.CaptureChallenge.VoteValue.GOOD);
        var bad = votes.Count(
            vote => vote.Value
                == Contracts.GameModes.CaptureChallenge.VoteValue.BAD);
        if (eligible < 0
            || good < 0
            || bad < 0
            || good + bad > eligible
            || goal.ScoreFactor is < 0m or > 10m
            || goal.ScoreFactor % CaptureChallengeDefaults.ScoreFactorStep != 0m)
        {
            throw new InvalidOperationException(
                "The frozen voting data is invalid and cannot be scored.");
        }
        var score = eligible == 0
            ? 0m
            : Math.Round(
                100m
                * Math.Max(0m, (good - bad) / (decimal)eligible)
                * goal.ScoreFactor,
                2,
                MidpointRounding.AwayFromZero);

        return new CaptureResult
        {
            CaptureId = capture.CaptureId,
            GoalId = capture.RoundGoalId,
            OwnerUserId = capture.OwnerUserId,
            Good = good,
            Bad = bad,
            Eligible = eligible,
            ScoreFactor = goal.ScoreFactor,
            Score = score,
            NoEligibleVoters = eligible == 0
        };
    }

    private static IReadOnlyList<PlayerRoundResult> CreatePlayerResults(
        IEnumerable<PlayerScore> orderedPlayers)
    {
        var results = new List<PlayerRoundResult>();
        decimal? previousScore = null;
        var rank = 0;
        foreach (var player in orderedPlayers)
        {
            if (previousScore != player.Score)
            {
                rank = results.Count + 1;
                previousScore = player.Score;
            }

            results.Add(
                new PlayerRoundResult
                {
                    UserId = player.UserId,
                    Score = player.Score,
                    Rank = rank
                });
        }

        return results;
    }

    private sealed record PlayerScore(Guid UserId, decimal Score);
}
