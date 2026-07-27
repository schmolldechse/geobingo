using GeoBingo.GameModes.CaptureChallenge;

namespace GeoBingo.GameModes.Abstractions;

public sealed record GameModeProjectionPair(
    CaptureChallengePublicProjection? CaptureChallengePublic,
    CaptureChallengePersonalProjection? CaptureChallengePersonal);
