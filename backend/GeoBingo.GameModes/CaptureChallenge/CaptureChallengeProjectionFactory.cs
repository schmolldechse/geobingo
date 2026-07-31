using System;
using System.Collections.Generic;
using System.Linq;
using GeoBingo.Contracts.GameModes.CaptureChallenge;
using GeoBingo.GameModes.Abstractions;

namespace GeoBingo.GameModes.CaptureChallenge;

internal static class CaptureChallengeProjectionFactory
{
    public static GameModeProjectionPair Create(
        CaptureChallengeLobbyState lobbyState,
        CaptureChallengeRoundState? roundState,
        GameModeProjectionContext context,
        IReadOnlyList<GameModeValidationIssue> roundStartIssues)
    {
        var settings = roundState?.Settings ?? lobbyState.Settings;
        var goals = (roundState?.Goals ?? lobbyState.Goals)
            .OrderBy(goal => goal.DisplayOrder)
            .Select(CreateGoalProjection)
            .ToArray();

        var publicProjection = new CaptureChallengePublicProjection(
            Settings: CaptureChallengeLobbyState.CopySettings(settings),
            Goals: goals,
            ActiveParticipantCount: context.ActiveParticipantCount,
            ProjectedCaptureSlotCount:
                (roundState?.Participants.Count ?? context.ActiveParticipantCount)
                * goals.Length,
            RoundStartIssues:
                roundState is null
                    ? roundStartIssues.ToArray()
                    : [],
            Status: roundState?.Status,
            CaptureEndsAt: roundState?.CaptureEndsAt,
            VotingEndsAt: roundState?.VotingEndsAt,
            CurrentCaptureEndsAt: roundState?.CurrentCaptureEndsAt,
            CurrentCaptureSequence: roundState?.CurrentCaptureSequence,
            ParticipantCaptureProgress: CreateParticipantCaptureProgress(roundState),
            SubmittedCaptureCount: roundState?.Captures.Count ?? 0,
            ReleasedCaptureCount:
                roundState?.Captures.Count(capture => capture.ReleasedForVoting) ?? 0,
            VotingProgress: CreateVotingProgress(roundState));

        var personalProjection = context.RecipientUserId is Guid recipientUserId
            ? CreatePersonalProjection(roundState, recipientUserId)
            : null;

        return new GameModeProjectionPair(
            publicProjection,
            personalProjection);
    }

    private static CaptureChallengePersonalProjection CreatePersonalProjection(
        CaptureChallengeRoundState? roundState,
        Guid recipientUserId)
    {
        if (roundState is null)
        {
            return new CaptureChallengePersonalProjection(
                Status: null,
                CaptureSlots: [],
                CurrentCapture: null,
                TotalEligibleVoteCount: 0,
                SubmittedVoteCount: 0,
                VoteHistory: []);
        }

        var captureSlots =
            roundState.Status == CaptureChallengeStatus.CAPTURING
                ? CreateCaptureSlots(roundState, recipientUserId)
                : [];
        var currentCapture =
            roundState.Status == CaptureChallengeStatus.VOTING
                ? CreateCurrentCapture(roundState, recipientUserId)
                : null;
        var voteHistory =
            roundState.Status == CaptureChallengeStatus.VOTING
                ? CreateVoteHistory(roundState, recipientUserId)
                : [];
        var foreignCaptureIds = roundState.Captures
            .Where(
                capture =>
                    capture.ReleasedForVoting
                    && capture.OwnerUserId != recipientUserId)
            .Select(capture => capture.CaptureId)
            .ToHashSet();

        return new CaptureChallengePersonalProjection(
            Status: roundState.Status,
            CaptureSlots: captureSlots,
            CurrentCapture: currentCapture,
            TotalEligibleVoteCount: foreignCaptureIds.Count,
            SubmittedVoteCount:
                roundState.Votes.Count(
                    vote =>
                        vote.VoterUserId == recipientUserId
                        && foreignCaptureIds.Contains(vote.CaptureId)),
            VoteHistory: voteHistory);
    }

