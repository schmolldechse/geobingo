import { createContext } from "svelte";
import type { LobbyActions } from "./lobby-actions";
import type { LobbyState } from "./lobby-state.svelte";
import type { ResultState } from "./result-state.svelte";

export type LobbyContext = {
	lobby: LobbyState;
	results: ResultState;
	actions: LobbyActions;
};

const [getLobbyContext, setLobbyContext] = createContext<LobbyContext>();

export { getLobbyContext, setLobbyContext };
