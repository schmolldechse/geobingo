using System.ComponentModel;
using Tapper;

namespace GeoBingo.Contracts.Lobbies;

[TranspilationSource]
[Description("The finite type of a transient game-mode event.")]
public enum GameModeEventType
{
    CAPTURE_CHALLENGE_CAPTURING_STARTED,
    CAPTURE_CHALLENGE_VOTING_STARTED,
    CAPTURE_CHALLENGE_ROUND_COMPLETED
}
