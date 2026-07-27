using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.GameModes.CaptureChallenge;
using GeoBingo.Contracts.Lobbies;
using GeoBingo.GameModes.Abstractions;

namespace GeoBingo.GameModes.CaptureChallenge;

public sealed class CaptureChallengeOperations
{
    public void UpdateSettings(
        CaptureChallengeLobbyState state,
        UpdateCaptureChallengeSettingsRequest request,
        LobbyOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        EnsureWaitingHost(context);
        var settings = ValidateSettings(request.Settings);
        state.ReplaceSettings(settings);
    }

    public void AddGoal(
        CaptureChallengeLobbyState state,
        AddCaptureGoalRequest request,
        LobbyOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        EnsureWaitingHost(context);

        var goalData = ValidateGoal(request.Goal);
        state.AppendGoal(
            new CaptureChallengeGoal(
                Guid.NewGuid(),
                goalData.Title,
                goalData.Description,
                state.Goals.Count,
                goalData.ScoreFactor));
    }

    public void UpdateGoal(
        CaptureChallengeLobbyState state,
        UpdateCaptureGoalRequest request,
        LobbyOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        EnsureWaitingHost(context);
        var existingGoal = state.FindGoal(request.GoalId);
        if (existingGoal is null)
        {
            ThrowValidation("goalId", "The selected goal does not exist.");
        }

        var goalData = ValidateGoal(request.Goal);
        state.ReplaceGoal(
            new CaptureChallengeGoal(
                existingGoal.Id,
                goalData.Title,
                goalData.Description,
                existingGoal.DisplayOrder,
                goalData.ScoreFactor));
    }

    public void RemoveGoal(
        CaptureChallengeLobbyState state,
        RemoveCaptureGoalRequest request,
        LobbyOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        EnsureWaitingHost(context);
        if (state.FindGoal(request.GoalId) is null)
        {
            ThrowValidation("goalId", "The selected goal does not exist.");
        }

        state.RemoveGoal(request.GoalId);
    }

    public void ReorderGoals(
        CaptureChallengeLobbyState state,
        ReorderCaptureGoalsRequest request,
        LobbyOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        EnsureWaitingHost(context);
        var requestedGoalIds = request.GoalIdsInDisplayOrder;
        if (requestedGoalIds is null)
        {
            ThrowValidation(
                "goalIdsInDisplayOrder",
                "The display order is required.");
        }

        var currentGoalIds = state.Goals
            .Select(goal => goal.Id)
            .ToHashSet();

        if (requestedGoalIds.Count == 0
            || requestedGoalIds.Count != currentGoalIds.Count
            || requestedGoalIds.Distinct().Count() != requestedGoalIds.Count
            || requestedGoalIds.Any(goalId => !currentGoalIds.Contains(goalId)))
        {
            ThrowValidation(
                "goalIdsInDisplayOrder",
                "The display order must contain every current goal identifier exactly once.");
        }

        state.ReplaceGoalDisplayOrder(requestedGoalIds);
    }

    public void SubmitCapture(
        CaptureChallengeRoundState state,
        SubmitCaptureRequest request,
        LobbyOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        EnsureRoundParticipant(
            state,
            context,
            CaptureChallengeStatus.CAPTURING);
        if (state.FindCapture(context.ActorUserId, request.RoundGoalId) is not null)
        {
            throw new GameModeDomainException(
                ErrorCode.CAPTURE_ALREADY_EXISTS,
                "A capture already exists for this goal.");
        }

        EnsureCaptureDeadline(state, context);
        if (state.FindGoal(request.RoundGoalId) is null)
        {
            ThrowValidation(
                "roundGoalId",
                "The selected goal does not belong to the active round.");
        }

        var position = ValidatePosition(request.Position);
        state.AddCapture(
            new CaptureChallengeCapture(
                Guid.NewGuid(),
                state.RoundId,
                request.RoundGoalId,
                context.ActorUserId,
                position,
                context.Now,
                context.Now,
                ReleasedForVoting: false));
    }

    public void UpdateCapture(
        CaptureChallengeRoundState state,
        UpdateCaptureRequest request,
        LobbyOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        EnsureRoundParticipant(
            state,
            context,
            CaptureChallengeStatus.CAPTURING);
        var existingCapture = FindOwnedCapture(
            state,
            request.CaptureId,
            context.ActorUserId);
        EnsureCaptureDeadline(state, context);

        var position = ValidatePosition(request.Position);
        state.ReplaceCapture(
            existingCapture with
            {
                Position = position,
                UpdatedAt = context.Now
            });
    }

