using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GeoBingo.Contracts.GameModes.CaptureChallenge;
using GeoBingo.GameModes.Abstractions;

namespace GeoBingo.GameModes.CaptureChallenge;

public sealed class CaptureChallengeRoundState : IGameModeRoundState
{
    private readonly CaptureChallengeSettings _settings;
    private readonly ReadOnlyCollection<CaptureChallengeGoal> _goals;
    private readonly ReadOnlyCollection<GameModeParticipant> _participants;
    private readonly List<CaptureChallengeCapture> _captures = [];
    private readonly ReadOnlyCollection<CaptureChallengeCapture> _capturesView;
    private readonly List<CaptureChallengeAssignment> _assignments = [];
    private readonly ReadOnlyCollection<CaptureChallengeAssignment> _assignmentsView;
    private readonly List<CaptureChallengeVote> _votes = [];
    private readonly ReadOnlyCollection<CaptureChallengeVote> _votesView;
    private readonly HashSet<Guid> _eligibleVoterIds = [];

    internal CaptureChallengeRoundState(
        Guid roundId,
        CaptureChallengeSettings settings,
        IEnumerable<CaptureChallengeGoal> goals,
        IEnumerable<GameModeParticipant> participants,
        DateTimeOffset createdAt)
    {
        RoundId = roundId;
        _settings = CaptureChallengeLobbyState.CopySettings(settings);
        _goals = Array.AsReadOnly(
            goals
                .OrderBy(goal => goal.DisplayOrder)
                .Select(CaptureChallengeLobbyState.CopyGoal)
                .ToArray());
        _participants = Array.AsReadOnly(
            participants
                .OrderBy(participant => participant.JoinOrder)
                .Select(participant => participant with { })
                .ToArray());
        CreatedAt = createdAt;
        _capturesView = _captures.AsReadOnly();
        _assignmentsView = _assignments.AsReadOnly();
        _votesView = _votes.AsReadOnly();
    }

    public Guid RoundId { get; }

    public CaptureChallengeSettings Settings =>
        CaptureChallengeLobbyState.CopySettings(_settings);

    public DateTimeOffset CreatedAt { get; }

    public CaptureChallengeStatus? Status { get; private set; }

    public DateTimeOffset? CaptureEndsAt { get; private set; }

    public DateTimeOffset? VotingEndsAt { get; private set; }

    internal IReadOnlyList<CaptureChallengeGoal> Goals => _goals;

    internal IReadOnlyList<GameModeParticipant> Participants => _participants;

    internal IReadOnlyList<CaptureChallengeCapture> Captures => _capturesView;

    internal IReadOnlyList<CaptureChallengeAssignment> Assignments => _assignmentsView;

    internal IReadOnlyList<CaptureChallengeVote> Votes => _votesView;

    internal IReadOnlySet<Guid> EligibleVoterIds => _eligibleVoterIds;

    internal bool AllAssignmentsCompleted =>
        _assignments.Count > 0
        && _assignments.All(assignment => assignment.CompletedAt is not null)
        && _assignments.All(
            assignment => _votes.Any(vote => vote.AssignmentId == assignment.AssignmentId));

    internal bool IsParticipant(Guid userId) =>
        _participants.Any(participant => participant.UserId == userId);

    internal void RemoveCapturesOwnedBy(Guid userId) =>
        _captures.RemoveAll(
            capture => capture.OwnerUserId == userId);

    internal CaptureChallengeGoal? FindGoal(Guid goalId) =>
        _goals.FirstOrDefault(goal => goal.Id == goalId);

    internal CaptureChallengeCapture? FindCapture(Guid captureId) =>
        _captures.Find(capture => capture.CaptureId == captureId);

    internal CaptureChallengeCapture? FindCapture(
        Guid ownerUserId,
        Guid roundGoalId) =>
        _captures.Find(
            capture =>
                capture.OwnerUserId == ownerUserId
                && capture.RoundGoalId == roundGoalId);

    internal CaptureChallengeAssignment? FindAssignment(Guid assignmentId) =>
        _assignments.Find(assignment => assignment.AssignmentId == assignmentId);

    internal CaptureChallengeAssignment? FindCurrentAssignment(Guid voterUserId) =>
        _assignments
            .Where(
                assignment =>
                    assignment.VoterUserId == voterUserId
                    && assignment.RevealedAt is not null
                    && assignment.CompletedAt is null)
            .OrderBy(assignment => assignment.Sequence)
            .FirstOrDefault();

