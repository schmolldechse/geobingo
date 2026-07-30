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
                CurrentAssignment: null,
                TotalAssignmentCount: 0,
                CompletedAssignmentCount: 0,
                VoteHistory: []);
        }

        var captureSlots =
            roundState.Status == CaptureChallengeStatus.CAPTURING
                ? CreateCaptureSlots(roundState, recipientUserId)
                : [];
        var assignments = roundState.Assignments
            .Where(assignment => assignment.VoterUserId == recipientUserId)
            .OrderBy(assignment => assignment.Sequence)
            .ToArray();
        var currentAssignment =
            roundState.Status == CaptureChallengeStatus.VOTING
                ? CreateCurrentAssignment(roundState, recipientUserId)
                : null;
        var voteHistory =
            roundState.Status == CaptureChallengeStatus.VOTING
                ? CreateVoteHistory(roundState, assignments)
                : [];

        return new CaptureChallengePersonalProjection(
            Status: roundState.Status,
            CaptureSlots: captureSlots,
            CurrentAssignment: currentAssignment,
            TotalAssignmentCount: assignments.Length,
            CompletedAssignmentCount:
                assignments.Count(assignment => assignment.CompletedAt is not null),
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

    private static CaptureChallengeCurrentAssignmentProjection?
        CreateCurrentAssignment(
            CaptureChallengeRoundState roundState,
            Guid recipientUserId)
    {
        var assignment = roundState.FindCurrentAssignment(recipientUserId);
        if (assignment is null)
        {
            return null;
        }

        var capture = roundState.FindCapture(assignment.CaptureId);
        if (capture is null)
        {
            return null;
        }

        var goal = roundState.FindGoal(capture.RoundGoalId);
        if (goal is null)
        {
            return null;
        }

        return new CaptureChallengeCurrentAssignmentProjection(
            AssignmentId: assignment.AssignmentId,
            Goal: CreateGoalProjection(goal),
            Position: CaptureChallengeRoundState.CopyPosition(capture.Position),
            Sequence: assignment.Sequence);
    }

    private static IReadOnlyList<CaptureChallengeVoteHistoryEntryProjection>
        CreateVoteHistory(
            CaptureChallengeRoundState roundState,
            IEnumerable<CaptureChallengeAssignment> assignments) =>
        assignments
            .Select(
                assignment =>
                    (
                        Assignment: assignment,
                        Vote: roundState.FindVote(assignment.AssignmentId)))
            .Where(entry => entry.Vote is not null)
            .Select(
                entry =>
                    new CaptureChallengeVoteHistoryEntryProjection(
                        entry.Assignment.AssignmentId,
                        entry.Vote!.Value))
            .ToArray();

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
            CompletedAssignmentCount:
                roundState.Assignments.Count(
                    assignment => assignment.CompletedAt is not null),
            TotalAssignmentCount: roundState.Assignments.Count);
    }

    private static CaptureChallengeGoalProjection CreateGoalProjection(
        CaptureChallengeGoal goal) =>
        new(
            GoalId: goal.Id,
            Title: goal.Title,
            DisplayOrder: goal.DisplayOrder,
            ScoreFactor: goal.ScoreFactor);
}
