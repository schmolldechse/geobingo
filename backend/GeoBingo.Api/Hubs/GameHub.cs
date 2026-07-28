using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GeoBingo.Api.Authentication;
using GeoBingo.Api.Lobbies.Mapping;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Api.Lobbies.Transport;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.GameModes.CaptureChallenge;
using GeoBingo.Contracts.Lobbies;
using GeoBingo.Contracts.SignalR;
using GeoBingo.Observability.Metrics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace GeoBingo.Api.Hubs;

[Authorize]
public sealed class GameHub : Hub<IGameClient>, IGameHub
{
    private readonly ILobbyRegistry lobbyRegistry;
    private readonly LobbyConnectionRegistry connectionRegistry;
    private readonly LobbyConnectionLifecycle connectionLifecycle;
    private readonly LobbyOperationDispatcher operationDispatcher;
    private readonly LobbyProjectionMapper projectionMapper;
    private readonly LobbyMemberTransportEjector memberEjector;
    private readonly SignalRErrorMapper errorMapper;
    private readonly GameMetrics gameMetrics;

    public GameHub(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        lobbyRegistry =
            services.GetRequiredService<ILobbyRegistry>();
        connectionRegistry =
            services.GetRequiredService<LobbyConnectionRegistry>();
        connectionLifecycle =
            services.GetRequiredService<LobbyConnectionLifecycle>();
        operationDispatcher =
            services.GetRequiredService<LobbyOperationDispatcher>();
        projectionMapper =
            services.GetRequiredService<LobbyProjectionMapper>();
        memberEjector =
            services.GetRequiredService<LobbyMemberTransportEjector>();
        errorMapper =
            services.GetRequiredService<SignalRErrorMapper>();
        gameMetrics = services.GetRequiredService<GameMetrics>();
    }

    public override async Task OnConnectedAsync()
    {
        var identity = ReadIdentity();
        connectionRegistry.RegisterConnection(
            Context.ConnectionId,
            identity.UserId);
        gameMetrics.SignalRConnectionOpened();
        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        try
        {
            await connectionLifecycle.HandleDisconnectedAsync(
                    Context.ConnectionId,
                    default)
                .ConfigureAwait(false);
        }
        finally
        {
            gameMetrics.SignalRConnectionClosed();
            await base.OnDisconnectedAsync(exception)
                .ConfigureAwait(false);
        }
    }

    public async Task<HubOperationResult> JoinLobby(
        JoinLobbyRequest request)
    {
        var identity = ReadIdentity();
        if (!lobbyRegistry.IsAcceptingCreations)
        {
            return errorMapper.Rejected(
                ErrorCode.SERVER_SHUTTING_DOWN,
                "The server is shutting down and cannot accept lobby joins.",
                stateVersion: null,
                errors: null);
        }

        if (!LobbyRegistry.TryNormalizeCode(
                request.Code,
                out var normalizedCode)
            || !lobbyRegistry.TryResolveCode(
                normalizedCode,
                out var lobbyId)
            || !lobbyRegistry.TryGet(
                lobbyId,
                out var runtime)
            || runtime is null)
        {
            return errorMapper.Rejected(
                ErrorCode.LOBBY_NOT_FOUND,
                "The lobby was not found.",
                stateVersion: null,
                errors: null);
        }

        var completion =
            await connectionLifecycle.JoinOrReconnectAsync(
                    runtime,
                    new LobbyJoinIdentity(
                        identity.UserId,
                        identity.Handle,
                        identity.DisplayName),
                    Context.ConnectionId,
                    Context.ConnectionAborted)
                .ConfigureAwait(false);
        var result = errorMapper.FromCompletion(completion);
        if (!result.Success)
        {
            return result;
        }

        await Groups.AddToGroupAsync(
                Context.ConnectionId,
                LobbyHubGroups.Lobby(lobbyId),
                Context.ConnectionAborted)
            .ConfigureAwait(false);
        await Groups.AddToGroupAsync(
                Context.ConnectionId,
                LobbyHubGroups.Personal(
                    lobbyId,
                    identity.UserId),
                Context.ConnectionAborted)
            .ConfigureAwait(false);

        var projection = await runtime.EnqueueReadAsync(
                (state, _) => ValueTask.FromResult(
                    projectionMapper.CreateForMember(
                        state,
                        identity.UserId)),
                Context.ConnectionAborted)
            .ConfigureAwait(false);
        await Clients.Caller
            .ReceiveLobbySnapshot(projection.Snapshot)
            .ConfigureAwait(false);
        await Clients.Caller
            .ReceivePersonalProjection(
                projection.PersonalProjection)
            .ConfigureAwait(false);
        return result;
    }

