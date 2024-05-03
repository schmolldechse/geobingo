import { Capture } from "../../objects/capture";
import { findLobby, updateLobby } from "../../objects/lobby";
import { Prompt } from "../../objects/prompt";
import { Vote } from "../../objects/vote";
import { Timer } from "../timer";

export class VotingTimer extends Timer {

    private initializedSeconds: number = 0;

    private captures: { prompt: string, capture: any }[] = [];
    private currentCapture: any = null;
    private currentIndex: number = 0;

    constructor(remainingTime: number, lobbyId: string, reverse: boolean) {
        super('voting', remainingTime, lobbyId, reverse);
        this.initializedSeconds = remainingTime;
    }

    onChanging(): void {
        const lobby = findLobby(this.lobbyId);
        if (!lobby) return this.stop();

        lobby.timers.voting = this.remainingTime;
        updateLobby(this.lobbyId, { timers: lobby.timers });

        if (this.remainingTime <= 0) {
            this.currentIndex++;

            if (this.currentIndex < this.captures.length) {
                this.remainingTime = this.initializedSeconds + 1;
                this.currentCapture = this.captures[this.currentIndex];

                updateLobby(this.lobbyId, { currentCapture: this.currentCapture });
            }
        }
    }

    onStart(): void {
        const lobby = findLobby(this.lobbyId);
        if (!lobby) return this.stop();

        this.captures = lobby.prompts
            .filter((prompt: Prompt) => prompt.captures)
            .flatMap((prompt: Prompt) => prompt.captures.map((capture: any) => ({ prompt: prompt.name, capture })));
        this.currentCapture = this.captures[this.currentIndex];

        lobby.timers.voting = this.initializedSeconds;
        updateLobby(this.lobbyId, { phase: lobby.phase, currentCapture: this.currentCapture });
    }

    onComplete(): void {
        const lobby = findLobby(this.lobbyId);
        if (!lobby) return this.stop();

        lobby.prompts.forEach((prompt: Prompt) => {
            prompt.captures?.forEach((capture: Capture) => {
                if (!capture.votes) return;
                const points = capture.votes?.reduce((total: number, vote: Vote) => total + vote.points, 0) || 0;

                const lobbyPlayer = lobby.players.find(playerSocket => playerSocket.player.id === capture.player.id);
                if (!lobbyPlayer) return;

                lobbyPlayer.player.points += points;
            })
        });

        updateLobby(this.lobbyId, { phase: 'score', players: lobby.players.map(playerSocket => playerSocket.player), prompts: lobby.prompts, currentCapture: undefined });
    }
}