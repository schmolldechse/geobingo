using System.ComponentModel;
using Tapper;

namespace GeoBingo.Contracts.GameModes.CaptureChallenge;

[TranspilationSource]
[Description("The finite value of a Capture Challenge vote.")]
public enum VoteValue
{
    [Description("The evaluated capture satisfies the goal.")]
    GOOD,

    [Description("The evaluated capture does not satisfy the goal.")]
    BAD
}
