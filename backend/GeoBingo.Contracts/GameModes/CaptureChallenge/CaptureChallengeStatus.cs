using System.ComponentModel;
using Tapper;

namespace GeoBingo.Contracts.GameModes.CaptureChallenge;

[TranspilationSource]
[Description("The active phase of a Capture Challenge round.")]
public enum CaptureChallengeStatus
{
    [Description("The round is in the capturing phase.")]
    CAPTURING,

    [Description("The round is in the voting phase.")]
    VOTING
}
