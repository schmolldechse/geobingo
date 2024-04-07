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
    }

    stopSocket() {
        gameEvents.forEach(event => socket.off(event));
    }
}