    public void RemoveCapture(
        CaptureChallengeRoundState state,
        RemoveCaptureRequest request,
        LobbyOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        EnsureRoundParticipant(
            state,
            context,
            CaptureChallengeStatus.CAPTURING);
        var existingCapture = FindOwnedCapture(
            state,
            request.CaptureId,
            context.ActorUserId);
        EnsureCaptureDeadline(state, context);

        state.RemoveCapture(existingCapture.CaptureId);
    }

    public void CastVote(
        CaptureChallengeRoundState state,
        CastVoteRequest request,
        LobbyOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        EnsureVotingParticipant(state, context);
        var assignment = FindOwnedAssignment(
            state,
            request.AssignmentId,
            context.ActorUserId);
        var currentAssignment = state.FindCurrentAssignment(context.ActorUserId);
        if (assignment.RevealedAt is null
            || assignment.CompletedAt is not null
            || currentAssignment?.AssignmentId != assignment.AssignmentId)
        {
            throw new GameModeDomainException(
                ErrorCode.ASSIGNMENT_NOT_FOUND,
                "The active voting assignment was not found.");
        }

        if (state.FindVote(assignment.AssignmentId) is not null)
        {
            throw new GameModeDomainException(
                ErrorCode.VOTE_ALREADY_EXISTS,
                "A vote already exists for this assignment.");
        }

        EnsureVotingDeadline(state, context);
        ValidateVoteValue(request.Value);
        state.AddVoteAndAdvance(
            new CaptureChallengeVote(
                assignment.AssignmentId,
                context.ActorUserId,
                request.Value,
                context.Now),
            context.Now);
    }

    public void ChangeVote(
        CaptureChallengeRoundState state,
        ChangeVoteRequest request,
        LobbyOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        EnsureVotingParticipant(state, context);
        var assignment = FindOwnedAssignment(
            state,
            request.AssignmentId,
            context.ActorUserId);
        if (assignment.RevealedAt is null
            || assignment.CompletedAt is null)
        {
            throw new GameModeDomainException(
                ErrorCode.VOTE_NOT_FOUND,
                "A completed vote was not found for this assignment.");
        }

        var existingVote = state.FindVote(assignment.AssignmentId);
        if (existingVote is null)
        {
            throw new GameModeDomainException(
                ErrorCode.VOTE_NOT_FOUND,
                "A completed vote was not found for this assignment.");
        }

        EnsureVotingDeadline(state, context);
        ValidateVoteValue(request.Value);
        state.ReplaceVote(
            existingVote with
            {
                Value = request.Value,
                UpdatedAt = context.Now
            });
    }

    private static void EnsureWaitingHost(LobbyOperationContext context)
    {
        EnsureSelectedMode(context);
        if (context.LobbyStatus != LobbyStatus.WAITING)
        {
            throw new GameModeDomainException(
                ErrorCode.INVALID_LOBBY_STATUS,
                "This operation is available only while the lobby is waiting.");
        }

        EnsureActorMayMutate(context);
        if (context.ActorUserId != context.HostUserId)
        {
            throw new GameModeDomainException(
                ErrorCode.HOST_ONLY,
                "Only the current lobby host may perform this operation.");
        }
    }

    private static void EnsureRoundParticipant(
        CaptureChallengeRoundState state,
        LobbyOperationContext context,
        CaptureChallengeStatus requiredStatus)
    {
        EnsureSelectedMode(context);
        if (context.LobbyStatus != LobbyStatus.PLAYING)
        {
            throw new GameModeDomainException(
                ErrorCode.INVALID_LOBBY_STATUS,
                "This operation is available only while a round is playing.");
        }

        if (state.Status != requiredStatus)
        {
            throw new GameModeDomainException(
                ErrorCode.INVALID_MODE_STATUS,
                "This operation is not available in the current game-mode phase.");
        }

        EnsureActorMayMutate(context);
        if (!state.IsParticipant(context.ActorUserId))
        {
            throw new GameModeDomainException(
                ErrorCode.NOT_A_MEMBER,
                "The current user is not an active round participant.");
        }
    }