    internal CaptureChallengeVote? FindVote(Guid assignmentId) =>
        _votes.Find(vote => vote.AssignmentId == assignmentId);

    internal DateTimeOffset EnterCapturing(DateTimeOffset now)
    {
        Status = CaptureChallengeStatus.CAPTURING;
        CaptureEndsAt = now.AddSeconds(_settings.CaptureDurationSeconds);
        VotingEndsAt = null;
        return CaptureEndsAt.Value;
    }

    internal DateTimeOffset EnterVoting(DateTimeOffset now)
    {
        var maximumAssignmentCount = _assignments
            .GroupBy(assignment => assignment.VoterUserId)
            .Select(assignments => assignments.Count())
            .DefaultIfEmpty(0)
            .Max();
        var votingDurationSeconds = Math.Clamp(
            maximumAssignmentCount * _settings.SecondsPerVote,
            60,
            30 * 60);

        Status = CaptureChallengeStatus.VOTING;
        VotingEndsAt = now.AddSeconds(votingDurationSeconds);
        return VotingEndsAt.Value;
    }

    internal void AddCapture(CaptureChallengeCapture capture) =>
        _captures.Add(capture with { Position = CopyPosition(capture.Position) });

    internal void ReplaceCapture(CaptureChallengeCapture capture)
    {
        var index = _captures.FindIndex(
            existing => existing.CaptureId == capture.CaptureId);
        _captures[index] = capture with { Position = CopyPosition(capture.Position) };
    }

    internal void RemoveCapture(Guid captureId) =>
        _captures.RemoveAll(capture => capture.CaptureId == captureId);

    internal void SetEligibleVoters(IEnumerable<Guid> voterUserIds)
    {
        _eligibleVoterIds.Clear();
        _eligibleVoterIds.UnionWith(voterUserIds);
    }

    internal void AddAssignment(CaptureChallengeAssignment assignment) =>
        _assignments.Add(assignment);

    internal void RevealFirstAssignment(Guid voterUserId, DateTimeOffset now)
    {
        var assignment = _assignments
            .Where(
                candidate =>
                    candidate.VoterUserId == voterUserId
                    && candidate.RevealedAt is null)
            .OrderBy(candidate => candidate.Sequence)
            .FirstOrDefault();

        if (assignment is null)
        {
            return;
        }

        var index = _assignments.FindIndex(
            candidate => candidate.AssignmentId == assignment.AssignmentId);
        _assignments[index] = assignment with
        {
            RevealedAt = now
        };
    }

    internal void AddVoteAndAdvance(
        CaptureChallengeVote vote,
        DateTimeOffset now)
    {
        _votes.Add(vote with { UpdatedAt = now });

        var assignmentIndex = _assignments.FindIndex(
            assignment => assignment.AssignmentId == vote.AssignmentId);
        var assignment = _assignments[assignmentIndex];
        _assignments[assignmentIndex] = assignment with { CompletedAt = now };

        RevealFirstAssignment(assignment.VoterUserId, now);
    }

    internal void ReplaceVote(CaptureChallengeVote vote)
    {
        var index = _votes.FindIndex(
            existing => existing.AssignmentId == vote.AssignmentId);
        _votes[index] = vote;
    }

    internal static StreetViewPosition CopyPosition(StreetViewPosition position) =>
        new()
        {
            PanoramaId = position.PanoramaId,
            Latitude = position.Latitude,
            Longitude = position.Longitude,
            Heading = position.Heading,
            Pitch = position.Pitch,
            Zoom = position.Zoom
        };
}

internal sealed record CaptureChallengeCapture(
    Guid CaptureId,
    Guid RoundId,
    Guid RoundGoalId,
    Guid OwnerUserId,
    StreetViewPosition Position,
    DateTimeOffset SubmittedAt,
    DateTimeOffset UpdatedAt,
    bool ReleasedForVoting);

internal sealed record CaptureChallengeAssignment(
    Guid AssignmentId,
    Guid RoundId,
    Guid VoterUserId,
    Guid CaptureId,
    int Sequence,
    DateTimeOffset? RevealedAt,
    DateTimeOffset? CompletedAt);

internal sealed record CaptureChallengeVote(
    Guid AssignmentId,
    Guid VoterUserId,
    VoteValue Value,
    DateTimeOffset UpdatedAt);