    private static IReadOnlyList<CaptureChallengeCaptureSlotProjection>
        CreateCaptureSlots(
            CaptureChallengeRoundState roundState,
            Guid recipientUserId) =>
        roundState.Goals
            .OrderBy(goal => goal.DisplayOrder)
            .Select(
                goal =>
                {
                    var capture = roundState.FindCapture(recipientUserId, goal.Id);
                    return new CaptureChallengeCaptureSlotProjection(
                        Goal: CreateGoalProjection(goal),
                        CaptureId: capture?.CaptureId,
                        Position:
                            capture is null
                                ? null
                                : CaptureChallengeRoundState.CopyPosition(capture.Position),
                        SubmittedAt: capture?.SubmittedAt,
                        UpdatedAt: capture?.UpdatedAt);
                })
            .ToArray();

    private static CaptureChallengeCurrentCaptureProjection?
        CreateCurrentCapture(
            CaptureChallengeRoundState roundState,
            Guid recipientUserId)
    {
        var capture = roundState.FindCurrentVotingCapture();
        var sequence = roundState.CurrentCaptureSequence;
        if (capture is null || sequence is null)
        {
            return null;
        }

        var goal = roundState.FindGoal(capture.RoundGoalId);
        if (goal is null)
        {
            throw new InvalidOperationException(
                "The current voting capture must reference a round goal.");
        }

        return new CaptureChallengeCurrentCaptureProjection(
            CaptureId: capture.CaptureId,
            OwnerUserId: capture.OwnerUserId,
            Goal: CreateGoalProjection(goal),
            Position: CaptureChallengeRoundState.CopyPosition(capture.Position),
            Sequence: sequence.Value,
            IsOwner: capture.OwnerUserId == recipientUserId,
            SelectedValue:
                roundState.FindVote(capture.CaptureId, recipientUserId)?.Value);
    }

    private static IReadOnlyList<CaptureChallengeVoteHistoryEntryProjection>
        CreateVoteHistory(
            CaptureChallengeRoundState roundState,
            Guid recipientUserId)
    {
        var currentSequence = roundState.CurrentCaptureSequence
            ?? roundState.VotingCaptureIds.Count + 1;
        return roundState.Votes
            .Where(vote => vote.VoterUserId == recipientUserId)
            .Select(
                vote => new
                {
                    Vote = vote,
                    Sequence = roundState.FindVotingSequence(vote.CaptureId)
                })
            .Where(entry => entry.Sequence > 0 && entry.Sequence < currentSequence)
            .OrderBy(entry => entry.Sequence)
            .Select(
                entry => new CaptureChallengeVoteHistoryEntryProjection(
                    entry.Vote.CaptureId,
                    entry.Sequence,
                    entry.Vote.Value))
            .ToArray();
    }

    private static IReadOnlyList<CaptureChallengeParticipantCaptureProgress>
        CreateParticipantCaptureProgress(CaptureChallengeRoundState? roundState)
    {
        if (roundState?.Status != CaptureChallengeStatus.CAPTURING)
        {
            return [];
        }

        return roundState.Participants
            .OrderBy(participant => participant.JoinOrder)
            .Select(
                participant =>
                    new CaptureChallengeParticipantCaptureProgress(
                        participant.UserId,
                        roundState.Captures.Count(
                            capture => capture.OwnerUserId == participant.UserId),
                        roundState.Goals.Count))
            .ToArray();
    }

    private static CaptureChallengeVotingProgress? CreateVotingProgress(
        CaptureChallengeRoundState? roundState)
    {
        if (roundState?.Status != CaptureChallengeStatus.VOTING)
        {
            return null;
        }

        return new CaptureChallengeVotingProgress(
            SubmittedVoteCount: roundState.Votes.Count,
            EligibleVoteCount:
                roundState.Captures
                    .Where(capture => capture.ReleasedForVoting)
                    .Sum(roundState.CountEligibleVoters));
    }

    private static CaptureChallengeGoalProjection CreateGoalProjection(
        CaptureChallengeGoal goal) =>
        new(
            GoalId: goal.Id,
            Title: goal.Title,
            DisplayOrder: goal.DisplayOrder,
            ScoreFactor: goal.ScoreFactor);
}