    private static void EnsureVotingParticipant(
        CaptureChallengeRoundState state,
        LobbyOperationContext context)
    {
        EnsureRoundParticipant(
            state,
            context,
            CaptureChallengeStatus.VOTING);
        if (!state.EligibleVoterIds.Contains(context.ActorUserId))
        {
            throw new GameModeDomainException(
                ErrorCode.FORBIDDEN,
                "The current user is not eligible to vote in this round.");
        }
    }

    private static void EnsureSelectedMode(LobbyOperationContext context)
    {
        if (!string.Equals(
                context.SelectedModeKey,
                CaptureChallengeMetadata.Key,
                StringComparison.Ordinal))
        {
            throw new GameModeDomainException(
                ErrorCode.MODE_OPERATION_NOT_SUPPORTED,
                "This operation is not supported by the selected game mode.");
        }

        if (context.SelectedModeVersion != CaptureChallengeMetadata.Version)
        {
            throw new GameModeDomainException(
                ErrorCode.MODE_VERSION_MISMATCH,
                "The selected game-mode version is not supported.");
        }
    }

    private static void EnsureActorMayMutate(LobbyOperationContext context)
    {
        if (!context.ActorMayMutate)
        {
            throw new GameModeDomainException(
                ErrorCode.FORBIDDEN,
                "The current user may not change this lobby.");
        }
    }

    private static void EnsureCaptureDeadline(
        CaptureChallengeRoundState state,
        LobbyOperationContext context)
    {
        if (state.CaptureEndsAt is not DateTimeOffset captureEndsAt
            || context.Now >= captureEndsAt)
        {
            throw new GameModeDomainException(
                ErrorCode.INVALID_MODE_STATUS,
                "The capture phase is closed.");
        }
    }

    private static void EnsureVotingDeadline(
        CaptureChallengeRoundState state,
        LobbyOperationContext context)
    {
        if (state.VotingEndsAt is not DateTimeOffset votingEndsAt
            || context.Now >= votingEndsAt)
        {
            throw new GameModeDomainException(
                ErrorCode.INVALID_MODE_STATUS,
                "The voting phase is closed.");
        }
    }

    private static CaptureChallengeCapture FindOwnedCapture(
        CaptureChallengeRoundState state,
        Guid captureId,
        Guid actorUserId)
    {
        var capture = state.FindCapture(captureId);
        if (capture is null)
        {
            throw new GameModeDomainException(
                ErrorCode.CAPTURE_NOT_FOUND,
                "The requested capture was not found.");
        }

        if (capture.OwnerUserId != actorUserId)
        {
            throw new GameModeDomainException(
                ErrorCode.FORBIDDEN,
                "The requested capture belongs to another player.");
        }

        return capture;
    }

    private static CaptureChallengeAssignment FindOwnedAssignment(
        CaptureChallengeRoundState state,
        Guid assignmentId,
        Guid actorUserId)
    {
        var assignment = state.FindAssignment(assignmentId);
        if (assignment is null || assignment.VoterUserId != actorUserId)
        {
            throw new GameModeDomainException(
                ErrorCode.ASSIGNMENT_NOT_FOUND,
                "The requested voting assignment was not found.");
        }

        return assignment;
    }

    private static CaptureChallengeSettings ValidateSettings(
        CaptureChallengeSettings? settings)
    {
        var errors = new ValidationErrors();
        if (settings is null)
        {
            ThrowValidation(
                "settings",
                "Capture Challenge settings are required.");
        }

        if (settings.CaptureDurationSeconds
            is < CaptureChallengeDefaults.MinimumCaptureDurationSeconds
            or > CaptureChallengeDefaults.MaximumCaptureDurationSeconds)
        {
            errors.Add(
                "settings.captureDurationSeconds",
                $"Capture duration must be between {CaptureChallengeDefaults.MinimumCaptureDurationSeconds} and {CaptureChallengeDefaults.MaximumCaptureDurationSeconds} seconds.");
        }

        if (settings.SecondsPerVote
            is < CaptureChallengeDefaults.MinimumSecondsPerVote
            or > CaptureChallengeDefaults.MaximumSecondsPerVote)
        {
            errors.Add(
                "settings.secondsPerVote",
                $"Seconds per vote must be between {CaptureChallengeDefaults.MinimumSecondsPerVote} and {CaptureChallengeDefaults.MaximumSecondsPerVote}.");
        }

        errors.ThrowIfAny("The Capture Challenge settings are invalid.");
        return CaptureChallengeLobbyState.CopySettings(settings);
    }

