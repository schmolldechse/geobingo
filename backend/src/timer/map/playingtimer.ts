import { findLobby, updateLobby } from "../../objects/lobby";
import { Prompt } from "../../objects/prompt";
import { Timer } from "../timer";

export class PlayingTimer extends Timer {
    constructor(remainingTime: number, lobbyId: string, reverse: boolean) {
        super('playing', remainingTime, lobbyId, reverse);
    }

    onChanging(): void {
        const lobby = findLobby(this.lobbyId);
        if (!lobby) return this.stop();

        lobby.timers.playing = this.remainingTime;
        updateLobby(this.lobbyId, { timers: lobby.timers });
    }

    onStart(): void {  }

    onComplete(): void {
        const lobby = findLobby(this.lobbyId);
        if (!lobby) return this.stop();

        const totalCaptures = lobby.prompts.map((prompt: Prompt) => prompt.captures?.length || 0).reduce((a, b) => a + b, 0);
        lobby.phase = (totalCaptures > 0 ? 'voting' : 'score');

        if (lobby.phase === 'score') {
            updateLobby(this.lobbyId, { phase: lobby.phase });
            return;
        }

        lobby.timeManagement?.next();
        lobby.timeManagement?.current()?.start();
    }
}