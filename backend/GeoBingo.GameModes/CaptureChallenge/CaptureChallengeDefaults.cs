namespace GeoBingo.GameModes.CaptureChallenge;

public static class CaptureChallengeDefaults
{
    public const int MinimumCaptureDurationSeconds = 3 * 60;
    public const int MaximumCaptureDurationSeconds = 60 * 60;
    public const int DefaultCaptureDurationSeconds = 120;

    public const int MinimumSecondsPerVote = 5;
    public const int MaximumSecondsPerVote = 60;
    public const int DefaultSecondsPerVote = 20;

    public const int MaximumCaptureSlots = 500;

    public const decimal DefaultScoreFactor = 1m;
    public const decimal ScoreFactorStep = 0.5m;
}
