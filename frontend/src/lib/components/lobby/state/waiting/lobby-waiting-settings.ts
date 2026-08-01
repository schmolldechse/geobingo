import type { GameModeSummary } from "$lib/generated/api";
import type { CaptureChallengeSettings } from "$lib/generated/realtime/GeoBingo.Contracts.GameModes.CaptureChallenge";

export const SETTINGS_UPDATE_DEBOUNCE_MS = 800;

export function mergeGameModeCatalog(selectedMode: GameModeSummary, catalog: readonly GameModeSummary[]): GameModeSummary[] {
	const seenKeys = new Set<string>();
	const uniqueCatalog = catalog.filter((mode) => {
		if (seenKeys.has(mode.key)) return false;
		seenKeys.add(mode.key);
		return true;
	});

	return seenKeys.has(selectedMode.key) ? uniqueCatalog : [selectedMode, ...uniqueCatalog];
}

export function buildCaptureChallengeSettings(
	captureDurationMinutes: number,
	secondsPerVote: number
): CaptureChallengeSettings {
	return {
		captureDurationSeconds: captureDurationMinutes * 60,
		secondsPerVote
	};
}

export function reorderGoalIds(goalIds: readonly string[], index: number, direction: -1 | 1): string[] {
	const destination = index + direction;
	const reordered = [...goalIds];
	if (index < 0 || index >= reordered.length || destination < 0 || destination >= reordered.length) return reordered;

	[reordered[index], reordered[destination]] = [reordered[destination], reordered[index]];
	return reordered;
}
