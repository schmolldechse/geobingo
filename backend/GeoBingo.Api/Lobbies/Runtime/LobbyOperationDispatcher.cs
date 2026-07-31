using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.GameModes.CaptureChallenge;
using GeoBingo.Contracts.Lobbies;
using GeoBingo.GameModes.Abstractions;
using GeoBingo.GameModes.CaptureChallenge;
using GeoBingo.GameModes.Registry;

namespace GeoBingo.Api.Lobbies.Runtime;

internal sealed class LobbyOperationDispatcher
{
    private readonly ILobbyRegistry lobbyRegistry;
    private readonly LobbyConnectionRegistry connectionRegistry;
    private readonly LobbyMembershipPolicy membershipPolicy;
    private readonly IGameModeRegistry gameModeRegistry;
    private readonly CaptureChallengeOperations
        captureChallengeOperations;
    private readonly TimeProvider timeProvider;

    public LobbyOperationDispatcher(
        ILobbyRegistry lobbyRegistry,
        LobbyConnectionRegistry connectionRegistry,
        LobbyMembershipPolicy membershipPolicy,
        IGameModeRegistry gameModeRegistry,
        CaptureChallengeOperations captureChallengeOperations,
        TimeProvider timeProvider)
    {
        this.lobbyRegistry = lobbyRegistry
            ?? throw new ArgumentNullException(nameof(lobbyRegistry));
        this.connectionRegistry = connectionRegistry
            ?? throw new ArgumentNullException(
                nameof(connectionRegistry));
        this.membershipPolicy = membershipPolicy
            ?? throw new ArgumentNullException(
                nameof(membershipPolicy));
        this.gameModeRegistry = gameModeRegistry
            ?? throw new ArgumentNullException(
                nameof(gameModeRegistry));
        this.captureChallengeOperations =
            captureChallengeOperations
            ?? throw new ArgumentNullException(
                nameof(captureChallengeOperations));
        this.timeProvider = timeProvider
            ?? throw new ArgumentNullException(
                nameof(timeProvider));
    }

    public bool TryResolveForConnection(
        string connectionId,
        Guid authenticatedUserId,
        out LobbyRuntime? runtime,
        out LobbyOperationFailure? failure)
    {
        runtime = null;

        if (string.IsNullOrWhiteSpace(connectionId)
            || authenticatedUserId == Guid.Empty)
        {
            failure = new LobbyOperationFailure(
                ErrorCode.AUTH_REQUIRED,
                "An authenticated connection is required.",
                Errors: null);
            return false;
        }

        if (!connectionRegistry.TryGet(
                connectionId,
                out var connectionContext)
            || connectionContext is null
            || connectionContext.UserId != authenticatedUserId)
        {
            failure = new LobbyOperationFailure(
                ErrorCode.AUTH_REQUIRED,
                "The authenticated connection is not registered.",
                Errors: null);
            return false;
        }

        if (connectionContext.LobbyId is not Guid lobbyId)
        {
            failure = new LobbyOperationFailure(
                ErrorCode.NOT_A_MEMBER,
                "The connection is not associated with a lobby.",
                Errors: null);
            return false;
        }

        if (!lobbyRegistry.TryGet(lobbyId, out runtime)
            || runtime is null)
        {
            failure = new LobbyOperationFailure(
                ErrorCode.LOBBY_NOT_FOUND,
                "The lobby was not found.",
                Errors: null);
            return false;
        }

        var summary = runtime.ReadSummary();
        if (summary.IsClosing || summary.IsClosed)
        {
            failure = new LobbyOperationFailure(
                ErrorCode.LOBBY_CLOSED,
                "The lobby is closed.",
                Errors: null);
            return false;
        }

        failure = null;
        return true;
    }

