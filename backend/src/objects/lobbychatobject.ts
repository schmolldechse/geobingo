import { Player } from "./player";

interface BasicLobbyChatObject {
    id: any;
    type: 'player' | 'system' | 'location';
    timestamp: number;
}

export interface PlayerChatObject extends BasicLobbyChatObject {
    type: 'player';
    player: Player;
    message: string;
}

export interface SystemChatObject extends BasicLobbyChatObject {
    type: 'system';
    message: string;
    data?: any;
}

export interface LocationChatObject extends BasicLobbyChatObject {
    type: 'location';
    pov: { heading: number, pitch: number, zoom: number };
    coordinates: { lat: number, lng: number };
}

export type LobbyChatObject = PlayerChatObject | SystemChatObject | LocationChatObject;