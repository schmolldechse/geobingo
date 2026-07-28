import type { LobbyResultsView } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
import { ResultsScope } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
import { SvelteMap } from "svelte/reactivity";

export class ResultState {
	latestRound = $state<LobbyResultsView | null>(null);
	cumulative = $state<LobbyResultsView | null>(null);
	specificRounds = new SvelteMap<string, LobbyResultsView>();
	visibleScope = $state<ResultsScope>(ResultsScope.CUMULATIVE);
	visibleRoundId = $state<string | null>(null);

	public receive(view: LobbyResultsView): void {
		switch (view.scope) {
			case ResultsScope.LAST_COMPLETED_ROUND:
				this.latestRound = view;
				break;
			case ResultsScope.SPECIFIC_ROUND:
				if (view.roundId) this.specificRounds.set(view.roundId, view);
				break;
			case ResultsScope.CUMULATIVE:
				this.cumulative = view;
				break;
		}
	}

	public select(scope: ResultsScope, roundId: string | null = null): void {
		this.visibleScope = scope;
		this.visibleRoundId = scope === ResultsScope.SPECIFIC_ROUND ? roundId : null;
	}

	public clear(): void {
		this.latestRound = null;
		this.cumulative = null;
		this.specificRounds.clear();
		this.visibleScope = ResultsScope.CUMULATIVE;
		this.visibleRoundId = null;
	}
}
