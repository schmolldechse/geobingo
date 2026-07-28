using System.ComponentModel;
using Tapper;

namespace GeoBingo.Contracts.Lobbies;

[TranspilationSource]
[Description("The reason an in-memory lobby permanently ended.")]
public enum LobbyEndedReason
{
    CLOSED_BY_HOST,
    NO_MEMBERS,
    KICKED,
    BANNED,
    LEFT,
    SERVER_SHUTDOWN,
    SERVER_ERROR
}
