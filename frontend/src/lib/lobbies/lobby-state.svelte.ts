import type {
	LobbyEnded,
	LobbyEndedReason,
	LobbyMemberView,
	LobbySnapshot,
	PersonalProjection
} from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
import { LobbyStatus } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
import type { SignalRError } from "$lib/generated/realtime/GeoBingo.Contracts.SignalR";
import type { ResultState } from "./result-state.svelte";

export type LobbyConnectionStatus = "idle" | "connecting" | "connected" | "reconnecting" | "disconnected";

export class LobbyState {
	snapshot = $state<LobbySnapshot | null>(null);
	personalProjection = $state<PersonalProjection | null>(null);
	highestStateVersion = $state(0);
	connectionStatus = $state<LobbyConnectionStatus>("idle");
	pendingOperationNames = $state<string[]>([]);
	lastOperationError = $state<SignalRError | null>(null);
	terminationReason = $state<LobbyEndedReason | null>(null);

	readonly #results: ResultState;

	public constructor(results: ResultState) {
		this.#results = results;
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

	public receiveSnapshot(snapshot: LobbySnapshot): void {
		if (snapshot.stateVersion < this.highestStateVersion) return;
		this.snapshot = snapshot;
		this.highestStateVersion = Math.max(this.highestStateVersion, snapshot.stateVersion);
		this.terminationReason = null;
	}

	public receivePersonalProjection(projection: PersonalProjection): void {
		if (projection.stateVersion < this.highestStateVersion) return;
		this.personalProjection = projection;
		this.highestStateVersion = Math.max(this.highestStateVersion, projection.stateVersion);
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
		this.clearForTermination(message.reason);
	}

	public terminateLocally(reason: LobbyEndedReason): void {
		this.clearForTermination(reason);
	}

	private clearForTermination(reason: LobbyEndedReason): void {
		this.snapshot = null;
		this.personalProjection = null;
		this.pendingOperationNames = [];
		this.lastOperationError = null;
		this.#results.clear();
		this.terminationReason = reason;
		this.connectionStatus = "disconnected";
	}
}
