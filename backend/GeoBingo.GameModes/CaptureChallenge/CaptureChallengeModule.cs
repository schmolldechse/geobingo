using System;
using System.Collections.Generic;
using System.Linq;
using GeoBingo.Contracts.Common;
using GeoBingo.GameModes.Abstractions;

namespace GeoBingo.GameModes.CaptureChallenge;

public sealed class CaptureChallengeModule : IGameModeModule
{
    public string Key => CaptureChallengeMetadata.Key;

    public string DisplayName => CaptureChallengeMetadata.DisplayName;

    public string Description => CaptureChallengeMetadata.Description;

    public Version Version => CaptureChallengeMetadata.Version;

    public int Order => CaptureChallengeMetadata.Order;

    public IGameModeLobbyState CreateDefaultLobbyState() => CaptureChallengeLobbyState.CreateDefault();

    public GameModeValidationResult ValidateLobbyState(
        IGameModeLobbyState state,
        GameModeValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        var captureChallengeState = RequireLobbyState(state);
        var issues = new List<GameModeValidationIssue>();
        ValidateSettings(captureChallengeState, issues);
        ValidateGoals(captureChallengeState, issues);

        if (context.ActiveParticipantCount < 0)
        {
            AddIssue(
                issues,
                "activeParticipantCount",
                "The active participant count cannot be negative.");
        }
        else
        {
            ValidateRoundStart(
                captureChallengeState,
                context.ActiveParticipantCount,
                issues);
        }

        return new GameModeValidationResult(
            IsValid: issues.Count == 0,
            Issues: issues.ToArray());
    }

    public GameModeRoundCreation CreateRound(
        IGameModeLobbyState state,
        GameModeRoundCreationContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        var captureChallengeState = RequireLobbyState(state);
        ValidateRoundCreationContext(context);

        var validationResult = ValidateLobbyState(
            captureChallengeState,
            new GameModeValidationContext(context.Participants.Count));
        if (!validationResult.IsValid)
        {
            throw CreateValidationException(
                "The Capture Challenge round cannot start.",
                validationResult.Issues);
        }

        var roundState = new CaptureChallengeRoundState(
            context.RoundId,
            context.RoundNumber,
            Key,
            Version,
            captureChallengeState.Settings,
            captureChallengeState.CreateGoalSnapshot(),
            context.Participants,
            context.CreatedAt);
        return new GameModeRoundCreation(roundState);
    }

    public GameModeProjectionPair CreateProjections(GameModeProjectionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var lobbyState = RequireLobbyState(context.LobbyState);
        var roundState = context.RoundState switch
        {
            null => null,
            CaptureChallengeRoundState captureChallengeRoundState =>
                captureChallengeRoundState,
            _ => throw new ArgumentException(
                "The round state is not a Capture Challenge state.",
                nameof(context))
        };
        var roundStartValidation = ValidateLobbyState(
            lobbyState,
            new GameModeValidationContext(context.ActiveParticipantCount));

        return CaptureChallengeProjectionFactory.Create(
            lobbyState,
            roundState,
            context,
            roundStartValidation.Issues);
    }

    public GameModeAdvanceOutcome Advance(GameModeAdvanceContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _ = RequireLobbyState(context.LobbyState);
        var roundState = context.RoundState as CaptureChallengeRoundState
            ?? throw new ArgumentException(
                "The round state is not a Capture Challenge state.",
                nameof(context));

        if (roundState.Status is null)
        {
            var captureDeadline = roundState.EnterCapturing(context.Now);
            return new GameModeAdvanceOutcome(
                GameModeAdvanceKind.STATE_CHANGED,
                captureDeadline);
        }

        if (roundState.Status == Contracts.GameModes.CaptureChallenge.CaptureChallengeStatus.CAPTURING)
        {
            var captureDeadline = roundState.CaptureEndsAt ?? throw new InvalidOperationException("A capturing round must have a capture deadline.");
            if (context.Now < captureDeadline)
            {
                return new GameModeAdvanceOutcome(
                    GameModeAdvanceKind.NO_CHANGE,
                    captureDeadline);
            }

            var votingDeadline = roundState.EnterVoting(context.Now);
            return new GameModeAdvanceOutcome(
                GameModeAdvanceKind.STATE_CHANGED,
                votingDeadline);
        }

        if (roundState.Status == Contracts.GameModes.CaptureChallenge.CaptureChallengeStatus.VOTING)
        {
            var votingDeadline = roundState.VotingEndsAt
                ?? throw new InvalidOperationException(
                    "A voting round must have a voting deadline.");
            return new GameModeAdvanceOutcome(
                GameModeAdvanceKind.NO_CHANGE,
                context.Now < votingDeadline ? votingDeadline : null);
        }

        throw new InvalidOperationException("The Capture Challenge round has an unsupported status.");
    }

    public void HandleParticipantWithdrawal(
        GameModeParticipantWithdrawalContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _ = RequireLobbyState(context.LobbyState);
        var roundState = context.RoundState
            as CaptureChallengeRoundState
            ?? throw new ArgumentException(
                "The round state is not a Capture Challenge state.",
                nameof(context));

        if (context.ParticipantUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "The participant user identifier cannot be empty.",
                nameof(context));
        }

        if (!roundState.IsParticipant(context.ParticipantUserId))
        {
            throw new ArgumentException(
                "The selected user is not a participant of this round.",
                nameof(context));
        }

