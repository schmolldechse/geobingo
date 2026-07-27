using System;

namespace GeoBingo.GameModes.CaptureChallenge;

internal static class CaptureChallengeMetadata
{
    public const string Key = "capture_challenge";
    public const string DisplayName = "Capture Challenge";
    public const string Description =
        "Find Street View captures that match the configured goals and evaluate the other players' submissions.";
    public const int Order = 100;

    public static Version Version { get; } = new(1, 0, 0);
}
