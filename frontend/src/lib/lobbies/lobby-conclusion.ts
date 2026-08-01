import { ErrorCode } from "$lib/generated/realtime/GeoBingo.Contracts.Common";
import { LobbyEndedReason } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";

export type LobbyConclusionErrorCode =
	| ErrorCode.LOBBY_NOT_FOUND
	| ErrorCode.LOBBY_CLOSED
	| ErrorCode.LOBBY_FULL
	| ErrorCode.LOBBY_NOT_JOINABLE
	| ErrorCode.LOBBY_BANNED
	| ErrorCode.ALREADY_IN_ANOTHER_LOBBY
	| ErrorCode.SERVER_SHUTTING_DOWN;

export type LobbyConclusionSource =
	| {
			type: "ended";
			reason: LobbyEndedReason;
	  }
	| {
			type: "rejected";
			code: LobbyConclusionErrorCode;
	  };

export type LobbyConclusionPresentation =
	| {
			kind: "screen";
			status: number;
			message: string;
	  }
	| {
			kind: "redirect";
			href: "/";
	  };

const endedPresentations: Record<LobbyEndedReason, LobbyConclusionPresentation> = {
	[LobbyEndedReason.CLOSED_BY_HOST]: {
		kind: "screen",
		status: 410,
		message: "This lobby has been closed."
	},
	[LobbyEndedReason.NO_MEMBERS]: {
		kind: "screen",
		status: 410,
		message: "This lobby is no longer available."
	},
	[LobbyEndedReason.KICKED]: {
		kind: "screen",
		status: 403,
		message: "You were removed from this lobby."
	},
	[LobbyEndedReason.BANNED]: {
		kind: "screen",
		status: 403,
		message: "You can’t rejoin this lobby."
	},
	[LobbyEndedReason.LEFT]: {
		kind: "redirect",
		href: "/"
	},
	[LobbyEndedReason.SERVER_SHUTDOWN]: {
		kind: "screen",
		status: 503,
		message: "This lobby ended when the server went offline."
	},
	[LobbyEndedReason.SERVER_ERROR]: {
		kind: "screen",
		status: 503,
		message: "The lobby connection could not be restored."
	}
};

const rejectedPresentations: Record<LobbyConclusionErrorCode, LobbyConclusionPresentation> = {
	[ErrorCode.LOBBY_NOT_FOUND]: {
		kind: "screen",
		status: 404,
		message: "This lobby could not be found."
	},
	[ErrorCode.LOBBY_CLOSED]: {
		kind: "screen",
		status: 410,
		message: "This lobby has been closed."
	},
	[ErrorCode.LOBBY_FULL]: {
		kind: "screen",
		status: 409,
		message: "This lobby is full."
	},
	[ErrorCode.LOBBY_NOT_JOINABLE]: {
		kind: "screen",
		status: 409,
		message: "This lobby is not accepting new players."
	},
	[ErrorCode.LOBBY_BANNED]: {
		kind: "screen",
		status: 403,
		message: "You can’t rejoin this lobby."
	},
	[ErrorCode.ALREADY_IN_ANOTHER_LOBBY]: {
		kind: "screen",
		status: 409,
		message: "You are already connected to another lobby."
	},
	[ErrorCode.SERVER_SHUTTING_DOWN]: {
		kind: "screen",
		status: 503,
		message: "This lobby ended when the server went offline."
	}
};

export const isLobbyConclusionErrorCode = (code: ErrorCode): code is LobbyConclusionErrorCode =>
	code === ErrorCode.LOBBY_NOT_FOUND ||
	code === ErrorCode.LOBBY_CLOSED ||
	code === ErrorCode.LOBBY_FULL ||
	code === ErrorCode.LOBBY_NOT_JOINABLE ||
	code === ErrorCode.LOBBY_BANNED ||
	code === ErrorCode.ALREADY_IN_ANOTHER_LOBBY ||
	code === ErrorCode.SERVER_SHUTTING_DOWN;

export const getLobbyConclusionPresentation = (source: LobbyConclusionSource): LobbyConclusionPresentation =>
	source.type === "ended" ? endedPresentations[source.reason] : rejectedPresentations[source.code];
