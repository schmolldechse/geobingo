using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoBingo.Api.Lobbies.Runtime;

internal sealed record LobbyConnectionContext(
    string ConnectionId,
    Guid UserId,
    Guid? LobbyId);

internal enum LobbyConnectionAssociation
{
    ASSOCIATED,
    ALREADY_ASSOCIATED,
    UNKNOWN_CONNECTION,
    USER_MISMATCH,
    ALREADY_IN_ANOTHER_LOBBY
}

internal sealed class LobbyConnectionRegistry
{
    private readonly object syncRoot = new();
    private readonly Dictionary<string, LobbyConnectionContext> connections = new(StringComparer.Ordinal);
    private readonly Dictionary<LobbyConnectionMembershipKey, HashSet<string>> connectionsByLobbyMember = [];

    public void RegisterConnection(string connectionId, Guid userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        if (userId == Guid.Empty)
            throw new ArgumentException("The user identifier cannot be empty.", nameof(userId));

        lock (syncRoot)
        {
            if (connections.TryGetValue(connectionId, out var existing))
            {
                if (existing.UserId != userId)
                    throw new InvalidOperationException("A connection cannot be registered for a different user.");
                return;
            }

            connections.Add(
                connectionId,
                new LobbyConnectionContext(
                    connectionId,
                    userId,
                    LobbyId: null));
        }
    }

    public LobbyConnectionAssociation TryAssociate(
        string connectionId,
        Guid userId,
        Guid lobbyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "The user identifier cannot be empty.",
                nameof(userId));

        if (lobbyId == Guid.Empty)
            throw new ArgumentException(
                "The lobby identifier cannot be empty.",
                nameof(lobbyId));

        lock (syncRoot)
        {
            if (!connections.TryGetValue(connectionId, out var existing))
                return LobbyConnectionAssociation.UNKNOWN_CONNECTION;

            if (existing.UserId != userId)
                return LobbyConnectionAssociation.USER_MISMATCH;

            if (existing.LobbyId == lobbyId)
                return LobbyConnectionAssociation.ALREADY_ASSOCIATED;

            if (existing.LobbyId is not null)
                return LobbyConnectionAssociation.ALREADY_IN_ANOTHER_LOBBY;

            var membershipKey = new LobbyConnectionMembershipKey(
                lobbyId,
                userId);
            if (!connectionsByLobbyMember.TryGetValue(
                    membershipKey,
                    out var memberConnections))
            {
                memberConnections = new HashSet<string>(
                    StringComparer.Ordinal);
                connectionsByLobbyMember.Add(
                    membershipKey,
                    memberConnections);
            }

            memberConnections.Add(connectionId);
            connections[connectionId] = existing with { LobbyId = lobbyId };
            return LobbyConnectionAssociation.ASSOCIATED;
        }
    }

    public bool TryGet(
        string connectionId,
        out LobbyConnectionContext? context)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
        {
            context = null;
            return false;
        }

        lock (syncRoot)
        {
            if (!connections.TryGetValue(connectionId, out var existing))
            {
                context = null;
                return false;
            }

            context = existing with { };
            return true;
        }
    }

    public IReadOnlyList<string> GetConnections(
        Guid lobbyId,
        Guid userId)
    {
        if (lobbyId == Guid.Empty || userId == Guid.Empty)
            return [];

        lock (syncRoot)
        {
            var membershipKey = new LobbyConnectionMembershipKey(
                lobbyId,
                userId);
            return connectionsByLobbyMember.TryGetValue(
                membershipKey,
                out var memberConnections)
                    ? memberConnections.ToArray()
                    : [];
        }
    }

    public LobbyConnectionContext? Disassociate(string connectionId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
            return null;

        lock (syncRoot)
        {
            if (!connections.TryGetValue(connectionId, out var existing))
                return null;

            if (existing.LobbyId is Guid lobbyId)
            {
                RemoveMembershipConnection(
                    new LobbyConnectionMembershipKey(
                        lobbyId,
                        existing.UserId),
                    connectionId);
                connections[connectionId] = existing with { LobbyId = null };
            }

            return existing with { };
        }
    }

    public LobbyConnectionContext? RemoveConnection(string connectionId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
            return null;

        lock (syncRoot)
        {
            if (!connections.Remove(connectionId, out var existing))
                return null;

            if (existing.LobbyId is Guid lobbyId)
            {
                RemoveMembershipConnection(
                    new LobbyConnectionMembershipKey(
                        lobbyId,
                        existing.UserId),
                    connectionId);
            }

            return existing with { };
        }
    }

    public IReadOnlyList<LobbyConnectionContext> RemoveLobby(Guid lobbyId)
    {
        if (lobbyId == Guid.Empty)
            return [];

        lock (syncRoot)
        {
            var removedContexts = connections.Values
                .Where(context => context.LobbyId == lobbyId)
                .Select(context => context with { })
                .ToArray();

            foreach (var context in removedContexts)
            {
                connections[context.ConnectionId] =
                    context with { LobbyId = null };
            }

            var membershipKeys = connectionsByLobbyMember.Keys
                .Where(key => key.LobbyId == lobbyId)
                .ToArray();
            foreach (var membershipKey in membershipKeys)
            {
                connectionsByLobbyMember.Remove(membershipKey);
            }

            return removedContexts;
        }
    }

    private void RemoveMembershipConnection(
        LobbyConnectionMembershipKey membershipKey,
        string connectionId)
    {
        if (!connectionsByLobbyMember.TryGetValue(
                membershipKey,
                out var memberConnections))
        {
            return;
        }

        memberConnections.Remove(connectionId);
        if (memberConnections.Count == 0)
        {
            connectionsByLobbyMember.Remove(membershipKey);
        }
    }

    private readonly record struct LobbyConnectionMembershipKey(
        Guid LobbyId,
        Guid UserId);
}
