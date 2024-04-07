import socket from "../server/socket";
import { Player } from "./player";

const gameEvents = [
    'geobingo:lobbyUpdate'
];

interface GameProps {
    id: string;
    players: Player[];
    host: Player;
    privateLobby: boolean;
    phase: 'waiting' | 'playing' | 'score';
    prompts: string[];
    maxSize: number;
    time: number;
    startingAt?: Date;
    endingAt?: Date; 
}

export class Game implements GameProps {
    id!: string;
    players!: Player[];
    host!: Player;
    privateLobby!: boolean;
    phase!: 'waiting' | 'playing' | 'score';
    prompts!: string[];
    maxSize!: number;
    time!: number;
    startingAt?: Date;
    endingAt?: Date;

    constructor(props: GameProps) {
        Object.assign(this, props);

        socket.on('geobingo:lobbyUpdate', (response: any) => {
            const gameProps = JSON.parse(JSON.stringify(response.game));
            Object.assign(this, gameProps);
        });
    }

    removePrompt(index: number) {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:removePrompt', { index: index, lobbyCode: this.id }, (response: any) => {
            console.log('Response:', response);
        });
    }

    addPrompt() {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:addPrompt', { lobbyCode: this.id }, (response: any) => {
            console.log('Response:', response);
        });
    }

    changePrompt(index: number, prompt: string) {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:changePrompt', { lobbyCode: this.id, index: index, prompt: prompt }, (response: any) => {
            console.log('Response:', response);
        });
    }

    kickPlayer(id: string) {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:kickPlayer', { lobbyCode: this.id, playerId: id }, (response: any) => {
            console.log('Response:', response);
        });
    }

    makeHost(id: string) {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:makeHost', { lobbyCode: this.id, playerId: id }, (response: any) => {
            console.log('Response:', response);
        });
    }

    startGame() {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:startGame', { lobbyCode: this.id }, (response: any) => {
            console.log('Response:', response);
            if (!response.succes) return; // TODO: toast


        });
    }

    /**
     * changing is a json object with the properties to change
     * @param changing 
     */
    editLobby(changing: any) {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:editLobby', { lobbyCode: this.id, changing: changing }, (response: any) => {
            console.log('Response:', response);
        });
    }

    stopSocket() {
        gameEvents.forEach(event => socket.off(event));
    }
}