    public async Task<HubOperationResult> LeaveLobby()
    {
        var identity = ReadIdentity();
        var lobbyId = ResolveCurrentLobbyId(identity.UserId);
        var completion =
            await operationDispatcher.LeaveAsync(
                    Context.ConnectionId,
                    identity.UserId,
                    Context.ConnectionAborted)
                .ConfigureAwait(false);
        var result = errorMapper.FromCompletion(completion);
        if (result.Success)
        {
            await memberEjector.EjectAsync(
                    lobbyId,
                    identity.UserId,
                    LobbyEndedReason.LEFT,
                    completion.StateVersion,
                    Context.ConnectionAborted)
                .ConfigureAwait(false);
        }

        return result;
    }

    public Task<HubOperationResult> CloseLobby() =>
        ExecuteAsync((identity) =>
            operationDispatcher.CloseAsync(
                Context.ConnectionId,
                identity.UserId,
                Context.ConnectionAborted));

    public async Task<HubOperationResult> RemovePlayer(
        RemovePlayerRequest request)
    {
        var identity = ReadIdentity();
        var lobbyId = ResolveCurrentLobbyId(identity.UserId);
        var completion =
            await operationDispatcher.RemovePlayerAsync(
                    Context.ConnectionId,
                    identity.UserId,
                    request,
                    Context.ConnectionAborted)
                .ConfigureAwait(false);
        var result = errorMapper.FromCompletion(completion);
        if (result.Success)
        {
            await memberEjector.EjectAsync(
                    lobbyId,
                    request.UserId,
                    request.Kind == PlayerRemovalKind.BAN
                        ? LobbyEndedReason.BANNED
                        : LobbyEndedReason.KICKED,
                    completion.StateVersion,
                    Context.ConnectionAborted)
                .ConfigureAwait(false);
        }

        return result;
    }

    public Task<HubOperationResult> TransferHost(
        TransferHostRequest request) =>
        ExecuteAsync(identity =>
            operationDispatcher.TransferHostAsync(
                Context.ConnectionId,
                identity.UserId,
                request,
                Context.ConnectionAborted));

    public Task<HubOperationResult> SelectGameMode(
        SelectGameModeRequest request) =>
        ExecuteAsync(identity =>
            operationDispatcher.SelectGameModeAsync(
                Context.ConnectionId,
                identity.UserId,
                request,
                Context.ConnectionAborted));

    public Task<HubOperationResult> UpdateLobbySettings(
        UpdateLobbySettingsRequest request) =>
        ExecuteAsync(identity =>
            operationDispatcher.UpdateLobbySettingsAsync(
                Context.ConnectionId,
                identity.UserId,
                request,
                Context.ConnectionAborted));

    public Task<HubOperationResult> StartRound() =>
        ExecuteAsync(identity =>
            operationDispatcher.StartRoundAsync(
                Context.ConnectionId,
                identity.UserId,
                Context.ConnectionAborted));

    public Task<HubOperationResult>
        UpdateCaptureChallengeSettings(
            UpdateCaptureChallengeSettingsRequest request) =>
        ExecuteAsync(identity =>
            operationDispatcher
                .UpdateCaptureChallengeSettingsAsync(
                    Context.ConnectionId,
                    identity.UserId,
                    request,
                    Context.ConnectionAborted));

