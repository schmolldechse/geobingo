import type { ResultSelection } from "$lib/lobbies/result-state.svelte";

const getRoundSelection = (roundId: string, isLatest: boolean): ResultSelection =>
	isLatest ? { kind: "latest" } : { kind: "round", roundId };

const isRoundSelected = (selection: ResultSelection, roundId: string, isLatest: boolean): boolean =>
	(isLatest && selection.kind === "latest") || (selection.kind === "round" && selection.roundId === roundId);

export { getRoundSelection, isRoundSelected };
