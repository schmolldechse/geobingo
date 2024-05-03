import { PlayerSocket } from "../socket/playersocket";
import { TimerManager } from "../timer/timer";
import { LobbyChatObject } from "./lobbychatobject";
import { Prompt } from "./prompt";

export type Lobby = {
    id: string;
    players: PlayerSocket[];
    host: PlayerSocket;
    privateLobby: boolean;
    phase: 'dashboard' | 'playing' | 'voting' | 'score';
    prompts: Prompt[];
    maxSize: number;
    timeManagement?: TimerManager;
    timers: { initializing: number, playing: number, voting: number }; // in seconds
    chat: LobbyChatObject[];
    votedSkipTime?: string[]; // playerIds
}

export let lobbies: Lobby[] = [];

export const findLobby = (lobbyId: string): Lobby | undefined => {
    return lobbies.find(lobby => lobby.id === lobbyId);
}

/**
 * Removes the lobby from the lobbies array
 * Cleares the timeManagement object if it exists
 * @param lobby The lobby to be removed.
 */
export const removeLobby = (lobby: Lobby) => {
    console.log('Deleting lobby with id ' + lobby.id + ' because no players are left');
    lobbies = lobbies.filter(lobbyObject => {
        if (lobbyObject.id === lobby.id && lobbyObject.timeManagement) clearInterval(lobbyObject.timeManagement.current().intervalId!);
        return lobbyObject.id !== lobby.id;
    });
}

/**
 * Creates a copy of the lobby object which will be used to send to the client
 * @param lobby The lobby to be copied
 * @returns A copy of the lobby object without the playerSocket and timeManagement properties
 */
export const createSendingLobby = (lobby: Lobby) => {
    const { timeManagement, votedSkipTime, ...copy } = lobby;

    return {
        ...copy,
        players: lobby.players.map(playerSocket => playerSocket.player),
        host: lobby.host.player
    };
}

/**
 * Updates the lobby with the given id and sends an update to all players in the lobby
 * @param lobbyId The id of the lobby to be updated
 * @param update The update object to be sent to the players
 */
export const updateLobby = (lobbyId: string, update: any) => {
    const lobby = findLobby(lobbyId);
    if (!lobby) return;

    console.log('Updating lobby with id ' + lobbyId);
    lobby.players.forEach(player => player.emit('geobingo:lobbyUpdate', update));
}

/**
 * Sends a chat message to all players in the lobby. 
 * @param lobby The lobby where the message should be sent
 * @param lobbyChatObject The chat message to be sent
 */
export const sendChatMessage = (lobby: Lobby, lobbyChatObject: LobbyChatObject) => {
    lobby.players.forEach(player => player.emit('geobingo:lobbyChatMessage', lobbyChatObject));
}

/**
 * @returns A unique lobby id
 */
export const generateLobbyId = () => {
    const length = 6;
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let id = '';

    for (let i = 0; i < length; i++) {
        const index = Math.floor(Math.random() * chars.length);
        id += chars[index];
    }

    return id;
};