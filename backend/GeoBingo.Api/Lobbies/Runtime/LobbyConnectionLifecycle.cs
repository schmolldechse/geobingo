using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.Lobbies;

namespace GeoBingo.Api.Lobbies.Runtime;

internal sealed record LobbyJoinIdentity
{
    public LobbyJoinIdentity(
        Guid userId,
        string handle,
        string displayName,
        string? avatarUrl)
    {
        var errors = new Dictionary<string, string[]>(
            StringComparer.Ordinal);

        if (userId == Guid.Empty)
        {
            errors["userId"] =
                ["The user identifier cannot be empty."];
        }

        ValidateName(handle, "handle", errors);
        ValidateName(displayName, "displayName", errors);

        if (errors.Count > 0)
        {
            throw new LobbyRuntimeException(
                ErrorCode.VALIDATION_FAILED,
                "The lobby join identity is invalid.",
                errors);
        }

        UserId = userId;
        Handle = handle;
        DisplayName = displayName;
        AvatarUrl = NormalizeAvatarUrl(avatarUrl);
    }

    public Guid UserId { get; }

    public string Handle { get; }

    public string DisplayName { get; }

    public string? AvatarUrl { get; }

    private static void ValidateName(
        string value,
        string memberName,
        IDictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > 64
            || !string.Equals(
                value,
                value.Trim(),
                StringComparison.Ordinal))
        {
            errors[memberName] =
                ["The value must be trimmed and contain between 1 and 64 characters."];
        }
    }

    private static string? NormalizeAvatarUrl(string? avatarUrl) =>
        string.IsNullOrWhiteSpace(avatarUrl)
            ? null
            : avatarUrl.Trim();
}

internal sealed class LobbyConnectionLifecycle
{
    private static readonly TimeSpan DisconnectGracePeriod =
        TimeSpan.FromSeconds(90);

    private readonly ILobbyRegistry lobbyRegistry;
    private readonly LobbyConnectionRegistry connectionRegistry;
    private readonly LobbyMembershipPolicy membershipPolicy;
    private readonly TimeProvider timeProvider;

    public LobbyConnectionLifecycle(
        ILobbyRegistry lobbyRegistry,
        LobbyConnectionRegistry connectionRegistry,
        LobbyMembershipPolicy membershipPolicy,
        TimeProvider timeProvider)
    {
        this.lobbyRegistry = lobbyRegistry
            ?? throw new ArgumentNullException(
                nameof(lobbyRegistry));
        this.connectionRegistry = connectionRegistry
            ?? throw new ArgumentNullException(
                nameof(connectionRegistry));
        this.membershipPolicy = membershipPolicy
            ?? throw new ArgumentNullException(
                nameof(membershipPolicy));
        this.timeProvider = timeProvider
            ?? throw new ArgumentNullException(
                nameof(timeProvider));
    }

    public ValueTask<LobbyOperationCompletion>
        JoinOrReconnectAsync(
            LobbyRuntime runtime,
            LobbyJoinIdentity identity,
            string connectionId,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        var actor = new LobbyActor(
            identity.UserId,
            connectionId,
            IsSystem: false);

        return runtime.EnqueueMutationAsync(
            "join-or-reconnect",
            actor,
            (state, operationActor, _) =>
                ValueTask.FromResult(
                    ApplyJoinOrReconnect(
                        runtime,
                        state,
                        operationActor,
                        identity,
                        connectionId)),
            cancellationToken);
    }

    public async ValueTask HandleDisconnectedAsync(
        string connectionId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        var context = connectionRegistry.RemoveConnection(
            connectionId);
        if (context?.LobbyId is not Guid lobbyId
            || !lobbyRegistry.TryGet(lobbyId, out var runtime)
            || runtime is null)
        {
            return;
        }

        _ = await runtime.EnqueueMutationAsync(
                "connection-disconnected",
                LobbyActor.System,
                (state, _, _) =>
                    ValueTask.FromResult(
                        ApplyDisconnect(
                            runtime,
                            state,
                            context.UserId,
                            connectionId)),
                CancellationToken.None)
            .ConfigureAwait(false);
    }

