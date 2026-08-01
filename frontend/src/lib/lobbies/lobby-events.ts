import type { SignalRError } from "$lib/generated/realtime/GeoBingo.Contracts.SignalR";

export type LobbyEvent =
	| {
			type: "operation-accepted";
			operationName: string;
	  }
	| {
			type: "operation-failed";
			error: SignalRError;
	  }
	| {
			type: "connection-interrupted";
	  }
	| {
			type: "connection-restored";
	  }
	| {
			type: "connection-lost";
			details: string;
	  };

export type LobbyEventHandler = (event: LobbyEvent) => void;
