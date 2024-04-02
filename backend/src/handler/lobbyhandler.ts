import { Player } from "../objects/player"; 
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

type Lobby = {
    id: string;
    players: Player[];
    host: Player;
    privateLobby: boolean;
}

let lobbies: Lobby[] = []; 

export default (playerSocket: PlayerSocket) => {
    const createLobby = (
        data: any,
        callback: Function
    ) => {
        const lobbyId = generateLobbyId();

        if (typeof data.privateLobby !== 'boolean') return callback({ error: true, message: 'Invalid parameters' });

        if (!playerSocket.player) return callback({ error: true, message: 'Not authenticated' });

        let lobby: Lobby = {
            id: lobbyId,
            players: [playerSocket.player],
            host: playerSocket.player,
            privateLobby: data.privateLobby
        }
        lobbies.push(lobby);

        console.log('Created lobby with id:', lobbyId);
        return callback({ error: false, data: { lobbyId: lobbyId } }); 
    }

    const joinLobby = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode.length === 0) return callback({ error: true, message: 'No lobby code given' });
        if (!playerSocket.player) return callback({ error: true, message: 'Not authenticated' });
        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ error: true, message: 'Lobby not found' });

        console.log('Joining lobby with id:', data.lobbyCode);
        return callback({ error: false, message: 'Joining lobby' });
    }

    createListener(playerSocket, 'geobingo', [createLobby, joinLobby]);
};