    private LobbyMutationOutcome ApplyJoinOrReconnect(
        LobbyRuntime runtime,
        LobbyRuntimeState state,
        LobbyActor actor,
        LobbyJoinIdentity identity,
        string connectionId)
    {
        try
        {
            var reconnecting = state.TryGetMember(
                identity.UserId,
                out var member);

            if (!reconnecting
                && state.Status != LobbyStatus.WAITING)
            {
                return Reject(
                    ErrorCode.LOBBY_NOT_JOINABLE,
                    "The lobby does not accept new members in its current status.");
            }

            LobbyOperationRules.EnsureAllowed(
                state,
                actor,
                reconnecting
                    ? LobbyOperationKind.RECONNECT_MEMBERSHIP
                    : LobbyOperationKind.CREATE_MEMBERSHIP);

            if (!reconnecting)
            {
                if (state.BannedUserIds.Contains(
                        identity.UserId))
                {
                    return Reject(
                        ErrorCode.LOBBY_BANNED,
                        "The current user is banned from this lobby.");
                }

                if (state.Members.Count
                    >= state.Settings.MaxPlayers)
                {
                    return Reject(
                        ErrorCode.LOBBY_FULL,
                        "The lobby has reached its member capacity.");
                }

                if (!state.CanCreateMembership(
                        identity.UserId))
                {
                    return Reject(
                        ErrorCode.LOBBY_NOT_JOINABLE,
                        "The lobby does not accept a new membership.");
                }
            }
            else if (!state.CanReconnect(identity.UserId))
            {
                return Reject(
                    ErrorCode.LOBBY_NOT_JOINABLE,
                    "The existing membership cannot reconnect.");
            }

            var association = connectionRegistry.TryAssociate(
                connectionId,
                identity.UserId,
                state.LobbyId);
            var associationFailure = MapAssociationFailure(
                association);
            if (associationFailure is not null)
            {
                return LobbyMutationOutcome.Rejected(
                    associationFailure);
            }

            if (association
                == LobbyConnectionAssociation
                    .ALREADY_ASSOCIATED)
            {
                if (member is null
                    || !member.ConnectionIds.Contains(
                        connectionId))
                {
                    throw new InvalidOperationException(
                        "The connection registry and lobby member state are inconsistent.");
                }
            }
            else
            {
                member ??= state.AddMember(
                    identity.UserId,
                    identity.Handle,
                    identity.DisplayName,
                    identity.AvatarUrl);
                member.AttachConnection(connectionId);
            }

            runtime.CancelDeadline(
                new LobbyDeadlineKey(
                    LobbyDeadlineKind.DISCONNECT_GRACE,
                    RoundId: null,
                    UserId: identity.UserId));

            return LobbyMutationOutcome.Applied;
        }
        catch (LobbyRuntimeException exception)
        {
            return LobbyMutationOutcome.Rejected(
                new LobbyOperationFailure(
                    exception.Code,
                    exception.Message,
                    exception.Errors));
        }
    }

    private LobbyMutationOutcome ApplyDisconnect(
        LobbyRuntime runtime,
        LobbyRuntimeState state,
        Guid userId,
        string connectionId)
    {
        if (state.IsClosing
            || state.IsClosed
            || !state.TryGetMember(userId, out var member)
            || member is null)
        {
            return LobbyMutationOutcome.Ignored;
        }

        var now = timeProvider.GetUtcNow();
        if (!member.DetachConnection(
                connectionId,
                now,
                DisconnectGracePeriod))
        {
            return LobbyMutationOutcome.Ignored;
        }

        if (member.IsConnected)
        {
            return LobbyMutationOutcome.Applied;
        }

        var deadline = member.DisconnectGraceEndsAt
            ?? throw new InvalidOperationException(
                "A disconnected lobby member requires a grace deadline.");
        ScheduleDisconnectDeadline(
            runtime,
            userId,
            deadline);
        return LobbyMutationOutcome.Applied;
    }

    private void ScheduleDisconnectDeadline(
        LobbyRuntime runtime,
        Guid userId,
        DateTimeOffset deadline)
    {
        var deadlineKey = new LobbyDeadlineKey(
            LobbyDeadlineKind.DISCONNECT_GRACE,
            RoundId: null,
            UserId: userId);

        runtime.ScheduleDeadline(
            deadlineKey,
            deadline,
            (state, _, _) =>
            {
                var now = timeProvider.GetUtcNow();
                if (state.IsClosing
                    || state.IsClosed
                    || !state.TryGetMember(
                        userId,
                        out var member)
                    || member is null
                    || !member.MatchesDisconnectDeadline(
                        deadline)
                    || now < deadline)
                {
                    return ValueTask.FromResult(
                        LobbyMutationOutcome.Ignored);
                }

                var effects = membershipPolicy.RemoveMember(
                    state,
                    LobbyActor.System,
                    userId,
                    LobbyMemberRemovalReason
                        .DISCONNECT_GRACE_EXPIRED,
                    now);

                if (effects.PreparationDeadlineKey
                    is LobbyDeadlineKey preparationDeadlineKey)
                {
                    runtime.CancelDeadline(
                        preparationDeadlineKey);
                }

                return ValueTask.FromResult(
                    LobbyMutationOutcome.Applied);
            });
    }

    private static LobbyOperationFailure? MapAssociationFailure(
        LobbyConnectionAssociation association) =>
        association switch
        {
            LobbyConnectionAssociation.ASSOCIATED => null,
            LobbyConnectionAssociation.ALREADY_ASSOCIATED =>
                null,
            LobbyConnectionAssociation.UNKNOWN_CONNECTION =>
                new LobbyOperationFailure(
                    ErrorCode.AUTH_REQUIRED,
                    "The authenticated connection is not registered.",
                    Errors: null),
            LobbyConnectionAssociation.USER_MISMATCH =>
                new LobbyOperationFailure(
                    ErrorCode.AUTH_REQUIRED,
                    "The connection is registered for another user.",
                    Errors: null),
            LobbyConnectionAssociation
                .ALREADY_IN_ANOTHER_LOBBY =>
                new LobbyOperationFailure(
                    ErrorCode.ALREADY_IN_ANOTHER_LOBBY,
                    "The connection is already associated with another lobby.",
                    Errors: null),
            _ => throw new ArgumentOutOfRangeException(
                nameof(association),
                association,
                "The lobby connection association is unsupported.")
        };

    private static LobbyMutationOutcome Reject(
        ErrorCode code,
        string message) =>
        LobbyMutationOutcome.Rejected(
            new LobbyOperationFailure(
                code,
                message,
                Errors: null));
}
