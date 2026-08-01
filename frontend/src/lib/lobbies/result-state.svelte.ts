import type { LobbyRoundResultsView } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
import { SvelteMap } from "svelte/reactivity";

export type ResultSelection = { kind: "cumulative" } | { kind: "latest" } | { kind: "round"; roundId: string };

export class ResultState {
	selection = $state<ResultSelection>({ kind: "cumulative" });
	historicalRounds = new SvelteMap<string, LobbyRoundResultsView>();

	public receive(view: LobbyRoundResultsView): void {
		this.historicalRounds.set(view.results.roundId, view);
	}

	public select(selection: ResultSelection): void {
		this.selection = selection;
	}

	public hasHistoricalRound(roundId: string): boolean {
		return this.historicalRounds.has(roundId);
	}

	public clear(): void {
		this.historicalRounds.clear();
		this.selection = { kind: "cumulative" };
	}
}