    public Task<HubOperationResult> AddCaptureGoal(
        AddCaptureGoalRequest request) =>
        ExecuteAsync(identity =>
            operationDispatcher.AddCaptureGoalAsync(
                Context.ConnectionId,
                identity.UserId,
                request,
                Context.ConnectionAborted));

    public Task<HubOperationResult> UpdateCaptureGoal(
        UpdateCaptureGoalRequest request) =>
        ExecuteAsync(identity =>
            operationDispatcher.UpdateCaptureGoalAsync(
                Context.ConnectionId,
                identity.UserId,
                request,
                Context.ConnectionAborted));

    public Task<HubOperationResult> RemoveCaptureGoal(
        RemoveCaptureGoalRequest request) =>
        ExecuteAsync(identity =>
            operationDispatcher.RemoveCaptureGoalAsync(
                Context.ConnectionId,
                identity.UserId,
                request,
                Context.ConnectionAborted));

    public Task<HubOperationResult> ReorderCaptureGoals(
        ReorderCaptureGoalsRequest request) =>
        ExecuteAsync(identity =>
            operationDispatcher.ReorderCaptureGoalsAsync(
                Context.ConnectionId,
                identity.UserId,
                request,
                Context.ConnectionAborted));

    public Task<HubOperationResult> SubmitCapture(
        SubmitCaptureRequest request) =>
        ExecuteAsync(identity =>
            operationDispatcher.SubmitCaptureAsync(
                Context.ConnectionId,
                identity.UserId,
                request,
                Context.ConnectionAborted));

    public Task<HubOperationResult> UpdateCapture(
        UpdateCaptureRequest request) =>
        ExecuteAsync(identity =>
            operationDispatcher.UpdateCaptureAsync(
                Context.ConnectionId,
                identity.UserId,
                request,
                Context.ConnectionAborted));

    public Task<HubOperationResult> RemoveCapture(
        RemoveCaptureRequest request) =>
        ExecuteAsync(identity =>
            operationDispatcher.RemoveCaptureAsync(
                Context.ConnectionId,
                identity.UserId,
                request,
                Context.ConnectionAborted));

    public Task<HubOperationResult> CastVote(
        CastVoteRequest request) =>
        ExecuteAsync(identity =>
            operationDispatcher.CastVoteAsync(
                Context.ConnectionId,
                identity.UserId,
                request,
                Context.ConnectionAborted));

    public Task<HubOperationResult> ChangeVote(
        ChangeVoteRequest request) =>
        ExecuteAsync(identity =>
            operationDispatcher.ChangeVoteAsync(
                Context.ConnectionId,
                identity.UserId,
                request,
                Context.ConnectionAborted));

    public async Task<HubOperationResult> RequestSnapshot()
    {
        var identity = ReadIdentity();
        var runtime = ResolveCurrentRuntime(identity.UserId);
        var projection = await runtime.EnqueueReadAsync(
                (state, _) => ValueTask.FromResult(
                    projectionMapper.CreateForMember(
                        state,
                        identity.UserId)),
                Context.ConnectionAborted)
            .ConfigureAwait(false);
        await Clients.Caller
            .ReceiveLobbySnapshot(projection.Snapshot)
            .ConfigureAwait(false);
        await Clients.Caller
            .ReceivePersonalProjection(
                projection.PersonalProjection)
            .ConfigureAwait(false);
        return errorMapper.Success(
            projection.Snapshot.StateVersion);
    }

    public async Task<HubOperationResult> RequestResults(
        RequestResultsRequest request)
    {
        var identity = ReadIdentity();
        var runtime = ResolveCurrentRuntime(identity.UserId);
        var results = await runtime.EnqueueReadAsync(
                (state, _) => ValueTask.FromResult(
                    CreateResultsView(
                        state,
                        identity.UserId,
                        request)),
                Context.ConnectionAborted)
            .ConfigureAwait(false);
        await Clients.Caller
            .ReceiveResults(results)
            .ConfigureAwait(false);
        return errorMapper.Success(results.StateVersion);
    }

