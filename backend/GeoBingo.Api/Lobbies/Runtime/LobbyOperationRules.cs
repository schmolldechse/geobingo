using System;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.Lobbies;

namespace GeoBingo.Api.Lobbies.Runtime;

internal enum LobbyOperationKind
{
    CREATE_MEMBERSHIP,
    RECONNECT_MEMBERSHIP,
    LEAVE,
    CLOSE_LOBBY,
    REMOVE_MEMBER,
    TRANSFER_HOST,
    UPDATE_LOBBY_SETTINGS,
    SELECT_MODE,
    UPDATE_MODE_CONFIGURATION,
    START_ROUND,
    MUTATE_CAPTURE,
    MUTATE_VOTE
}

internal static class LobbyOperationRules
{
    public static void EnsureAllowed(
        LobbyRuntimeState state,
        LobbyActor actor,
        LobbyOperationKind operation)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(actor);

        if (state.IsClosing || state.IsClosed)
        {
            throw new LobbyRuntimeException(
                ErrorCode.LOBBY_CLOSED,
                "The lobby is closed.");
        }

        if (!actor.IsSystem
            && (actor.UserId is not Guid actorUserId
                || actorUserId == Guid.Empty))
        {
            throw new LobbyRuntimeException(
                ErrorCode.AUTH_REQUIRED,
                "An authenticated lobby actor is required.");
        }

        if (!IsAllowedInStatus(state.Status, operation))
        {
            throw new LobbyRuntimeException(
                ErrorCode.INVALID_LOBBY_STATUS,
                "This operation is not available in the current lobby status.");
        }

        if (actor.IsSystem)
        {
            return;
        }

        var userId = actor.UserId!.Value;
        var isMember = state.Members.ContainsKey(userId);

        if (operation == LobbyOperationKind.CREATE_MEMBERSHIP)
        {
            if (isMember)
            {
                throw new LobbyRuntimeException(
                    ErrorCode.LOBBY_NOT_JOINABLE,
                    "The current user is already a lobby member.");
            }

            return;
        }

        if (!isMember)
        {
            throw new LobbyRuntimeException(
                ErrorCode.NOT_A_MEMBER,
                "The current user is not a lobby member.");
        }

        if (RequiresHost(operation)
            && state.HostUserId != userId)
        {
            throw new LobbyRuntimeException(
                ErrorCode.HOST_ONLY,
                "Only the current lobby host may perform this operation.");
        }
    }

    private static bool IsAllowedInStatus(
        LobbyStatus status,
        LobbyOperationKind operation) =>
        operation switch
        {
            LobbyOperationKind.CREATE_MEMBERSHIP =>
                status == LobbyStatus.WAITING,
            LobbyOperationKind.RECONNECT_MEMBERSHIP => true,
            LobbyOperationKind.LEAVE => true,
            LobbyOperationKind.CLOSE_LOBBY => true,
            LobbyOperationKind.REMOVE_MEMBER => true,
            LobbyOperationKind.TRANSFER_HOST => true,
            LobbyOperationKind.UPDATE_LOBBY_SETTINGS =>
                status == LobbyStatus.WAITING,
            LobbyOperationKind.SELECT_MODE =>
                status == LobbyStatus.WAITING,
            LobbyOperationKind.UPDATE_MODE_CONFIGURATION =>
                status == LobbyStatus.WAITING,
            LobbyOperationKind.START_ROUND =>
                status == LobbyStatus.WAITING,
            LobbyOperationKind.MUTATE_CAPTURE =>
                status == LobbyStatus.PLAYING,
            LobbyOperationKind.MUTATE_VOTE =>
                status == LobbyStatus.PLAYING,
            _ => false
        };

    private static bool RequiresHost(LobbyOperationKind operation) =>
        operation is
            LobbyOperationKind.CLOSE_LOBBY
            or LobbyOperationKind.REMOVE_MEMBER
            or LobbyOperationKind.TRANSFER_HOST
            or LobbyOperationKind.UPDATE_LOBBY_SETTINGS
            or LobbyOperationKind.SELECT_MODE
            or LobbyOperationKind.UPDATE_MODE_CONFIGURATION
            or LobbyOperationKind.START_ROUND;
}
