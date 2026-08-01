using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.Lobbies;

namespace GeoBingo.Api.Lobbies.Runtime;

internal static class LobbyStateMachine
{
    public static void EnsureTransitionAllowed(
        LobbyStatus current,
        LobbyStatus next)
    {
        if (IsTransitionAllowed(current, next))
        {
            return;
        }

        throw new LobbyRuntimeException(
            ErrorCode.INVALID_LOBBY_STATUS,
            $"The lobby cannot transition from {current} to {next}.");
    }

    private static bool IsTransitionAllowed(
        LobbyStatus current,
        LobbyStatus next) =>
        (current, next) switch
        {
            (LobbyStatus.WAITING, LobbyStatus.PREPARING) => true,
            (LobbyStatus.PREPARING, LobbyStatus.WAITING) => true,
            (LobbyStatus.PREPARING, LobbyStatus.PLAYING) => true,
            (LobbyStatus.PLAYING, LobbyStatus.WAITING) => true,
            _ => false
        };
}
