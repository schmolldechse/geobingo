using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Api.Mapping;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.GameModes.CaptureChallenge;
using GeoBingo.Contracts.Lobbies;
using GeoBingo.GameModes.Abstractions;
using GeoBingo.GameModes.Registry;
using InternalCapture = GeoBingo.GameModes.CaptureChallenge;

namespace GeoBingo.Api.Lobbies.Mapping;

internal sealed record LobbyConnectionProjection(
    LobbySnapshot Snapshot,
    PersonalProjection PersonalProjection);

internal sealed class LobbyProjectionMapper(
    IGameModeRegistry gameModeRegistry,
    GameModeMapper gameModeMapper,
    TimeProvider timeProvider
)
{
    public LobbyProjectionBatch CreateBatch(LobbyRuntimeState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var members = state.Members.Values
            .OrderBy(member => member.JoinOrder)
            .ToArray();
        var module = gameModeRegistry.GetRequired(
            state.SelectedModeKey,
            state.SelectedModeVersion);
        var now = timeProvider.GetUtcNow();
        var publicPair = module.CreateProjections(
            CreateProjectionContext(state, members.Length, recipientUserId: null, now));
        if (publicPair.CaptureChallengePersonal is not null)
        {
            throw new InvalidOperationException(
                "A public game-mode projection cannot contain personal state.");
        }

        var snapshot = MapSnapshot(
            state,
            members,
            module,
            publicPair);
        var personal = new Dictionary<Guid, PersonalProjection>();
        foreach (var member in members)
        {
            var pair = module.CreateProjections(
                CreateProjectionContext(state, members.Length, member.UserId, now));
            personal.Add(
                member.UserId,
                MapPersonal(state, member.UserId, pair));
        }

        return new LobbyProjectionBatch(
            snapshot,
            new ReadOnlyDictionary<Guid, PersonalProjection>(personal),
            state.IsClosing || state.IsClosed);
    }

    public LobbyConnectionProjection CreateForMember(LobbyRuntimeState state, Guid userId)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (!state.Members.ContainsKey(userId))
        {
            throw new LobbyRuntimeException(
                ErrorCode.NOT_A_MEMBER,
                "The current user is not a lobby member.");
        }

        var batch = CreateBatch(state);
        return new LobbyConnectionProjection(batch.Snapshot, batch.PersonalProjections[userId]);
    }

    private LobbySnapshot MapSnapshot(
        LobbyRuntimeState state,
        IReadOnlyList<LobbyMemberState> members,
        IGameModeModule module,
        GameModeProjectionPair pair) => new()
        {
            LobbyId = state.LobbyId,
            Code = state.Code,
            StateVersion = state.StateVersion,
            Status = state.Status,
            PreparingEndsAt = state.PreparationDeadline,
            HostUserId = state.HostUserId,
            Members = members.Select(MapMember).ToArray(),
            Settings = state.Settings,
            SelectedMode = gameModeMapper.Map(module),
            CurrentRound = state.CurrentRound is null || state.CurrentRoundNumber is null
                ? null
                : new CurrentRoundView
                {
                    RoundId = state.CurrentRound.RoundId,
                    RoundNumber = state.CurrentRoundNumber.Value
                },
            GameMode = new GameModePublicProjection
            {
                ModeKey = state.SelectedModeKey,
                ModeVersion = state.SelectedModeVersion.ToString(3),
                CaptureChallenge = MapPublic(pair.CaptureChallengePublic)
            },
            CompletedRoundSummaries = state.CompletedRoundSummaries
                .ToArray(),
            LastCompletedRoundResults =
                state.CompletedRoundResults.Count == 0
                    ? null
                    : state.CompletedRoundResults[^1],
            CumulativeResults = state.CumulativeResults.ToArray()
        };

    private PersonalProjection MapPersonal(
        LobbyRuntimeState state,
        Guid userId,
        GameModeProjectionPair pair) => new()
        {
            LobbyId = state.LobbyId,
            UserId = userId,
            StateVersion = state.StateVersion,
            CaptureChallenge = MapPersonal(pair.CaptureChallengePersonal)
        };

    private static LobbyMemberView MapMember(
        LobbyMemberState member) => new()
        {
            UserId = member.UserId,
            Handle = member.Handle,
            DisplayName = member.DisplayName,
            JoinOrder = member.JoinOrder,
            Connected = member.IsConnected
        };

    private CaptureChallengePublicProjection? MapPublic(InternalCapture.CaptureChallengePublicProjection? projection)
    {
        if (projection is null)
            return null;

        return new CaptureChallengePublicProjection
        {
            Settings = projection.Settings,
            Goals = projection.Goals
                .Select(MapGoal)
                .ToArray(),
            ActiveParticipantCount =
                projection.ActiveParticipantCount,
            ProjectedCaptureSlotCount =
                projection.ProjectedCaptureSlotCount,
            RoundStartIssues = projection.RoundStartIssues
                .Select(issue => new GameModeValidationIssueView
                {
                    Code = issue.Code,
                    Message = issue.Message,
                    MemberName = issue.MemberName
                })
                .ToArray(),
            Status = projection.Status,
            CaptureEndsAt = projection.CaptureEndsAt,
            VotingEndsAt = projection.VotingEndsAt,
            ParticipantCaptureProgress = projection.ParticipantCaptureProgress
                    .Select(progress =>
                        new CaptureChallengeParticipantCaptureProgress
                        {
                            UserId = progress.UserId,
                            SubmittedCaptureCount =
                                progress.SubmittedCaptureCount,
                            TotalGoalCount =
                                progress.TotalGoalCount
                        })
                    .ToArray(),
            SubmittedCaptureCount = projection.SubmittedCaptureCount,
            ReleasedCaptureCount = projection.ReleasedCaptureCount,
            VotingProgress = projection.VotingProgress is null
                ? null
                : new CaptureChallengeVotingProgress
                {
                    CompletedAssignmentCount = projection.VotingProgress.CompletedAssignmentCount,
                    TotalAssignmentCount = projection.VotingProgress.TotalAssignmentCount
                }
        };
    }

    private CaptureChallengePersonalProjection? MapPersonal(
        InternalCapture.CaptureChallengePersonalProjection?
            projection)
    {
        if (projection is null)
        {
            return null;
        }

        return new CaptureChallengePersonalProjection
        {
            Status = projection.Status,
            CaptureSlots = projection.CaptureSlots
                .Select(slot =>
                    new CaptureChallengeCaptureSlotProjection
                    {
                        Goal = MapGoal(slot.Goal),
                        CaptureId = slot.CaptureId,
                        Position = slot.Position,
                        SubmittedAt = slot.SubmittedAt,
                        UpdatedAt = slot.UpdatedAt
                    })
                .ToArray(),
            CurrentAssignment =
                projection.CurrentAssignment is null
                    ? null
                    : new CaptureChallengeCurrentAssignmentProjection
                    {
                        AssignmentId =
                            projection.CurrentAssignment.AssignmentId,
                        Goal = MapGoal(
                            projection.CurrentAssignment.Goal),
                        Position =
                            projection.CurrentAssignment.Position,
                        Sequence =
                            projection.CurrentAssignment.Sequence
                    },
            TotalAssignmentCount =
                projection.TotalAssignmentCount,
            CompletedAssignmentCount =
                projection.CompletedAssignmentCount,
            VoteHistory = projection.VoteHistory
                .Select(entry =>
                    new CaptureChallengeVoteHistoryEntryProjection
                    {
                        AssignmentId = entry.AssignmentId,
                        Value = entry.Value
                    })
                .ToArray()
        };
    }

    private static CaptureChallengeGoalProjection MapGoal(
        InternalCapture.CaptureChallengeGoalProjection goal) =>
        new()
        {
            GoalId = goal.GoalId,
            Title = goal.Title,
            Description = goal.Description,
            DisplayOrder = goal.DisplayOrder,
            ScoreFactor = goal.ScoreFactor
        };

    private static GameModeProjectionContext
        CreateProjectionContext(
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
