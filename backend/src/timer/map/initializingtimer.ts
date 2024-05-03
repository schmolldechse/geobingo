import { findLobby, updateLobby } from "../../objects/lobby";
import { Timer } from "../timer";

export class InitializingTimer extends Timer {
    constructor(remainingTime: number, lobbyId: string, reverse: boolean) {
        super('initializing', remainingTime, lobbyId, reverse);
    }

    onChanging(): void {
        const lobby = findLobby(this.lobbyId);
        if (!lobby) return this.stop();

        lobby.timers.initializing = this.remainingTime;
        updateLobby(this.lobbyId, { timers: lobby.timers });
    }

    onStart(): void {
        const lobby = findLobby(this.lobbyId);
        if (!lobby) return this.stop();

        lobby.phase = 'playing';
        updateLobby(this.lobbyId, { phase: lobby.phase, prompts: lobby.prompts, timers: lobby.timers });
    }

    onComplete(): void {
        const lobby = findLobby(this.lobbyId);
        if (!lobby) return this.stop();
        
        lobby.timeManagement?.next();
        lobby.timeManagement?.current().start();
    }
}