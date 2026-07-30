using System;
using System.Collections.Generic;
using System.Linq;
using GeoBingo.Contracts.GameModes.CaptureChallenge;
using GeoBingo.GameModes.Abstractions;

namespace GeoBingo.GameModes.CaptureChallenge;

public sealed class CaptureChallengeLobbyState : IGameModeLobbyState
{
    private CaptureChallengeSettings _settings;
    private readonly List<CaptureChallengeGoal> _goals;

    private CaptureChallengeLobbyState(
        CaptureChallengeSettings settings,
        IEnumerable<CaptureChallengeGoal> goals)
    {
        _settings = CopySettings(settings);
        _goals = goals.Select(CopyGoal).ToList();
    }

    public CaptureChallengeSettings Settings => CopySettings(_settings);

    internal IReadOnlyList<CaptureChallengeGoal> Goals => _goals
        .OrderBy(goal => goal.DisplayOrder)
        .ToArray();

    internal static CaptureChallengeLobbyState CreateDefault() => new(new CaptureChallengeSettings
    {
        CaptureDurationSeconds = CaptureChallengeDefaults.DefaultCaptureDurationSeconds,
        SecondsPerVote = CaptureChallengeDefaults.DefaultSecondsPerVote,
    }, []);

    internal void ReplaceSettings(CaptureChallengeSettings settings) => _settings = CopySettings(settings);

    internal void AppendGoal(CaptureChallengeGoal goal) => _goals.Add(CopyGoal(goal) with { DisplayOrder = _goals.Count });

    internal void ReplaceGoal(CaptureChallengeGoal goal)
    {
        var index = _goals.FindIndex(existing => existing.Id == goal.Id);
        _goals[index] = CopyGoal(goal);
    }

    internal void RemoveGoal(Guid goalId)
    {
        _goals.RemoveAll(goal => goal.Id == goalId);
        NormalizeDisplayOrder();
    }

    internal void ReplaceGoalDisplayOrder(IReadOnlyList<Guid> goalIdsInDisplayOrder)
    {
        var displayOrders = goalIdsInDisplayOrder
            .Select((goalId, displayOrder) => (goalId, displayOrder))
            .ToDictionary(entry => entry.goalId, entry => entry.displayOrder);

        for (var index = 0; index < _goals.Count; index++)
        {
            var goal = _goals[index];
            _goals[index] = goal with { DisplayOrder = displayOrders[goal.Id] };
        }
    }

    internal CaptureChallengeGoal? FindGoal(Guid goalId) =>
        _goals.Find(goal => goal.Id == goalId);

    internal IReadOnlyList<CaptureChallengeGoal> CreateGoalSnapshot() =>
        Goals.Select(CopyGoal).ToArray();

    internal static CaptureChallengeSettings CopySettings(CaptureChallengeSettings settings) => new()
    {
        CaptureDurationSeconds = settings.CaptureDurationSeconds,
        SecondsPerVote = settings.SecondsPerVote,
    };

    internal static CaptureChallengeGoal CopyGoal(CaptureChallengeGoal goal) => goal with { };

    private void NormalizeDisplayOrder()
    {
        var ordered = _goals
            .OrderBy(goal => goal.DisplayOrder)
            .ToArray();

        for (var displayOrder = 0; displayOrder < ordered.Length; displayOrder++)
        {
            var index = _goals.FindIndex(goal => goal.Id == ordered[displayOrder].Id);
            _goals[index] = _goals[index] with { DisplayOrder = displayOrder };
        }
    }
}

internal sealed record CaptureChallengeGoal(
    Guid Id,
    string Title,
    int DisplayOrder,
    decimal ScoreFactor);
