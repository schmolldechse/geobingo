using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using GeoBingo.Contracts.Common;
using GeoBingo.GameModes.Registry;
using GeoBingo.Observability.Metrics;
using Microsoft.Extensions.Logging;

namespace GeoBingo.Api.Lobbies.Runtime;

internal sealed class LobbyRegistry : ILobbyRegistry
{
    private const string CodeAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int CodeLength = 8;

    private readonly object syncRoot = new();
    private readonly Dictionary<Guid, LobbyRuntime> runtimesById = [];
    private readonly Dictionary<string, Guid> lobbyIdsByCode =
        new(StringComparer.Ordinal);
    private readonly IGameModeRegistry gameModeRegistry;
    private readonly LobbyProjectionPublisher projectionPublisher;
    private readonly TimeProvider timeProvider;
    private readonly ILoggerFactory loggerFactory;
    private readonly GameMetrics gameMetrics;
    private bool acceptingCreations = true;

    public LobbyRegistry(
        IGameModeRegistry gameModeRegistry,
        LobbyProjectionPublisher projectionPublisher,
        TimeProvider timeProvider,
        ILoggerFactory loggerFactory,
        GameMetrics gameMetrics)
    {
        this.gameModeRegistry = gameModeRegistry
            ?? throw new ArgumentNullException(nameof(gameModeRegistry));
        this.projectionPublisher = projectionPublisher
            ?? throw new ArgumentNullException(nameof(projectionPublisher));
        this.timeProvider = timeProvider
            ?? throw new ArgumentNullException(nameof(timeProvider));
        this.loggerFactory = loggerFactory
            ?? throw new ArgumentNullException(nameof(loggerFactory));
        this.gameMetrics = gameMetrics
            ?? throw new ArgumentNullException(nameof(gameMetrics));
    }

    public bool IsAcceptingCreations
    {
        get
        {
            lock (syncRoot)
            {
                return acceptingCreations;
            }
        }
    }

    public LobbyRuntime Create(LobbyCreation creation)
    {
        ArgumentNullException.ThrowIfNull(creation);

        LobbyRuntime runtime;
        lock (syncRoot)
        {
            if (!acceptingCreations)
            {
                throw new LobbyRuntimeException(
                    ErrorCode.SERVER_SHUTTING_DOWN,
                    "The server is shutting down and cannot create a lobby.");
            }

            var lobbyId = CreateUniqueLobbyId();
            var code = CreateUniqueLobbyCode();
            var defaultMode = gameModeRegistry.Default;
            var runtimeState = new LobbyRuntimeState(
                lobbyId,
                code,
                creation,
                defaultMode.Key,
                defaultMode.Version,
                defaultMode.CreateDefaultLobbyState());
            runtime = new LobbyRuntime(
                runtimeState,
                projectionPublisher,
                timeProvider,
                loggerFactory.CreateLogger<LobbyRuntime>());

            runtimesById.Add(lobbyId, runtime);
            lobbyIdsByCode.Add(code, lobbyId);
            gameMetrics.LobbyCreated(
                "WAITING",
                runtimesById.Count);
        }

        return runtime;
    }

    public bool TryGet(Guid lobbyId, out LobbyRuntime? runtime)
    {
        if (lobbyId == Guid.Empty)
        {
            runtime = null;
            return false;
        }

        lock (syncRoot)
        {
            return runtimesById.TryGetValue(lobbyId, out runtime);
        }
    }

    public bool TryResolveCode(string code, out Guid lobbyId)
    {
        if (!TryNormalizeCode(code, out var normalizedCode))
        {
            lobbyId = Guid.Empty;
            return false;
        }

        lock (syncRoot)
        {
            return lobbyIdsByCode.TryGetValue(
                normalizedCode,
                out lobbyId);
        }
    }

    public IReadOnlyList<LobbyRuntime> GetActiveRuntimes()
    {
        lock (syncRoot)
        {
            return runtimesById
                .OrderBy(entry => entry.Key)
                .Select(entry => entry.Value)
                .ToArray();
        }
    }

    public void StopAcceptingCreations()
    {
        lock (syncRoot)
        {
            acceptingCreations = false;
        }
    }

    public bool TryRemoveClosed(LobbyRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);

        var summary = runtime.ReadSummary();
        if (!summary.IsClosed || summary.CloseReason is null)
            return false;

        lock (syncRoot)
        {
            if (!runtimesById.TryGetValue(
                    runtime.LobbyId,
                    out var registeredRuntime)
                || !ReferenceEquals(registeredRuntime, runtime)
                || !lobbyIdsByCode.TryGetValue(
                    runtime.Code,
                    out var registeredLobbyId)
                || registeredLobbyId != runtime.LobbyId)
            {
                return false;
            }

            runtimesById.Remove(runtime.LobbyId);
            lobbyIdsByCode.Remove(runtime.Code);
            gameMetrics.LobbyClosed(
                summary.Status.ToString(),
                summary.CloseReason.Value.ToString(),
                runtimesById.Count);
        }

        return true;
    }

    internal static bool TryNormalizeCode(
        string? input,
        out string normalizedCode)
    {
        if (input is null)
        {
            normalizedCode = string.Empty;
            return false;
        }

        normalizedCode = input.ToUpperInvariant();
        if (normalizedCode.Length != CodeLength)
        {
            normalizedCode = string.Empty;
            return false;
        }

        foreach (var character in normalizedCode)
        {
            if (!CodeAlphabet.Contains(character))
            {
                normalizedCode = string.Empty;
                return false;
            }
        }

        return true;
    }

    private Guid CreateUniqueLobbyId()
    {
        Guid lobbyId;
        do
        {
            lobbyId = Guid.CreateVersion7();
        }
        while (runtimesById.ContainsKey(lobbyId));

        return lobbyId;
    }

    private string CreateUniqueLobbyCode()
    {
        Span<char> codeCharacters = stackalloc char[CodeLength];
        string code;
        do
        {
            for (var index = 0; index < codeCharacters.Length; index++)
            {
                codeCharacters[index] = CodeAlphabet[
                    RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
            }

            code = new string(codeCharacters);
        }
        while (lobbyIdsByCode.ContainsKey(code));

        return code;
    }
}
