using System;
using System.Collections.Generic;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.SignalR;
using GeoBingo.GameModes.Abstractions;

namespace GeoBingo.Api.Hubs;

internal sealed class SignalRErrorMapper
{
    private readonly HubInvocationMetadataAccessor metadataAccessor;

    public SignalRErrorMapper(
        HubInvocationMetadataAccessor metadataAccessor)
    {
        this.metadataAccessor = metadataAccessor
            ?? throw new ArgumentNullException(
                nameof(metadataAccessor));
    }

    public HubOperationResult FromCompletion(
        LobbyOperationCompletion completion)
    {
        ArgumentNullException.ThrowIfNull(completion);

        return completion.Disposition
            == LobbyOperationDisposition.REJECTED
                ? Rejected(
                    completion.Failure!.Code,
                    completion.Failure.Message,
                    completion.StateVersion,
                    completion.Failure.Errors)
                : Success(completion.StateVersion);
    }

    public HubOperationResult Success(long stateVersion) =>
        new()
        {
            Success = true,
            StateVersion = stateVersion,
            Error = null
        };

    public HubOperationResult Validation(
        IReadOnlyDictionary<string, string[]> errors) =>
        Rejected(
            ErrorCode.VALIDATION_FAILED,
            "The request contains invalid values.",
            stateVersion: null,
            errors);

    public HubOperationResult FromException(Exception exception) =>
        exception switch
        {
            LobbyRuntimeException lobbyException =>
                Rejected(
                    lobbyException.Code,
                    lobbyException.Message,
                    stateVersion: null,
                    lobbyException.Errors),
            GameModeDomainException modeException =>
                Rejected(
                    modeException.Code,
                    modeException.Message,
                    stateVersion: null,
                    modeException.Errors),
            _ => Rejected(
                ErrorCode.INTERNAL_ERROR,
                "The operation could not be completed.",
                stateVersion: null,
                errors: null)
        };

    public HubOperationResult Rejected(
        ErrorCode code,
        string details,
        long? stateVersion,
        IReadOnlyDictionary<string, string[]>? errors)
    {
        var metadata = metadataAccessor.Current;
        return new HubOperationResult
        {
            Success = false,
            StateVersion = stateVersion is > 0
                ? stateVersion
                : null,
            Error = new SignalRError
            {
                Code = code,
                Retryable = IsRetryable(code),
                Details = details,
                CurrentStateVersion = stateVersion is > 0
                    ? stateVersion
                    : null,
                CorrelationId = metadata.CorrelationId,
                TraceId = metadata.TraceId,
                Errors = errors
            }
        };
    }

    private static bool IsRetryable(ErrorCode code) =>
        code is ErrorCode.RATE_LIMITED
            or ErrorCode.SERVER_SHUTTING_DOWN;
}
