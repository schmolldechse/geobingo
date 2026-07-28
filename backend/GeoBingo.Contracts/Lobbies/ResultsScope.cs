using System.ComponentModel;
using Tapper;

namespace GeoBingo.Contracts.Lobbies;

[TranspilationSource]
[Description("The finite result projection requested by a lobby member.")]
public enum ResultsScope
{
    LAST_COMPLETED_ROUND,
    SPECIFIC_ROUND,
    CUMULATIVE
}
