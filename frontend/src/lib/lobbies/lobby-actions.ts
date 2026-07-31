import type {
	CaptureGoalInput,
	CaptureChallengeSettings,
	StreetViewPosition,
	VoteValue
} from "$lib/generated/realtime/GeoBingo.Contracts.GameModes.CaptureChallenge";
import { PlayerRemovalKind, ResultsScope, type LobbySettings } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
import type { IGameHub } from "$lib/generated/realtime/TypedSignalR.Client/GeoBingo.Contracts.SignalR";
import type { GameConnection } from "$lib/realtime/game-connection.svelte";
import type { LobbyState } from "./lobby-state.svelte";
import type { ResultState } from "./result-state.svelte";

export class LobbyActions {
	readonly #connection: GameConnection;
	readonly #lobby: LobbyState;
	readonly #results: ResultState;

	public constructor(connection: GameConnection, lobby: LobbyState, results: ResultState) {
		this.#connection = connection;
		this.#lobby = lobby;
		this.#results = results;
	}

	public leaveLobby = () => this.#mutate("leaveLobby", (hub) => hub.leaveLobby());
	public closeLobby = () => this.#mutate("closeLobby", (hub) => hub.closeLobby());
	public startRound = () => this.#mutate("startRound", (hub) => hub.startRound());

	public updateLobbySettings = (settings: LobbySettings) =>
		this.#mutate("updateLobbySettings", (hub) => hub.updateLobbySettings({ settings }));

	public updateCaptureChallengeSettings = (settings: CaptureChallengeSettings) =>
		this.#mutate("updateCaptureChallengeSettings", (hub) => hub.updateCaptureChallengeSettings({ settings }));

	public addGoal = (goal: CaptureGoalInput) => this.#mutate("addGoal", (hub) => hub.addCaptureGoal({ goal }));

	public updateGoal = (goalId: string, goal: CaptureGoalInput) =>
		this.#mutate(`updateGoal:${goalId}`, (hub) => hub.updateCaptureGoal({ goalId, goal }));

	public removeGoal = (goalId: string) => this.#mutate(`removeGoal:${goalId}`, (hub) => hub.removeCaptureGoal({ goalId }));

	public reorderGoals = (goalIdsInDisplayOrder: string[]) =>
		this.#mutate("reorderGoals", (hub) => hub.reorderCaptureGoals({ goalIdsInDisplayOrder }));

	public submitCapture = (roundGoalId: string, position: StreetViewPosition) =>
		this.#mutate(`submitCapture:${roundGoalId}`, (hub) => hub.submitCapture({ roundGoalId, position }));

	public updateCapture = (captureId: string, position: StreetViewPosition) =>
		this.#mutate(`updateCapture:${captureId}`, (hub) => hub.updateCapture({ captureId, position }));

	public removeCapture = (captureId: string) =>
		this.#mutate(`removeCapture:${captureId}`, (hub) => hub.removeCapture({ captureId }));

	public castVote = (captureId: string, value: VoteValue) =>
		this.#mutate(`castVote:${captureId}`, (hub) => hub.castVote({ captureId, value }));

	public changeVote = (captureId: string, value: VoteValue) =>
		this.#mutate(`changeVote:${captureId}`, (hub) => hub.changeVote({ captureId, value }));

	public transferHost = (userId: string) => this.#mutate(`transferHost:${userId}`, (hub) => hub.transferHost({ userId }));

	public removePlayer = (userId: string, kind: PlayerRemovalKind) =>
		this.#mutate(`${kind.toLowerCase()}:${userId}`, (hub) => hub.removePlayer({ userId, kind }));

	public kickPlayer = (userId: string) => this.removePlayer(userId, PlayerRemovalKind.KICK);
	public banPlayer = (userId: string) => this.removePlayer(userId, PlayerRemovalKind.BAN);

	public selectGameMode = (modeKey: string, confirmModeStateReset = false) =>
		this.#mutate("selectGameMode", (hub) => hub.selectGameMode({ modeKey, confirmModeStateReset }));

	public requestVisibleResults = async (): Promise<void> => {
		const roundId =
			this.#results.visibleScope === ResultsScope.SPECIFIC_ROUND ? (this.#results.visibleRoundId ?? undefined) : undefined;
		await this.#connection.execute("requestResults", (hub) =>
			hub.requestResults({ scope: this.#results.visibleScope, roundId })
		);
	};

	public selectResults = async (scope: ResultsScope, roundId: string | null = null) => {
		this.#results.select(scope, roundId);
		await this.requestVisibleResults();
	};

	#mutate(operationName: string, operation: (hub: IGameHub) => Promise<unknown>): Promise<void> {
		if (this.#lobby.isPending(operationName)) return Promise.resolve();
		this.#lobby.setPending(operationName, true);

		return this.#connection
			.execute(operationName, operation)
			.then(() => undefined)
			.finally(() => this.#lobby.setPending(operationName, false));
	}
}
