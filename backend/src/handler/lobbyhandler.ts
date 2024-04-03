import { Player } from "../objects/player"; 
import { prompts } from "../objects/prompts";
import { PlayerSocket, createListener } from "../socket/playersocket";

function generateLobbyId() {
    const length = 6;
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let id = '';

    for (let i = 0; i < length; i++) {
        const index = Math.floor(Math.random() * chars.length);
        id += chars[index];
    }

    return id;
};

function getRandomPrompt() {
    const randomIndex = Math.floor(Math.random() * prompts.length);
    return prompts[randomIndex];
}

type Lobby = {
    id: string;
    players: Player[];
    host: Player;
    privateLobby: boolean;
    phase: 'waiting' | 'playing' | 'score';
    prompts: string[];
}

let lobbies: Lobby[] = []; 

export default (playerSocket: PlayerSocket) => {
    const createLobby = (
        data: any,
        callback: Function
    ) => {
        const lobbyId = generateLobbyId();

        if (typeof data.privateLobby !== 'boolean') return callback({ success: false, message: 'Invalid parameters' });

        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        let lobby: Lobby = {
            id: lobbyId,
            players: [playerSocket.player],
            host: playerSocket.player,
            privateLobby: data.privateLobby,
            phase: 'waiting',
            prompts: new Array(8).fill(null).map(() => getRandomPrompt())
        }
        lobbies.push(lobby);

        console.log('Created lobby with id:', lobbyId);
        return callback({ success: true, game: lobby }); 
    }

    const joinLobby = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });
        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });
        
        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        return callback({ success: true, message: 'Joining lobby' });
    }

    const leaveLobby = (
        data: any,
        callback: Function 
    ) => {
        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.players.some(player => player.id === playerSocket.player?.id));
        if (!lobby) return callback({ success: false, message: 'Not in a lobby' });

        // update for each player in the lobby

        lobby.players = lobby.players.filter(player => player.id !== playerSocket.player?.id);
        if (lobby.players.length === 0) {
            console.log('Deleteting lobby with id ' + lobby.id + ' because no players are left');
            lobbies = lobbies.filter(l => l.id !== lobby.id);
        }

        return callback({ success: true, message: 'Left lobby' });
    }

    createListener(playerSocket, 'geobingo', [createLobby, joinLobby, leaveLobby]);
};