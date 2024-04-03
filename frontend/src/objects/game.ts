import { WritableClass } from "../utils/writableclass";
import { Player } from "./player";
import socket from "../server/socket"; 

const gameEvents = [
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
}

export class Game extends WritableClass implements GameProps {
    id!: string;
    players!: Player[];
    host!: Player;
    privateLobby!: boolean;
    phase!: 'waiting' | 'playing' | 'score';
    prompts!: string[];

    constructor(props: GameProps) {
        super();
        Object.assign(this, props);
    }

    stopSocket() {
        gameEvents.forEach(event => socket.off(event));
    }
}