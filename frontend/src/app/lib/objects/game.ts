import socket from "../server/socket";
import { Player } from "./player";
import { Prompt } from "./prompt";

const gameEvents = [
    'geobingo:lobbyUpdate'
];

export class Game {
    id!: string;
    players!: Player[];
    host!: Player;
    privateLobby!: boolean;
    phase!: 'waiting' | 'playing' | 'voting' | 'score';
    prompts!: Prompt[];
    maxSize!: number;
    time!: number;
    votingTime!: number;
    startingAt?: Date;
    endingAt?: Date;
    votingPlayers?: string[];

    constructor(data: any) {
        this.id = data.id;
        this.players = data.players;
        this.host = data.host;
        this.privateLobby = data.privateLobby;
        this.phase = data.phase;
        this.prompts = data.prompts.map((prompt: any) => new Prompt(prompt.name, prompt.capture));
        this.maxSize = data.maxSize;
        this.time = data.time;
        this.votingTime = data.votingTime;
        this.startingAt = data.startingAt;
        this.endingAt = data.endingAt;
    }

    removePrompt = (index: number) => {
        if (!socket) throw new Error("Game is not defined");
        socket.emit('geobingo:removePrompt', { index: index, lobbyCode: this.id }, (response: any) => {
            console.log('Response:', response);
        });
    };

    addPrompt = () => {
        if (!socket) throw new Error("Game is not defined");
        socket.emit('geobingo:addPrompt', { lobbyCode: this.id }, (response: any) => {
            console.log('Response:', response);
        });
    };

    changePrompt = (index: number, promptName: string) => {
        if (!socket) throw new Error("Game is not defined");
        socket.emit('geobingo:changePrompt', { lobbyCode: this.id, index: index, promptName: promptName }, (response: any) => {
            console.log('Response:', response);
        });
    }

    kickPlayer = (playerId: string) => {
        if (!socket) throw new Error("Game is not defined");
        socket.emit('geobingo:kickPlayer', { lobbyCode: this.id, playerId: playerId }, (response: any) => {
            console.log('Response:', response);
        });
    }

    makeHost = (playerId: string) => {
        if (!socket) throw new Error("Game is not defined");
        socket.emit('geobingo:makeHost', { lobbyCode: this.id, playerId: playerId }, (response: any) => {
            console.log('Response:', response);
        });
    }

    startGame = () => {
        if (!socket) throw new Error("Game is not defined");
        socket.emit('geobingo:startGame', { lobbyCode: this.id }, (response: any) => {
            console.log('Response:', response);
        });
    }

    uploadCaptures = (prompts: Prompt[]) => {
        if (!socket) throw new Error("Game is not defined");
        socket.emit('geobingo:uploadCaptures', { lobbyCode: this.id, prompts: prompts }, (response: any) => {
            console.log('Response:', response);
        });
    }

    handleVote = (prompt: string, captureId: string, points: number) => {
        if (!socket) throw new Error("Game is not defined");
        socket.emit('geobingo:handleVote', { lobbyCode: this.id, prompt: prompt, captureId: captureId, points: points }, (response: any) => {
            console.log('Response:', response);
        });
    }

    finishVote = () => {
        if (!socket) throw new Error("Game is not defined");
        socket.emit('geobingo:finishVote', { lobbyCode: this.id }, (response: any) => {
            console.log('Response:', response);
        });
    }

    /**
     * changing is a json object with the properties to change
     * @param changing 
     */
    editLobby = (changing: any, callback: (response: any) => void) => {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:editLobby', { lobbyCode: this.id, changing: changing }, (response: any) => {
            console.log('Response:', response);
            callback(response);
        });
    }

    stopSocket() {
        gameEvents.forEach(event => socket.off(event));
    }
}