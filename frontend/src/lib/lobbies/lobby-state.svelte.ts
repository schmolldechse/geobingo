import type {
	LobbyEnded,
	LobbyEndedReason,
	LobbyMemberView,
	LobbyProjection,
	LobbySnapshot,
	PersonalProjection
} from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
import { LobbyStatus } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
import type { SignalRError } from "$lib/generated/realtime/GeoBingo.Contracts.SignalR";
import type { LobbyConclusionErrorCode, LobbyConclusionSource } from "./lobby-conclusion";
import type { ResultState } from "./result-state.svelte";

export type LobbyConnectionStatus = "idle" | "connecting" | "connected" | "reconnecting" | "disconnected";

export class LobbyState {
	#projection = $state<LobbyProjection | null>(null);
	highestStateVersion = $state(0);
	connectionStatus = $state<LobbyConnectionStatus>("idle");
	pendingOperationNames = $state<string[]>([]);
	lastOperationError = $state<SignalRError | null>(null);
	conclusion = $state<LobbyConclusionSource | null>(null);

	readonly #results: ResultState;

	public constructor(results: ResultState) {
		this.#results = results;
	}

	public get snapshot(): LobbySnapshot | null {
		return this.#projection?.snapshot ?? null;
	}

	public get personalProjection(): PersonalProjection | null {
		return this.#projection?.personalProjection ?? null;
	}

	public get isHost(): boolean {
		return Boolean(this.snapshot && this.personalProjection && this.snapshot.hostUserId === this.personalProjection.userId);
	}

	public get isWaiting(): boolean {
		return this.snapshot?.status === LobbyStatus.WAITING;
	}

	public get members(): LobbyMemberView[] {
		return this.snapshot?.members ?? [];
	}

	public get selectedMode(): LobbySnapshot["selectedMode"] | null {
		return this.snapshot?.selectedMode ?? null;
	}

	public get canAdminister(): boolean {
		return this.isHost && this.isWaiting;
	}

	public receiveProjection(projection: LobbyProjection): void {
		const { snapshot, personalProjection } = projection;
		if (
			snapshot.lobbyId !== personalProjection.lobbyId ||
			snapshot.stateVersion !== personalProjection.stateVersion ||
			snapshot.stateVersion < this.highestStateVersion ||
			(this.snapshot !== null && snapshot.lobbyId !== this.snapshot.lobbyId)
		) {
			return;
		}

		this.#projection = projection;
		this.highestStateVersion = snapshot.stateVersion;
		this.conclusion = null;
	}

	public setPending(operationName: string, pending: boolean): void {
		if (pending) {
			if (!this.pendingOperationNames.includes(operationName)) {
				this.pendingOperationNames = [...this.pendingOperationNames, operationName];
			}
			return;
		}

		this.pendingOperationNames = this.pendingOperationNames.filter((name) => name !== operationName);
	}

	public isPending(operationName: string): boolean {
		return this.pendingOperationNames.includes(operationName);
	}

	public terminate(message: LobbyEnded): void {
		if (message.stateVersion < this.highestStateVersion) return;
		this.highestStateVersion = message.stateVersion;
		this.clearForConclusion({ type: "ended", reason: message.reason });
	}

	public terminateLocally(reason: LobbyEndedReason): void {
		this.clearForConclusion({ type: "ended", reason });
	}

	public rejectLocally(code: LobbyConclusionErrorCode): void {
		this.clearForConclusion({ type: "rejected", code });
	}

	private clearForConclusion(conclusion: LobbyConclusionSource): void {
		this.#projection = null;
		this.pendingOperationNames = [];
		this.lastOperationError = null;
		this.#results.clear();
		this.conclusion = conclusion;
		this.connectionStatus = "disconnected";
	}
}
