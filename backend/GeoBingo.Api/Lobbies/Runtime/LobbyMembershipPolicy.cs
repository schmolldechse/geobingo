using System;
using System.Collections.Generic;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.Lobbies;
using GeoBingo.GameModes.Abstractions;
using GeoBingo.GameModes.Registry;

namespace GeoBingo.Api.Lobbies.Runtime;

internal enum LobbyMemberRemovalReason
{
    LEAVE,
    KICK,
    BAN,
    DISCONNECT_GRACE_EXPIRED
}

internal sealed record LobbyMembershipRemovalEffects(
    LobbyDeadlineKey DisconnectDeadlineKey,
    LobbyDeadlineKey? PreparationDeadlineKey);

internal sealed class LobbyMembershipPolicy
{
    private readonly IGameModeRegistry gameModeRegistry;

    public LobbyMembershipPolicy(
        IGameModeRegistry gameModeRegistry)
    {
        this.gameModeRegistry = gameModeRegistry
            ?? throw new ArgumentNullException(
                nameof(gameModeRegistry));
    }

    public LobbyMembershipRemovalEffects RemoveMember(
        LobbyRuntimeState state,
        LobbyActor actor,
        Guid targetUserId,
        LobbyMemberRemovalReason reason,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(actor);

        if (targetUserId == Guid.Empty)
        {
            throw new LobbyRuntimeException(
                ErrorCode.VALIDATION_FAILED,
                "The member identifier cannot be empty.");
        }

        if (state.IsClosing || state.IsClosed)
        {
            throw new LobbyRuntimeException(
                ErrorCode.LOBBY_CLOSED,
                "The lobby is closed.");
        }

        if (!state.TryGetMember(
                targetUserId,
                out var targetMember)
            || targetMember is null)
        {
            throw new LobbyRuntimeException(
                ErrorCode.NOT_A_MEMBER,
                "The selected user is not a lobby member.");
        }

        ValidateRemovalActor(
            state,
            actor,
            targetMember,
            reason,
            now);

        LobbyDeadlineKey? preparationDeadlineKey = null;
        ApplyStatusConsequences(
            state,
            targetUserId,
            now,
            ref preparationDeadlineKey);

        if (reason == LobbyMemberRemovalReason.BAN)
        {
            state.BannedUserIds.Add(targetUserId);
        }

        var removedHost = state.HostUserId == targetUserId;
        if (!state.RemoveMember(targetUserId))
        {
            throw new InvalidOperationException(
                "The validated lobby member could not be removed.");
        }

        if (removedHost && state.Members.Count > 0)
        {
            var newHostUserId = state.FindHostSuccessorUserId()
                ?? throw new InvalidOperationException(
                    "A non-empty lobby requires a host successor.");
            state.HostUserId = newHostUserId;
        }

        if (state.Members.Count == 0)
        {
            state.BeginClosing(LobbyEndedReason.NO_MEMBERS);
        }

        return new LobbyMembershipRemovalEffects(
            new LobbyDeadlineKey(
                LobbyDeadlineKind.DISCONNECT_GRACE,
                RoundId: null,
                UserId: targetUserId),
            preparationDeadlineKey);
    }

    public IReadOnlyList<LobbyMembershipRemovalEffects>
        RemoveExpiredDisconnectedMembersAfterRound(
            LobbyRuntimeState state,
            DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.Status != LobbyStatus.WAITING)
        {
            throw new InvalidOperationException(
                "Expired lobby members can only be pruned after the lobby returns to waiting.");
        }

        var effects = new List<LobbyMembershipRemovalEffects>();
        foreach (var userId in
                 state.FindExpiredDisconnectedMemberIds(now))
        {
            effects.Add(
                RemoveMember(
                    state,
                    LobbyActor.System,
                    userId,
                    LobbyMemberRemovalReason
                        .DISCONNECT_GRACE_EXPIRED,
                    now));

            if (state.IsClosing)
            {
                break;
            }
        }

        return effects;
    }

    private static void ValidateRemovalActor(
        LobbyRuntimeState state,
        LobbyActor actor,
        LobbyMemberState targetMember,
        LobbyMemberRemovalReason reason,
        DateTimeOffset now)
    {
        switch (reason)
        {
            case LobbyMemberRemovalReason.LEAVE:
                if (actor.IsSystem
                    || actor.UserId != targetMember.UserId)
                {
                    throw new LobbyRuntimeException(
                        ErrorCode.FORBIDDEN,
                        "A member may only leave for themselves.");
                }

                return;
            case LobbyMemberRemovalReason.KICK:
            case LobbyMemberRemovalReason.BAN:
                if (actor.IsSystem
                    || actor.UserId != state.HostUserId)
                {
                    throw new LobbyRuntimeException(
                        ErrorCode.HOST_ONLY,
                        "Only the current lobby host may remove another member.");
                }

                if (targetMember.UserId == state.HostUserId)
                {
                    throw new LobbyRuntimeException(
                        ErrorCode.VALIDATION_FAILED,
                        "The lobby host cannot kick or ban themselves.");
                }

                return;
            case LobbyMemberRemovalReason
                .DISCONNECT_GRACE_EXPIRED:
                if (!actor.IsSystem)
                {
                    throw new LobbyRuntimeException(
                        ErrorCode.FORBIDDEN,
                        "Only the server may expire a disconnected member.");
                }

                if (!targetMember.HasExpiredDisconnectGrace(now))
                {
                    throw new LobbyRuntimeException(
                        ErrorCode.FORBIDDEN,
                        "The member disconnect grace period has not expired.");
                }

                return;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(reason),
                    reason,
                    "The lobby member removal reason is unsupported.");
        }
    }

    private void ApplyStatusConsequences(
        LobbyRuntimeState state,
        Guid targetUserId,
        DateTimeOffset now,
        ref LobbyDeadlineKey? preparationDeadlineKey)
    {
        switch (state.Status)
        {
            case LobbyStatus.WAITING:
                return;
            case LobbyStatus.PREPARING:
                var preparingRoundId =
                    state.DiscardPreparingRound();
                preparationDeadlineKey = new LobbyDeadlineKey(
                    LobbyDeadlineKind.PREPARATION,
                    preparingRoundId,
                    UserId: null);
                return;
            case LobbyStatus.PLAYING:
                var playingRound = state.CurrentRound
                    ?? throw new InvalidOperationException(
                        "A playing lobby requires an active round.");
                var module = gameModeRegistry.GetRequired(
                    state.SelectedModeKey,
                    state.SelectedModeVersion);
                module.HandleParticipantWithdrawal(
                    new GameModeParticipantWithdrawalContext(
                        state.ModeLobbyState,
                        playingRound,
                        targetUserId,
                        now));
                return;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state.Status,
                    "The lobby status is unsupported.");
        }
    }
}
