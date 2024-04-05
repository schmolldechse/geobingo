import { WritableClass } from "../utils/writableclass";
import { Player } from "./player";
import socket from "../server/socket"; 
import { getGeoBingo } from "$lib/geobingo";

const gameEvents = [
    'geobingo:lobbyUpdate',
    'game:changePlayerSize',
    'game:changeTime',
    'game:kickPlayer',
    'game:makeHost',
    'game:leaveGame'
    // weitere
]

interface GameProps {
    id: string;
    players: Player[];
    host: Player;
    privateLobby: boolean;
    phase: 'waiting' | 'playing' | 'score';
    prompts: string[];
    maxSize: number;
    time: number;
}

export class Game extends WritableClass implements GameProps {
    id!: string;
    players!: Player[];
    host!: Player;
    privateLobby!: boolean;
    phase!: 'waiting' | 'playing' | 'score';
    prompts!: string[];
    maxSize!: number;
    time!: number;

    constructor(props: GameProps) {
        super();
        Object.assign(this, props);

        socket.on('geobingo:lobbyUpdate', (response: any) => {
            const gameProps = JSON.parse(JSON.stringify(response.game));
            Object.assign(this, gameProps);

            getGeoBingo().refresh();
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