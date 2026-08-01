using System.ComponentModel;
using Tapper;

namespace GeoBingo.Contracts.Lobbies;

[TranspilationSource]
[Description("The global lifecycle status of a lobby.")]
public enum LobbyStatus
{
    [Description("The lobby is waiting for players to join.")]
    WAITING,

    [Description("The lobby is preparing for the game.")]
    PREPARING,

    [Description("The game is currently in progress.")]
    PLAYING
}