        switch (roundState.Status)
        {
            case null:
                return;
            case Contracts.GameModes.CaptureChallenge
                .CaptureChallengeStatus.CAPTURING:
                roundState.RemoveCapturesOwnedBy(
                    context.ParticipantUserId);
                return;
            case Contracts.GameModes.CaptureChallenge
                .CaptureChallengeStatus.VOTING:
                return;
            default:
                throw new InvalidOperationException(
                    "The Capture Challenge round has an unsupported status.");
        }
    }

    private static CaptureChallengeLobbyState RequireLobbyState(IGameModeLobbyState state) => state as CaptureChallengeLobbyState
        ?? throw new ArgumentException("The lobby state is not a Capture Challenge state.", nameof(state));

    private static void ValidateSettings(
        CaptureChallengeLobbyState state,
        ICollection<GameModeValidationIssue> issues)
    {
        var settings = state.Settings;
        if (settings.CaptureDurationSeconds
            is < CaptureChallengeDefaults.MinimumCaptureDurationSeconds
            or > CaptureChallengeDefaults.MaximumCaptureDurationSeconds)
        {
            AddIssue(
                issues,
                "settings.captureDurationSeconds",
                $"Capture duration must be between {CaptureChallengeDefaults.MinimumCaptureDurationSeconds} and {CaptureChallengeDefaults.MaximumCaptureDurationSeconds} seconds.");
        }

        if (settings.SecondsPerVote
            is < CaptureChallengeDefaults.MinimumSecondsPerVote
            or > CaptureChallengeDefaults.MaximumSecondsPerVote)
        {
            AddIssue(
                issues,
                "settings.secondsPerVote",
                $"Seconds per vote must be between {CaptureChallengeDefaults.MinimumSecondsPerVote} and {CaptureChallengeDefaults.MaximumSecondsPerVote}.");
        }
    }

    private static void ValidateGoals(
        CaptureChallengeLobbyState state,
        ICollection<GameModeValidationIssue> issues)
    {
        var goals = state.Goals;
        if (goals.Count > 25)
        {
            AddIssue(
                issues,
                "goals",
                "A Capture Challenge lobby may contain at most 25 goals.");
        }

        if (goals.Select(goal => goal.Id).Distinct().Count() != goals.Count)
        {
            AddIssue(
                issues,
                "goals",
                "Every goal identifier must be unique.");
        }

        var orderedGoals = goals
            .OrderBy(goal => goal.DisplayOrder)
            .ToArray();
        for (var index = 0; index < orderedGoals.Length; index++)
        {
            var goal = orderedGoals[index];
            var memberPrefix = $"goals[{index}]";

            if (goal.Id == Guid.Empty)
            {
                AddIssue(
                    issues,
                    $"{memberPrefix}.goalId",
                    "The goal identifier cannot be empty.");
            }

            if (goal.DisplayOrder != index)
            {
                AddIssue(
                    issues,
                    $"{memberPrefix}.displayOrder",
                    "Goal display order must be contiguous and start at zero.");
            }

            if (string.IsNullOrWhiteSpace(goal.Title)
                || goal.Title.Length > 80
                || !string.Equals(goal.Title, goal.Title.Trim(), StringComparison.Ordinal))
            {
                AddIssue(
                    issues,
                    $"{memberPrefix}.title",
                    "The goal title must be trimmed and contain between 1 and 80 characters.");
            }

            if (goal.ScoreFactor is < 0m or > 10m
                || goal.ScoreFactor % CaptureChallengeDefaults.ScoreFactorStep != 0m)
            {
                AddIssue(
                    issues,
                    $"{memberPrefix}.scoreFactor",
                    "The score factor must be between 0 and 10 in increments of 0.5.");
            }
        }
    }

    private static void ValidateRoundStart(
        CaptureChallengeLobbyState state,
        int activeParticipantCount,
        ICollection<GameModeValidationIssue> issues)
    {
        if (state.Goals.Count < 1)
        {
            AddIssue(
                issues,
                "goals",
                $"At least 1 goal is required to start a round.");
        }

        if (activeParticipantCount < GameModeDefaults.MinimumParticipants)
        {
            AddIssue(
                issues,
                "activeParticipantCount",
                $"At least {GameModeDefaults.MinimumParticipants} active participants are required to start a round.");
        }

        var captureSlotCount = (long)activeParticipantCount * state.Goals.Count;
        if (captureSlotCount > CaptureChallengeDefaults.MaximumCaptureSlots)
        {
            AddIssue(
                issues,
                "goals",
                $"A round may contain at most {CaptureChallengeDefaults.MaximumCaptureSlots} capture slots.");
        }
    }

    private static void ValidateRoundCreationContext(
        GameModeRoundCreationContext context)
    {
        if (context.RoundId == Guid.Empty)
        {
            throw new ArgumentException(
                "The round identifier cannot be empty.",
                nameof(context));
        }

        if (context.RoundNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(context),
                "The round number must be positive.");
        }

        if (context.Participants is null)
        {
            throw new ArgumentException(
                "Round participants are required.",
                nameof(context));
        }

        if (context.Participants.Select(participant => participant.UserId).Distinct().Count()
            != context.Participants.Count)
        {
            throw new ArgumentException(
                "Round participant identifiers must be unique.",
                nameof(context));
        }

        if (context.Participants.Select(participant => participant.JoinOrder).Distinct().Count()
            != context.Participants.Count)
        {
            throw new ArgumentException(
                "Round participant join-order values must be unique.",
                nameof(context));
        }
    }

    private static void AddIssue(
        ICollection<GameModeValidationIssue> issues,
        string memberName,
        string message) =>
        issues.Add(
            new GameModeValidationIssue(
                ErrorCode.VALIDATION_FAILED,
                message,
                memberName));

    private static GameModeDomainException CreateValidationException(
        string message,
        IReadOnlyList<GameModeValidationIssue> issues) =>
        new(
            ErrorCode.VALIDATION_FAILED,
            message,
            issues
                .GroupBy(issue => issue.MemberName ?? "state", StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(issue => issue.Message).ToArray(),
                    StringComparer.Ordinal));
}
