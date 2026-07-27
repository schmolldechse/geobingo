using System;
using System.Threading;
using System.Threading.Tasks;
using GeoBingo.Contracts.Common;
using GeoBingo.GameModes.Abstractions;
using GeoBingo.GameModes.CaptureChallenge;

namespace GeoBingo.Api.Lobbies.Runtime;

internal sealed class LobbyOperationDispatcher
{
    private readonly ILobbyRegistry lobbyRegistry;
    private readonly LobbyConnectionRegistry connectionRegistry;
    private readonly CaptureChallengeOperations captureChallengeOperations;

    public LobbyOperationDispatcher(
        ILobbyRegistry lobbyRegistry,
        LobbyConnectionRegistry connectionRegistry,
        CaptureChallengeOperations captureChallengeOperations)
    {
        this.lobbyRegistry = lobbyRegistry
            ?? throw new ArgumentNullException(nameof(lobbyRegistry));
        this.connectionRegistry = connectionRegistry
            ?? throw new ArgumentNullException(nameof(connectionRegistry));
        this.captureChallengeOperations = captureChallengeOperations
            ?? throw new ArgumentNullException(
                nameof(captureChallengeOperations));
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

    public ValueTask<LobbyOperationCompletion> DispatchForConnectionAsync(
        string operationName,
        string connectionId,
        Guid authenticatedUserId,
        Func<LobbyRuntimeState, LobbyActor, CancellationToken,
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
            WrapExpectedFailures(mutation),
            cancellationToken);
    }

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
