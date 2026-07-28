using System.ComponentModel;
using Tapper;

namespace GeoBingo.Contracts.Lobbies;

[TranspilationSource]
[Description("The finite way a host removes another lobby member.")]
public enum PlayerRemovalKind
{
    KICK,
    BAN
}
