using System;

namespace GeoBingo.Api.Hubs;

internal static class LobbyHubGroups
{
    public static string Lobby(Guid lobbyId) =>
        $"lobby:{lobbyId:D}";

    public static string Personal(
        Guid lobbyId,
        Guid userId) =>
        $"lobby:{lobbyId:D}:user:{userId:D}";
}
