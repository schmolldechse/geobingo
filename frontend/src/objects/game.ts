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
            console.log('Lobby update:', response.game);
        });
    }

    removePrompt(index: number) {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:removePrompt', { index: index }, (response: any) => {
            console.log('Response:', response);
        });
    }

    stopSocket() {
        gameEvents.forEach(event => socket.off(event));
    }
}