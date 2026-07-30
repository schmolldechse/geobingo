import { apiUrl } from "$lib/api-url";
import { ErrorCode } from "$lib/generated/realtime/GeoBingo.Contracts.Common";
import { LobbyEndedReason, ResultsScope } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
import type { HubOperationResult, SignalRError } from "$lib/generated/realtime/GeoBingo.Contracts.SignalR";
import { getHubProxyFactory, getReceiverRegister, type Disposable } from "$lib/generated/realtime/TypedSignalR.Client";
import type { IGameClient, IGameHub } from "$lib/generated/realtime/TypedSignalR.Client/GeoBingo.Contracts.SignalR";
import { isLobbyConclusionErrorCode } from "$lib/lobbies/lobby-conclusion";
import type { LobbyEventHandler } from "$lib/lobbies/lobby-events";
import type { LobbyState } from "$lib/lobbies/lobby-state.svelte";
import type { ResultState } from "$lib/lobbies/result-state.svelte";
import { HubConnectionBuilder, HubConnectionState, type HubConnection } from "@microsoft/signalr";
import { boundedReconnectPolicy } from "./reconnection-policy";
import { toast } from "../components/ui/toast";
import { createLobbyToastHandler } from "../lobbies/lobby-toast-handler";

export class GameConnection {
	readonly #code: string;
	readonly #lobby: LobbyState;
	readonly #results: ResultState;
	readonly #connection: HubConnection;
	readonly #hub: IGameHub;
	readonly #handleEvent: LobbyEventHandler;
	#receiverSubscription: Disposable | null = null;
	#stopPromise: Promise<void> | null = null;
	#intentionalStop = false;

	public constructor(code: string, lobby: LobbyState, results: ResultState) {
		this.#code = code;
		this.#lobby = lobby;
		this.#results = results;
		this.#handleEvent = createLobbyToastHandler(toast);
		this.#connection = new HubConnectionBuilder()
			.withUrl(apiUrl("/hubs/game").href, { withCredentials: true })
			.withAutomaticReconnect(boundedReconnectPolicy)
			.build();
		this.#hub = getHubProxyFactory("IGameHub").createHubProxy(this.#connection);

		const receiver: IGameClient = {
			receiveLobbySnapshot: async (snapshot) => this.#lobby.receiveSnapshot(snapshot),
			receivePersonalProjection: async (projection) => this.#lobby.receivePersonalProjection(projection),
			receiveGameModeEvent: async () => undefined,
			receiveResults: async (results) => this.#results.receive(results),
			receiveLobbyEnded: async (message) => {
				this.#lobby.terminate(message);
				void this.stop();
			},
			receiveError: async (error) => {
				this.#handleRejected(error);
			}
		};
		this.#receiverSubscription = getReceiverRegister("IGameClient").register(this.#connection, receiver);

		this.#connection.onreconnecting(() => {
			this.#lobby.connectionStatus = "reconnecting";
			this.#handleEvent({ type: "connection-interrupted" });
		});
		this.#connection.onreconnected(() => {
			this.#lobby.connectionStatus = "connected";
			this.#handleEvent({ type: "connection-restored" });
			void this.#resynchronize();
		});
		this.#connection.onclose(() => {
			if (!this.#intentionalStop && !this.#lobby.conclusion) {
				this.#handleEvent({
					type: "connection-lost",
					details: "The connection to the lobby could not be restored."
				});
				this.#lobby.terminateLocally(LobbyEndedReason.SERVER_ERROR);
			}
		});
	}

	public async start(): Promise<void> {
		if (this.#connection.state !== HubConnectionState.Disconnected) return;
		this.#lobby.connectionStatus = "connecting";

		try {
			await this.#connection.start();
			this.#lobby.connectionStatus = "connected";
			await this.#joinAndRead();
		} catch {
			const error = this.#transportError("The connection to the lobby could not be established.");
			this.#lobby.connectionStatus = "disconnected";
			this.#lobby.lastOperationError = error;
			this.#handleEvent({ type: "connection-lost", details: error.details });
			this.#lobby.terminateLocally(LobbyEndedReason.SERVER_ERROR);
		}
	}

	public async execute<T>(operationName: string, operation: (hub: IGameHub) => Promise<T>): Promise<T | undefined> {
		this.#lobby.lastOperationError = null;
		try {
			const result = (await operation(this.#hub)) as T & HubOperationResult;
			if (typeof result === "object" && result !== null && "success" in result && !(result as HubOperationResult).success) {
				this.#handleRejected((result as HubOperationResult).error);
				return undefined;
			}
			this.#handleEvent({ type: "operation-accepted", operationName });
			return result;
		} catch {
			this.#publishOperationError(this.#transportError("The lobby action could not be sent."));
			return undefined;
		}
	}

	public stop(): Promise<void> {
		if (this.#stopPromise) return this.#stopPromise;
		this.#intentionalStop = true;
		this.#receiverSubscription?.dispose();
		this.#receiverSubscription = null;
		this.#stopPromise =
			this.#connection.state === HubConnectionState.Disconnected ? Promise.resolve() : this.#connection.stop();
		return this.#stopPromise;
	}

	async #joinAndRead(): Promise<void> {
		const joined = await this.#hub.joinLobby({ code: this.#code });
		if (!joined.success) {
			this.#handleRejected(joined.error);
			return;
		}
		await this.execute("requestSnapshot", (hub) => hub.requestSnapshot());
		await this.execute("requestResults", (hub) => hub.requestResults({ scope: ResultsScope.CUMULATIVE }));
	}

	async #resynchronize(): Promise<void> {
		const joined = await this.#hub.joinLobby({ code: this.#code });
		if (!joined.success) {
			this.#handleRejected(joined.error);
			return;
		}
		await this.execute("requestSnapshot", (hub) => hub.requestSnapshot());
		const roundId =
			this.#results.visibleScope === ResultsScope.SPECIFIC_ROUND ? (this.#results.visibleRoundId ?? undefined) : undefined;
		await this.execute("requestResults", (hub) => hub.requestResults({ scope: this.#results.visibleScope, roundId }));
	}

	#handleRejected(error: SignalRError | undefined): void {
		const rejection = error ?? this.#transportError("The lobby action was rejected.");
		if (isLobbyConclusionErrorCode(rejection.code)) {
			this.#lobby.rejectLocally(rejection.code);
			void this.stop();
			return;
		}
		this.#publishOperationError(rejection);
	}

	#publishOperationError(error: SignalRError): void {
		this.#lobby.lastOperationError = error;
		this.#handleEvent({ type: "operation-failed", error });
	}

	#transportError(details: string): SignalRError {
		return {
			code: ErrorCode.INTERNAL_ERROR,
			retryable: false,
			details,
			correlationId: crypto.randomUUID(),
			traceId: ""
		};
	}
}