    private static ValidatedGoal ValidateGoal(CaptureGoalInput? input)
    {
        var errors = new ValidationErrors();
        if (input is null)
        {
            ThrowValidation("goal", "Goal data is required.");
        }

        var title = input.Title?.Trim() ?? string.Empty;
        if (title.Length == 0)
        {
            errors.Add("goal.title", "The goal title is required.");
        }
        else if (title.Length > 80)
        {
            errors.Add(
                "goal.title",
                "The goal title must contain at most 80 characters.");
        }

        if (input.Description?.Length > 500)
        {
            errors.Add(
                "goal.description",
                "The goal description must contain at most 500 characters.");
        }

        var scoreFactor =
            input.ScoreFactor ?? CaptureChallengeDefaults.DefaultScoreFactor;
        if (scoreFactor is < 0m or > 10m
            || scoreFactor % CaptureChallengeDefaults.ScoreFactorStep != 0m)
        {
            errors.Add(
                "goal.scoreFactor",
                "The score factor must be between 0 and 10 in increments of 0.5.");
        }

        errors.ThrowIfAny("The Capture Challenge goal is invalid.");
        return new ValidatedGoal(
            title,
            input.Description,
            scoreFactor);
    }

    private static StreetViewPosition ValidatePosition(
        StreetViewPosition? position)
    {
        var errors = new ValidationErrors();
        if (position is null)
        {
            ThrowValidation(
                "position",
                "A Street View position is required.");
        }

        if (string.IsNullOrWhiteSpace(position.PanoramaId))
        {
            errors.Add(
                "position.panoramaId",
                "The panorama identifier is required.");
        }
        else if (position.PanoramaId.Length > 512)
        {
            errors.Add(
                "position.panoramaId",
                "The panorama identifier must contain at most 512 characters.");
        }

        ValidateFiniteRange(
            errors,
            "position.latitude",
            position.Latitude,
            -90d,
            90d);
        ValidateFiniteRange(
            errors,
            "position.longitude",
            position.Longitude,
            -180d,
            180d);
        ValidateFiniteRange(
            errors,
            "position.pitch",
            position.Pitch,
            -90d,
            90d);
        ValidateFiniteRange(
            errors,
            "position.zoom",
            position.Zoom,
            0d,
            5d);

        if (!double.IsFinite(position.Heading)
            || position.Heading < 0d
            || position.Heading >= 360d)
        {
            errors.Add(
                "position.heading",
                "Heading must be finite and in the range from 0 inclusive to 360 exclusive.");
        }

        errors.ThrowIfAny("The Street View position is invalid.");
        return CaptureChallengeRoundState.CopyPosition(position);
    }

    private static void ValidateFiniteRange(
        ValidationErrors errors,
        string memberName,
        double value,
        double minimum,
        double maximum)
    {
        if (!double.IsFinite(value) || value < minimum || value > maximum)
        {
            errors.Add(
                memberName,
                $"The value must be finite and between {minimum} and {maximum}.");
        }
    }

    private static void ValidateVoteValue(VoteValue value)
    {
        if (!Enum.IsDefined(value))
        {
            ThrowValidation(
                "value",
                "The vote value must be GOOD or BAD.");
        }
    }

    [DoesNotReturn]
    private static void ThrowValidation(
        string memberName,
        string message) =>
        throw new GameModeDomainException(
            ErrorCode.VALIDATION_FAILED,
            "The game-mode operation is invalid.",
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [memberName] = [message]
            });

    private sealed record ValidatedGoal(
        string Title,
        string? Description,
        decimal ScoreFactor);

    private sealed class ValidationErrors
    {
        private readonly Dictionary<string, List<string>> _errors =
            new(StringComparer.Ordinal);

        public void Add(string memberName, string message)
        {
            if (!_errors.TryGetValue(memberName, out var messages))
            {
                messages = [];
                _errors.Add(memberName, messages);
            }

            messages.Add(message);
        }

        public void ThrowIfAny(string message)
        {
            if (_errors.Count == 0)
            {
                return;
            }

            throw new GameModeDomainException(
                ErrorCode.VALIDATION_FAILED,
                message,
                _errors.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.ToArray(),
                    StringComparer.Ordinal));
        }
    }
}
