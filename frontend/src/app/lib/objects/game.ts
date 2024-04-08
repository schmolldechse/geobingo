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
    phase!: 'waiting' | 'playing' | 'score';
    prompts!: Prompt[];
    maxSize!: number;
    time!: number;
    startingAt?: Date;
    endingAt?: Date;

    constructor(data: any, player: Player) {
        this.id = data.id;
        this.players = data.players;
        this.host = data.host;
        this.privateLobby = data.privateLobby;
        this.phase = data.phase;
        this.prompts = data.prompts.map(prompt => new Prompt(
            prompt.name,
            player,
            false,
            { lat: 0, lng: 0 }
        ));
        this.maxSize = data.maxSize;
        this.time = data.time;
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

    changePrompt = (index: number, prompt: any) => {
        if (!socket) throw new Error("Game is not defined");
        socket.emit('geobingo:changePrompt', { lobbyCode: this.id, index: index, prompt: prompt }, (response: any) => {
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

    /**
     * changing is a json object with the properties to change
     * @param changing 
     */
    editLobby = (changing: any) => {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:editLobby', { lobbyCode: this.id, changing: changing }, (response: any) => {
            console.log('Response:', response);
        });
    }

    stopSocket() {
        gameEvents.forEach(event => socket.off(event));
    }
}