    private async Task<HubOperationResult> ExecuteAsync(
        Func<
            HubIdentity,
            ValueTask<LobbyOperationCompletion>> operation)
    {
        var identity = ReadIdentity();
        var completion = await operation(identity)
            .ConfigureAwait(false);
        return errorMapper.FromCompletion(completion);
    }

    private LobbyRuntime ResolveCurrentRuntime(Guid userId)
    {
        if (!operationDispatcher.TryResolveForConnection(
                Context.ConnectionId,
                userId,
                out var runtime,
                out var failure)
            || runtime is null)
        {
            throw new LobbyRuntimeException(
                failure!.Code,
                failure.Message,
                failure.Errors);
        }

        return runtime;
    }

    private Guid ResolveCurrentLobbyId(Guid userId) =>
        ResolveCurrentRuntime(userId).LobbyId;

    private LobbyResultsView CreateResultsView(
        LobbyRuntimeState state,
        Guid userId,
        RequestResultsRequest request)
    {
        if (!state.Members.ContainsKey(userId))
        {
            throw new LobbyRuntimeException(
                ErrorCode.NOT_A_MEMBER,
                "The current user is not a lobby member.");
        }

        if (request.Scope == ResultsScope.SPECIFIC_ROUND
            && request.RoundId is null
            || request.Scope != ResultsScope.SPECIFIC_ROUND
            && request.RoundId is not null)
        {
            throw new LobbyRuntimeException(
                ErrorCode.VALIDATION_FAILED,
                "The result scope and round identifier do not match.");
        }

        CompletedRoundResults? roundResults =
            request.Scope switch
            {
                ResultsScope.CUMULATIVE => null,
                ResultsScope.LAST_COMPLETED_ROUND =>
                    state.CompletedRoundResults
                        .LastOrDefault()
                    ?? throw new LobbyRuntimeException(
                        ErrorCode.ROUND_NOT_FOUND,
                        "No completed round is available."),
                ResultsScope.SPECIFIC_ROUND =>
                    state.CompletedRoundResults
                        .FirstOrDefault(result =>
                            result.RoundId == request.RoundId)
                    ?? ResolveMissingRound(state, request.RoundId),
                _ => throw new LobbyRuntimeException(
                    ErrorCode.VALIDATION_FAILED,
                    "The result scope is invalid.")
            };

        return new LobbyResultsView
        {
            LobbyId = state.LobbyId,
            StateVersion = state.StateVersion,
            Scope = request.Scope,
            RoundId = roundResults?.RoundId,
            RoundNumber = roundResults?.RoundNumber,
            CompletedAt = roundResults?.CompletedAt,
            RoundResults = roundResults,
            CumulativeResults =
                state.CumulativeResults.ToArray()
        };
    }

    private static CompletedRoundResults ResolveMissingRound(
        LobbyRuntimeState state,
        Guid? roundId)
    {
        if (roundId is Guid requestedRoundId
            && state.CurrentRound?.RoundId
                == requestedRoundId)
        {
            throw new LobbyRuntimeException(
                ErrorCode.ROUND_NOT_COMPLETED,
                "The requested round is not complete.");
        }

        throw new LobbyRuntimeException(
            ErrorCode.ROUND_NOT_FOUND,
            "The requested round was not found.");
    }

    private HubIdentity ReadIdentity()
    {
        var handle = Context.User?.FindFirstValue(
            GeoBingoClaimTypes.Handle);
        var displayName = Context.User?.FindFirstValue(
            ClaimTypes.Name);
        if (!Guid.TryParse(
                Context.User?.FindFirstValue(
                    ClaimTypes.NameIdentifier),
                out var userId)
            || userId == Guid.Empty
            || string.IsNullOrWhiteSpace(handle)
            || string.IsNullOrWhiteSpace(displayName))
        {
            throw new LobbyRuntimeException(
                ErrorCode.AUTH_REQUIRED,
                "The current connection has no valid authenticated identity.");
        }

        return new HubIdentity(
            userId,
            handle,
            displayName);
    }

    private sealed record HubIdentity(
        Guid UserId,
        string Handle,
        string DisplayName);
}
