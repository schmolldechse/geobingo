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
    private readonly List<CaptureChallengeVote> _votes = [];
    private readonly ReadOnlyCollection<CaptureChallengeVote> _votesView;
    private readonly HashSet<Guid> _eligibleVoterIds = [];
    private readonly List<Guid> _votingCaptureIds = [];
    private readonly ReadOnlyCollection<Guid> _votingCaptureIdsView;
    private int currentVotingCaptureIndex = -1;

    internal CaptureChallengeRoundState(
        Guid roundId,
        int roundNumber,
        string modeKey,
        Version modeVersion,
        CaptureChallengeSettings settings,
        IEnumerable<CaptureChallengeGoal> goals,
        IEnumerable<GameModeParticipant> participants,
        Guid votingOrderSeed,
        DateTimeOffset createdAt)
    {
        if (roundId == Guid.Empty)
        {
            throw new ArgumentException(
                "The round identifier cannot be empty.",
                nameof(roundId));
        }

        if (roundNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(roundNumber),
                "The round number must be positive.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(modeKey);
        ArgumentNullException.ThrowIfNull(modeVersion);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(goals);
        ArgumentNullException.ThrowIfNull(participants);
        if (votingOrderSeed == Guid.Empty)
        {
            throw new ArgumentException(
                "The voting-order seed cannot be empty.",
                nameof(votingOrderSeed));
        }

        RoundId = roundId;
        RoundNumber = roundNumber;
        ModeKey = modeKey;
        ModeVersion = modeVersion;
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
        VotingOrderSeed = votingOrderSeed;
        CreatedAt = createdAt;
        _capturesView = _captures.AsReadOnly();
        _votesView = _votes.AsReadOnly();
        _votingCaptureIdsView = _votingCaptureIds.AsReadOnly();
    }

    public Guid RoundId { get; }

    public int RoundNumber { get; }

    public string ModeKey { get; }

    public Version ModeVersion { get; }

    public CaptureChallengeSettings Settings =>
        CaptureChallengeLobbyState.CopySettings(_settings);

    public DateTimeOffset CreatedAt { get; }

    internal Guid VotingOrderSeed { get; }

    public DateTimeOffset? PlayingStartedAt { get; private set; }

    public CaptureChallengeStatus? Status { get; private set; }

    public DateTimeOffset? CaptureEndsAt { get; private set; }

    public DateTimeOffset? VotingStartedAt { get; private set; }

    public DateTimeOffset? CurrentCaptureEndsAt { get; private set; }

    public DateTimeOffset? VotingEndsAt { get; private set; }

    internal IReadOnlyList<CaptureChallengeGoal> Goals => _goals;

    internal IReadOnlyList<GameModeParticipant> Participants => _participants;

    internal IReadOnlyList<CaptureChallengeCapture> Captures => _capturesView;

    internal IReadOnlyList<CaptureChallengeVote> Votes => _votesView;

    internal IReadOnlyList<Guid> VotingCaptureIds => _votingCaptureIdsView;

    internal IReadOnlySet<Guid> EligibleVoterIds => _eligibleVoterIds;

    internal int? CurrentCaptureSequence =>
        currentVotingCaptureIndex >= 0
        && currentVotingCaptureIndex < _votingCaptureIds.Count
            ? currentVotingCaptureIndex + 1
            : null;

    internal bool IsParticipant(Guid userId) =>
        _participants.Any(participant => participant.UserId == userId);

    internal void RemoveCapturesOwnedBy(Guid userId) =>
        _captures.RemoveAll(capture => capture.OwnerUserId == userId);

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

    internal CaptureChallengeCapture? FindCurrentVotingCapture() =>
        CurrentCaptureSequence is null
            ? null
            : FindCapture(_votingCaptureIds[currentVotingCaptureIndex]);

    internal CaptureChallengeVote? FindVote(
        Guid captureId,
        Guid voterUserId) =>
        _votes.Find(
            vote =>
                vote.CaptureId == captureId
                && vote.VoterUserId == voterUserId);

    internal int FindVotingSequence(Guid captureId)
    {
        var index = _votingCaptureIds.IndexOf(captureId);
        return index < 0 ? 0 : index + 1;
    }

    internal DateTimeOffset EnterCapturing(DateTimeOffset now)
    {
        if (Status is not null
            || PlayingStartedAt is not null
            || CaptureEndsAt is not null
            || VotingStartedAt is not null
            || CurrentCaptureEndsAt is not null
            || VotingEndsAt is not null)
        {
            throw new InvalidOperationException(
                "Only a prepared Capture Challenge round may enter capturing.");
        }

        PlayingStartedAt = now;
        Status = CaptureChallengeStatus.CAPTURING;
        CaptureEndsAt = PlayingStartedAt.Value.AddSeconds(
            _settings.CaptureDurationSeconds);
        return CaptureEndsAt.Value;
    }

    internal DateTimeOffset EnterVoting(
        IReadOnlySet<Guid> currentMemberUserIds,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(currentMemberUserIds);
        if (Status != CaptureChallengeStatus.CAPTURING
            || CaptureEndsAt is not DateTimeOffset captureEndsAt
            || now < captureEndsAt
            || VotingStartedAt is not null
            || VotingEndsAt is not null
            || _eligibleVoterIds.Count != 0
            || _votingCaptureIds.Count != 0
            || _votes.Count != 0
            || _captures.Any(capture => capture.ReleasedForVoting))
        {
            throw new InvalidOperationException(
                "Only a completed, unfrozen capture phase may enter voting.");
        }

        if (currentMemberUserIds.Any(userId => userId == Guid.Empty))
        {
            throw new ArgumentException(
                "Current member identifiers cannot be empty.",
                nameof(currentMemberUserIds));
        }

        var eligibleVoterIds = _participants
            .Where(participant => currentMemberUserIds.Contains(participant.UserId))
            .OrderBy(participant => participant.JoinOrder)
            .Select(participant => participant.UserId)
            .ToArray();
        var eligibleVoterIdSet = eligibleVoterIds.ToHashSet();
        var releasedCaptures = _captures
            .Where(capture => eligibleVoterIdSet.Contains(capture.OwnerUserId))
            .Select(
                capture => capture with
                {
                    Position = CopyPosition(capture.Position),
                    ReleasedForVoting = true
                })
            .ToArray();
        var votingCaptureIds = CaptureChallengeVotingOrderFactory.CreateOrder(
            RoundId,
            VotingOrderSeed,
            releasedCaptures);

        _captures.Clear();
        _captures.AddRange(releasedCaptures);
        _eligibleVoterIds.UnionWith(eligibleVoterIds);
        _votingCaptureIds.AddRange(votingCaptureIds);
        Status = CaptureChallengeStatus.VOTING;
        VotingStartedAt = now;
        currentVotingCaptureIndex = 0;
        VotingEndsAt = now.AddSeconds(
            checked(votingCaptureIds.Count * _settings.SecondsPerVote));
        CurrentCaptureEndsAt = votingCaptureIds.Count == 0
            ? now
            : CalculateSlotDeadline(currentVotingCaptureIndex);
        return CurrentCaptureEndsAt.Value;
    }

    internal CaptureChallengeVotingAdvance AdvanceVoting(DateTimeOffset now)
    {
        if (Status != CaptureChallengeStatus.VOTING
            || VotingStartedAt is not DateTimeOffset votingStartedAt
            || VotingEndsAt is not DateTimeOffset votingEndsAt
            || CurrentCaptureEndsAt is null)
        {
            throw new InvalidOperationException(
                "Only an active voting phase may advance.");
        }

        if (_votingCaptureIds.Count == 0 || now >= votingEndsAt)
        {
            var changed = currentVotingCaptureIndex != _votingCaptureIds.Count;
            currentVotingCaptureIndex = _votingCaptureIds.Count;
            CurrentCaptureEndsAt = null;
            return new CaptureChallengeVotingAdvance(
                Completed: true,
                StateChanged: changed,
                NextDeadline: null);
        }

        var slotTicks = TimeSpan.FromSeconds(_settings.SecondsPerVote).Ticks;
        var elapsedTicks = Math.Max(0, (now - votingStartedAt).Ticks);
        var targetIndex = (int)Math.Min(
            _votingCaptureIds.Count - 1L,
            elapsedTicks / slotTicks);
        var stateChanged = targetIndex != currentVotingCaptureIndex;
        currentVotingCaptureIndex = targetIndex;
        CurrentCaptureEndsAt = CalculateSlotDeadline(targetIndex);
        return new CaptureChallengeVotingAdvance(
            Completed: false,
            StateChanged: stateChanged,
            NextDeadline: CurrentCaptureEndsAt);
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

    internal void AddVote(CaptureChallengeVote vote) =>
        _votes.Add(vote);

    internal void ReplaceVote(CaptureChallengeVote vote)
    {
        var index = _votes.FindIndex(
            existing =>
                existing.CaptureId == vote.CaptureId
                && existing.VoterUserId == vote.VoterUserId);
        _votes[index] = vote;
    }

    internal int CountEligibleVoters(CaptureChallengeCapture capture) =>
        _eligibleVoterIds.Count(userId => userId != capture.OwnerUserId);

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

    private DateTimeOffset CalculateSlotDeadline(int slotIndex) =>
        VotingStartedAt!.Value.AddSeconds(
            checked((slotIndex + 1) * _settings.SecondsPerVote));
}

internal sealed record CaptureChallengeVotingAdvance(
    bool Completed,
    bool StateChanged,
    DateTimeOffset? NextDeadline);

internal sealed record CaptureChallengeCapture(
    Guid CaptureId,
    Guid RoundId,
    Guid RoundGoalId,
    Guid OwnerUserId,
    StreetViewPosition Position,
    DateTimeOffset SubmittedAt,
    DateTimeOffset UpdatedAt,
    bool ReleasedForVoting);

internal sealed record CaptureChallengeVote(
    Guid CaptureId,
    Guid VoterUserId,
    VoteValue Value,
    DateTimeOffset UpdatedAt);