    public ValueTask<LobbyOperationCompletion> LeaveAsync(
        string connectionId,
        Guid userId,
        CancellationToken cancellationToken) =>
        DispatchForConnectionAsync(
            "leave-lobby",
            connectionId,
            userId,
            (runtime, state, actor, _) =>
            {
                LobbyOperationRules.EnsureAllowed(
                    state,
                    actor,
                    LobbyOperationKind.LEAVE);
                var effects = membershipPolicy.RemoveMember(
                    state,
                    actor,
                    userId,
                    LobbyMemberRemovalReason.LEAVE,
                    timeProvider.GetUtcNow());
                CancelRemovalDeadlines(runtime, effects);
                return ValueTask.FromResult(
                    LobbyMutationOutcome.Applied);
            },
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> CloseAsync(
        string connectionId,
        Guid userId,
        CancellationToken cancellationToken) =>
        DispatchForConnectionAsync(
            "close-lobby",
            connectionId,
            userId,
            (runtime, state, actor, _) =>
            {
                LobbyOperationRules.EnsureAllowed(
                    state,
                    actor,
                    LobbyOperationKind.CLOSE_LOBBY);
                runtime.CancelAllDeadlines();
                state.BeginClosing(
                    LobbyEndedReason.CLOSED_BY_HOST);
                return ValueTask.FromResult(
                    LobbyMutationOutcome.Applied);
            },
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> RemovePlayerAsync(
        string connectionId,
        Guid userId,
        RemovePlayerRequest request,
        CancellationToken cancellationToken) =>
        DispatchForConnectionAsync(
            request.Kind == PlayerRemovalKind.BAN
                ? "ban-member"
                : "kick-member",
            connectionId,
            userId,
            (runtime, state, actor, _) =>
            {
                LobbyOperationRules.EnsureAllowed(
                    state,
                    actor,
                    LobbyOperationKind.REMOVE_MEMBER);
                var effects = membershipPolicy.RemoveMember(
                    state,
                    actor,
                    request.UserId,
                    request.Kind == PlayerRemovalKind.BAN
                        ? LobbyMemberRemovalReason.BAN
                        : LobbyMemberRemovalReason.KICK,
                    timeProvider.GetUtcNow());
                CancelRemovalDeadlines(runtime, effects);
                return ValueTask.FromResult(
                    LobbyMutationOutcome.Applied);
            },
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> TransferHostAsync(
        string connectionId,
        Guid userId,
        TransferHostRequest request,
        CancellationToken cancellationToken) =>
        DispatchForConnectionAsync(
            "transfer-host",
            connectionId,
            userId,
            (_, state, actor, _) =>
            {
                LobbyOperationRules.EnsureAllowed(
                    state,
                    actor,
                    LobbyOperationKind.TRANSFER_HOST);
                if (request.UserId == state.HostUserId
                    || !state.Members.ContainsKey(request.UserId))
                {
                    throw new LobbyRuntimeException(
                        ErrorCode.VALIDATION_FAILED,
                        "The new host must be another active lobby member.");
                }

                state.HostUserId = request.UserId;
                return ValueTask.FromResult(
                    LobbyMutationOutcome.Applied);
            },
            cancellationToken);

    public ValueTask<LobbyOperationCompletion>
        UpdateLobbySettingsAsync(
            string connectionId,
            Guid userId,
            UpdateLobbySettingsRequest request,
            CancellationToken cancellationToken) =>
        DispatchForConnectionAsync(
            "update-lobby-settings",
            connectionId,
            userId,
            (_, state, actor, _) =>
            {
                LobbyOperationRules.EnsureAllowed(
                    state,
                    actor,
                    LobbyOperationKind.UPDATE_LOBBY_SETTINGS);
                state.ReplaceSettings(request.Settings);
                return ValueTask.FromResult(
                    LobbyMutationOutcome.Applied);
            },
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> SelectGameModeAsync(
        string connectionId,
        Guid userId,
        SelectGameModeRequest request,
        CancellationToken cancellationToken) =>
        DispatchForConnectionAsync(
            "select-game-mode",
            connectionId,
            userId,
            (_, state, actor, _) =>
            {
                LobbyOperationRules.EnsureAllowed(
                    state,
                    actor,
                    LobbyOperationKind.SELECT_MODE);
                if (!gameModeRegistry.TryGet(
                        request.ModeKey,
                        out var module)
                    || module is null)
                {
                    throw new LobbyRuntimeException(
                        ErrorCode.MODE_NOT_FOUND,
                        "The requested game mode was not found.");
                }

                if (string.Equals(
                        state.SelectedModeKey,
                        module.Key,
                        StringComparison.Ordinal))
                {
                    throw new LobbyRuntimeException(
                        ErrorCode.VALIDATION_FAILED,
                        "The requested game mode is already selected.");
                }

                if (state.HasNonDefaultModeConfiguration
                    && !request.ConfirmModeStateReset)
                {
                    throw new LobbyRuntimeException(
                        ErrorCode.VALIDATION_FAILED,
                        "Changing the game mode requires confirmation because the current configuration will be reset.");
                }

                state.SelectedModeKey = module.Key;
                state.SelectedModeVersion = module.Version;
                state.ModeLobbyState =
                    module.CreateDefaultLobbyState();
                state.HasNonDefaultModeConfiguration = false;
                return ValueTask.FromResult(
                    LobbyMutationOutcome.Applied);
            },
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> StartRoundAsync(
        string connectionId,
        Guid userId,
        CancellationToken cancellationToken) =>
        DispatchForConnectionAsync(
            "start-round",
            connectionId,
            userId,
            (runtime, state, actor, _) =>
            {
                LobbyOperationRules.EnsureAllowed(
                    state,
                    actor,
                    LobbyOperationKind.START_ROUND);

                if (state.CurrentRound is not null
                    || state.CurrentRoundNumber is not null)
                {
                    throw new InvalidOperationException(
                        "A waiting lobby cannot start a second active round.");
                }

                var module = gameModeRegistry.GetRequired(
                    state.SelectedModeKey,
                    state.SelectedModeVersion);
                var participants = state.Members.Values
                    .OrderBy(member => member.JoinOrder)
                    .Select(member =>
                        new GameModeParticipant(
                            member.UserId,
                            member.Handle,
                            member.DisplayName,
                            member.JoinOrder))
                    .ToArray();
                var acceptedAt = timeProvider.GetUtcNow();
                var roundId = Guid.NewGuid();
                var roundNumber = state.GetNextRoundNumber();
                var roundCreation = module.CreateRound(
                    state.ModeLobbyState,
                    new GameModeRoundCreationContext(
                        roundId,
                        roundNumber,
                        participants,
                        acceptedAt));
                var round = roundCreation.RoundState
                    ?? throw new InvalidOperationException(
                        "The selected game mode returned no round state.");

                if (round.RoundId != roundId
                    || round.RoundNumber != roundNumber
                    || !string.Equals(
                        round.ModeKey,
                        module.Key,
                        StringComparison.Ordinal)
                    || round.ModeVersion != module.Version)
                {
                    throw new InvalidOperationException(
                        "The created round state does not match the accepted round metadata.");
                }

                var preparationDeadline =
                    acceptedAt.Add(
                        LobbyRuntimeState.PreparationDuration);
                state.BeginPreparation(
                    round,
                    roundNumber,
                    preparationDeadline);
                SchedulePreparationDeadline(
                    runtime,
                    state.LobbyId,
                    roundId,
                    preparationDeadline);

                return ValueTask.FromResult(
                    LobbyMutationOutcome.Applied);
            },
            cancellationToken);

    public ValueTask<LobbyOperationCompletion>
        UpdateCaptureChallengeSettingsAsync(
            string connectionId,
            Guid userId,
            UpdateCaptureChallengeSettingsRequest request,
            CancellationToken cancellationToken) =>
        DispatchWaitingCaptureChallengeAsync(
            "update-capture-challenge-settings",
            connectionId,
            userId,
            (state, context) =>
                captureChallengeOperations.UpdateSettings(
                    state,
                    request,
                    context),
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> AddCaptureGoalAsync(
        string connectionId,
        Guid userId,
        AddCaptureGoalRequest request,
        CancellationToken cancellationToken) =>
        DispatchWaitingCaptureChallengeAsync(
            "add-capture-goal",
            connectionId,
            userId,
            (state, context) =>
                captureChallengeOperations.AddGoal(
                    state,
                    request,
                    context),
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> UpdateCaptureGoalAsync(
        string connectionId,
        Guid userId,
        UpdateCaptureGoalRequest request,
        CancellationToken cancellationToken) =>
        DispatchWaitingCaptureChallengeAsync(
            "update-capture-goal",
            connectionId,
            userId,
            (state, context) =>
                captureChallengeOperations.UpdateGoal(
                    state,
                    request,
                    context),
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> RemoveCaptureGoalAsync(
        string connectionId,
        Guid userId,
        RemoveCaptureGoalRequest request,
        CancellationToken cancellationToken) =>
        DispatchWaitingCaptureChallengeAsync(
            "remove-capture-goal",
            connectionId,
            userId,
            (state, context) =>
                captureChallengeOperations.RemoveGoal(
                    state,
                    request,
                    context),
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> ReorderCaptureGoalsAsync(
        string connectionId,
        Guid userId,
        ReorderCaptureGoalsRequest request,
        CancellationToken cancellationToken) =>
        DispatchWaitingCaptureChallengeAsync(
            "reorder-capture-goals",
            connectionId,
            userId,
            (state, context) =>
                captureChallengeOperations.ReorderGoals(
                    state,
                    request,
                    context),
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> SubmitCaptureAsync(
        string connectionId,
        Guid userId,
        SubmitCaptureRequest request,
        CancellationToken cancellationToken) =>
        DispatchRoundCaptureChallengeAsync(
            "submit-capture",
            LobbyOperationKind.MUTATE_CAPTURE,
            connectionId,
            userId,
            (state, context) =>
                captureChallengeOperations.SubmitCapture(
                    state,
                    request,
                    context),
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> UpdateCaptureAsync(
        string connectionId,
        Guid userId,
        UpdateCaptureRequest request,
        CancellationToken cancellationToken) =>
        DispatchRoundCaptureChallengeAsync(
            "update-capture",
            LobbyOperationKind.MUTATE_CAPTURE,
            connectionId,
            userId,
            (state, context) =>
                captureChallengeOperations.UpdateCapture(
                    state,
                    request,
                    context),
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> RemoveCaptureAsync(
        string connectionId,
        Guid userId,
        RemoveCaptureRequest request,
        CancellationToken cancellationToken) =>
        DispatchRoundCaptureChallengeAsync(
            "remove-capture",
            LobbyOperationKind.MUTATE_CAPTURE,
            connectionId,
            userId,
            (state, context) =>
                captureChallengeOperations.RemoveCapture(
                    state,
                    request,
                    context),
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> CastVoteAsync(
        string connectionId,
        Guid userId,
        CastVoteRequest request,
        CancellationToken cancellationToken) =>
        DispatchRoundCaptureChallengeAsync(
            "cast-vote",
            LobbyOperationKind.MUTATE_VOTE,
            connectionId,
            userId,
            (state, context) =>
            {
                captureChallengeOperations.CastVote(
                    state,
                    request,
                    context);
            },
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> ChangeVoteAsync(
        string connectionId,
        Guid userId,
        ChangeVoteRequest request,
        CancellationToken cancellationToken) =>
        DispatchRoundCaptureChallengeAsync(
            "change-vote",
            LobbyOperationKind.MUTATE_VOTE,
            connectionId,
            userId,
            (state, context) =>
                captureChallengeOperations.ChangeVote(
                    state,
                    request,
                    context),
            cancellationToken);

    public ValueTask<LobbyOperationCompletion> DispatchSystemAsync(
        string operationName,
        Guid lobbyId,
        Func<LobbyRuntimeState, LobbyActor, CancellationToken,
            ValueTask<LobbyMutationOutcome>> mutation,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(mutation);

        if (lobbyId == Guid.Empty
            || !lobbyRegistry.TryGet(lobbyId, out var runtime)
            || runtime is null)
        {
            return ValueTask.FromResult(
                CreateRejectedCompletion(
                    runtime: null,
                    new LobbyOperationFailure(
                        ErrorCode.LOBBY_NOT_FOUND,
                        "The lobby was not found.",
                        Errors: null)));
        }

        return runtime.EnqueueMutationAsync(
            operationName,
            LobbyActor.System,
            WrapExpectedFailures(mutation),
            cancellationToken);
    }

    private ValueTask<LobbyOperationCompletion>
        DispatchWaitingCaptureChallengeAsync(
            string operationName,
            string connectionId,
            Guid userId,
            Action<
                CaptureChallengeLobbyState,
                LobbyOperationContext> operation,
            CancellationToken cancellationToken) =>
        DispatchForConnectionAsync(
            operationName,
            connectionId,
            userId,
            (_, state, actor, _) =>
            {
                LobbyOperationRules.EnsureAllowed(
                    state,
                    actor,
                    LobbyOperationKind.UPDATE_MODE_CONFIGURATION);
                var captureState =
                    RequireCaptureChallengeLobbyState(state);
                operation(
                    captureState,
                    CreateOperationContext(state, actor));
                state.HasNonDefaultModeConfiguration = true;
                return ValueTask.FromResult(
                    LobbyMutationOutcome.Applied);
            },
            cancellationToken);

    private ValueTask<LobbyOperationCompletion>
        DispatchRoundCaptureChallengeAsync(
            string operationName,
            LobbyOperationKind operationKind,
            string connectionId,
            Guid userId,
            Action<
                CaptureChallengeRoundState,
                LobbyOperationContext> operation,
            CancellationToken cancellationToken) =>
        DispatchForConnectionAsync(
            operationName,
            connectionId,
            userId,
            (_, state, actor, _) =>
            {
                LobbyOperationRules.EnsureAllowed(
                    state,
                    actor,
                    operationKind);
                var roundState =
                    state.CurrentRound
                        as CaptureChallengeRoundState
                    ?? throw new LobbyRuntimeException(
                        ErrorCode.INVALID_MODE_STATUS,
                        "No active Capture Challenge round is available.");
                operation(
                    roundState,
                    CreateOperationContext(state, actor));
                return ValueTask.FromResult(
                    LobbyMutationOutcome.Applied);
            },
            cancellationToken);

    private ValueTask<LobbyOperationCompletion>
        DispatchForConnectionAsync(
            string operationName,
            string connectionId,
            Guid authenticatedUserId,
            Func<
                LobbyRuntime,
                LobbyRuntimeState,
                LobbyActor,
                CancellationToken,
                ValueTask<LobbyMutationOutcome>> mutation,
            CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(mutation);

        if (!TryResolveForConnection(
                connectionId,
                authenticatedUserId,
                out var runtime,
                out var failure))
        {
            return ValueTask.FromResult(
                CreateRejectedCompletion(runtime, failure!));
        }

        return runtime!.EnqueueMutationAsync(
            operationName,
            new LobbyActor(
                authenticatedUserId,
                connectionId,
                IsSystem: false),
            WrapExpectedFailures(
                (state, actor, token) =>
                    mutation(
                        runtime,
                        state,
                        actor,
                        token)),
            cancellationToken);
    }

    private static CaptureChallengeLobbyState
        RequireCaptureChallengeLobbyState(
            LobbyRuntimeState state) =>
        state.ModeLobbyState as CaptureChallengeLobbyState
        ?? throw new LobbyRuntimeException(
            ErrorCode.MODE_OPERATION_NOT_SUPPORTED,
            "This operation is not supported by the selected game mode.");

    private void SchedulePreparationDeadline(
        LobbyRuntime runtime,
        Guid lobbyId,
        Guid roundId,
        DateTimeOffset deadline)
    {
        var deadlineKey = new LobbyDeadlineKey(
            LobbyDeadlineKind.PREPARATION,
            roundId,
            UserId: null);

        runtime.ScheduleDeadline(
            deadlineKey,
            deadline,
            (state, key, _) =>
            {
                if (!MatchesPreparationDeadline(
                        state,
                        key,
                        lobbyId,
                        roundId,
                        deadline))
                {
                    return ValueTask.FromResult(
                        LobbyMutationOutcome.Ignored);
                }

                var now = timeProvider.GetUtcNow();
                if (now < deadline)
                {
                    SchedulePreparationDeadline(
                        runtime,
                        lobbyId,
                        roundId,
                        deadline);
                    return ValueTask.FromResult(
                        LobbyMutationOutcome.Ignored);
                }

                var round = state.CurrentRound!;
                var module = gameModeRegistry.GetRequired(
                    round.ModeKey,
                    round.ModeVersion);
                var advance = module.Advance(
                    new GameModeAdvanceContext(
                        state.ModeLobbyState,
                        round,
                        state.Members.Keys.ToHashSet(),
                        now));
                if (advance.Kind != GameModeAdvanceKind.STATE_CHANGED
                    || advance.NextDeadline
                        is not DateTimeOffset modeDeadline)
                {
                    throw new InvalidOperationException(
                        "A prepared round must enter its first playing phase with a deadline.");
                }

                state.TransitionTo(LobbyStatus.PLAYING);
                state.PreparationDeadline = null;
                state.ModeDeadline = modeDeadline;
                state.ModeDeadlineRoundId = roundId;
                ScheduleModeDeadline(
                    runtime,
                    lobbyId,
                    roundId,
                    modeDeadline);
                return ValueTask.FromResult(
                    LobbyMutationOutcome.Applied);
            });
    }

    private void ScheduleModeDeadline(
        LobbyRuntime runtime,
        Guid lobbyId,
        Guid roundId,
        DateTimeOffset deadline)
    {
        var deadlineKey = new LobbyDeadlineKey(
            LobbyDeadlineKind.MODE,
            roundId,
            UserId: null);

        runtime.ScheduleDeadline(
            deadlineKey,
            deadline,
            (state, key, _) =>
            {
                if (!MatchesModeDeadline(
                        state,
                        key,
                        lobbyId,
                        roundId,
                        deadline))
                {
                    return ValueTask.FromResult(
                        LobbyMutationOutcome.Ignored);
                }

                var now = timeProvider.GetUtcNow();
                if (now < deadline)
                {
                    ScheduleModeDeadline(
                        runtime,
                        lobbyId,
                        roundId,
                        deadline);
                    return ValueTask.FromResult(
                        LobbyMutationOutcome.Ignored);
                }

                var round = state.CurrentRound!;
                var module = gameModeRegistry.GetRequired(
                    round.ModeKey,
                    round.ModeVersion);
                var advance = module.Advance(
                    new GameModeAdvanceContext(
                        state.ModeLobbyState,
                        round,
                        state.Members.Keys.ToHashSet(),
                        now));

                return ValueTask.FromResult(
                    ApplyModeAdvance(
                        runtime,
                        state,
                        lobbyId,
                        roundId,
                        deadline,
                        now,
                        advance));
            });
    }

    private LobbyMutationOutcome ApplyModeAdvance(
        LobbyRuntime runtime,
        LobbyRuntimeState state,
        Guid lobbyId,
        Guid roundId,
        DateTimeOffset elapsedDeadline,
        DateTimeOffset now,
        GameModeAdvanceOutcome advance)
    {
        switch (advance.Kind)
        {
            case GameModeAdvanceKind.NO_CHANGE:
                if (advance.NextDeadline
                    is not DateTimeOffset retryDeadline
                    || retryDeadline <= now)
                {
                    return LobbyMutationOutcome.Ignored;
                }

                ScheduleModeDeadline(
                    runtime,
                    lobbyId,
                    roundId,
                    retryDeadline);
                if (retryDeadline == elapsedDeadline)
                {
                    return LobbyMutationOutcome.Ignored;
                }

                state.ModeDeadline = retryDeadline;
                return LobbyMutationOutcome.Applied;
            case GameModeAdvanceKind.STATE_CHANGED:
                state.ModeDeadline = advance.NextDeadline;
                state.ModeDeadlineRoundId =
                    advance.NextDeadline is null
                        ? null
                        : roundId;
                if (advance.NextDeadline
                    is DateTimeOffset nextDeadline)
                {
                    ScheduleModeDeadline(
                        runtime,
                        lobbyId,
                        roundId,
                        nextDeadline);
                }

                return LobbyMutationOutcome.Applied;
            case GameModeAdvanceKind.COMPLETED:
                var completedRoundResults = advance.CompletedRoundResults
                    ?? throw new InvalidOperationException(
                        "A completed game-mode outcome requires round results.");
                state.CompleteRound(completedRoundResults);
                return LobbyMutationOutcome.AppliedWithCompletedRound(
                    completedRoundResults.RoundId);
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(advance),
                    advance.Kind,
                    "The game-mode advance kind is unsupported.");
        }
    }

    private static bool MatchesPreparationDeadline(
        LobbyRuntimeState state,
        LobbyDeadlineKey key,
        Guid lobbyId,
        Guid roundId,
        DateTimeOffset deadline) =>
        state.LobbyId == lobbyId
        && key.Kind == LobbyDeadlineKind.PREPARATION
        && key.RoundId == roundId
        && key.UserId is null
        && state.Status == LobbyStatus.PREPARING
        && state.CurrentRound
            is IGameModeRoundState round
        && round.RoundId == roundId
        && state.CurrentRoundNumber == round.RoundNumber
        && state.PreparationDeadline == deadline;

    private static bool MatchesModeDeadline(
        LobbyRuntimeState state,
        LobbyDeadlineKey key,
        Guid lobbyId,
        Guid roundId,
        DateTimeOffset deadline) =>
        state.LobbyId == lobbyId
        && key.Kind == LobbyDeadlineKind.MODE
        && key.RoundId == roundId
        && key.UserId is null
        && state.Status == LobbyStatus.PLAYING
        && state.CurrentRound
            is IGameModeRoundState round
        && round.RoundId == roundId
        && state.CurrentRoundNumber == round.RoundNumber
        && string.Equals(
            round.ModeKey,
            state.SelectedModeKey,
            StringComparison.Ordinal)
        && round.ModeVersion == state.SelectedModeVersion
        && state.ModeDeadlineRoundId == roundId
        && state.ModeDeadline == deadline;

    private LobbyOperationContext CreateOperationContext(
        LobbyRuntimeState state,
        LobbyActor actor)
    {
        var actorUserId = actor.UserId
            ?? throw new LobbyRuntimeException(
                ErrorCode.AUTH_REQUIRED,
                "An authenticated lobby actor is required.");
        var actorMayMutate = state.TryGetMember(
                actorUserId,
                out var member)
            && member?.IsConnected == true;

        return new LobbyOperationContext(
            state.LobbyId,
            actorUserId,
            state.HostUserId,
            actorMayMutate,
            state.Status,
            state.SelectedModeKey,
            state.SelectedModeVersion,
            timeProvider.GetUtcNow());
    }

    private static void CancelRemovalDeadlines(
        LobbyRuntime runtime,
        LobbyMembershipRemovalEffects effects)
    {
        runtime.CancelDeadline(effects.DisconnectDeadlineKey);
        if (effects.PreparationDeadlineKey
            is LobbyDeadlineKey preparationDeadlineKey)
        {
            runtime.CancelDeadline(preparationDeadlineKey);
        }
    }

    private static Func<
        LobbyRuntimeState,
        LobbyActor,
        CancellationToken,
        ValueTask<LobbyMutationOutcome>> WrapExpectedFailures(
        Func<
            LobbyRuntimeState,
            LobbyActor,
            CancellationToken,
            ValueTask<LobbyMutationOutcome>> mutation) =>
        async (state, actor, cancellationToken) =>
        {
            try
            {
                return await mutation(
                        state,
                        actor,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (GameModeDomainException exception)
            {
                return LobbyMutationOutcome.Rejected(
                    new LobbyOperationFailure(
                        exception.Code,
                        exception.Message,
                        exception.Errors));
            }
            catch (LobbyRuntimeException exception)
            {
                return LobbyMutationOutcome.Rejected(
                    new LobbyOperationFailure(
                        exception.Code,
                        exception.Message,
                        exception.Errors));
            }
        };

    private static LobbyOperationCompletion CreateRejectedCompletion(
        LobbyRuntime? runtime,
        LobbyOperationFailure failure) =>
        new(
            LobbyOperationDisposition.REJECTED,
            runtime?.ReadSummary().StateVersion ?? 0,
            failure);
}
