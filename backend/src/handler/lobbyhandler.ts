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
    players: PlayerSocket[];
    host: PlayerSocket;
    privateLobby: boolean;
    phase: 'waiting' | 'playing' | 'score';
    prompts: string[];
    maxSize: number;
    time: number;
}

export let lobbies: Lobby[] = []; 

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
            players: [playerSocket],
            host: playerSocket,
            privateLobby: data.privateLobby,
            phase: 'waiting',
            prompts: new Array(8).fill(null).map(() => getRandomPrompt()),
            maxSize: 10,
            time: 10
        }
        lobbies.push(lobby);

        console.log('Created lobby with id:', lobbyId);
        return callback({ success: true, game: createSendingLobby(lobby), message: 'Created lobby' }); 
    }

    const joinLobby = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });
        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });
        
        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        if (lobby.players.some(lobbyPlayer => lobbyPlayer.player?.id === playerSocket.player?.id)) return callback({ success: false, message: 'Already in lobby' });

        if (lobby.maxSize <= 0) return callback({ success: false, message: 'Lobbys max size has a invalid parameter' });
        if (lobby.players.length >= lobby.maxSize) return callback({ success: false, message: 'Lobby is full' });

        lobby.players.push(playerSocket);
        updateLobby(lobby);

        return callback({ success: true, game: createSendingLobby(lobby), message: 'Joining lobby' });
    }

    const leaveLobby = (
        data: any,
        callback: Function 
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });
        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.players.some(lobbyPlayer => { // lobbyPlayer is a PlayerSocket
            return lobbyPlayer.player?.id === playerSocket.player?.id;
        }));
        if (!lobby) return callback({ success: false, message: 'Not in a lobby' });

        lobby.players = lobby.players.filter(lobbyPlayer => { // lobbyPlayer is a PlayerSocket
            return lobbyPlayer.player?.id !== playerSocket.player?.id;
        });
        if (lobby.players.length === 0) {
            removeLobby(lobby);
        } else {
            let randomPlayer = lobby.players.find(player => player.id !== playerSocket.player?.id);
            lobby.host = randomPlayer || lobby.players[0];
            console.log('Lobbys host is now ' + lobby.host.player?.name);

            console.log('Updating lobby with id ' + lobby.id);
            updateLobby(lobby);
        }

        return callback({ success: true, message: 'Left lobby' });
    }

    const removePrompt = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });

        if (typeof data.index !== 'number') return callback({ success: false, message: 'No prompt given' });
        if (data.index < 0 || data.index >= prompts.length) return callback({ success: false, message: 'Invalid prompt' });
        
        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });
        
        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        if (lobby.host.player?.id !== playerSocket.player?.id) return callback({ success: false, message: 'Not the host' });

        if (!lobby.prompts[data.index]) return callback({ success: false, message: 'Prompt does not exist' });
        lobby.prompts.splice(data.index, 1);

        updateLobby(lobby);
        return callback({ success: true, message: 'Removed prompt' });
    }

    const addPrompt = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });
        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        if (lobby.host.player?.id !== playerSocket.player?.id) return callback({ success: false, message: 'Not the host' });
        lobby.prompts.push(getRandomPrompt());
        
        updateLobby(lobby);
        return callback({ success: true, message: 'Added prompt' });
    }

    const changePrompt = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });

        if (typeof data.index !== 'number') return callback({ success: false, message: 'No prompt given' });
        if (data.index < 0 || data.index >= prompts.length) return callback({ success: false, message: 'Invalid prompt' });

        if (typeof data.prompt !== 'string') return callback({ success: false, message: 'No prompt given' });

        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        if (lobby.host.player?.id !== playerSocket.player?.id) return callback({ success: false, message: 'Not the host' });

        if (!lobby.prompts[data.index]) return callback({ success: false, message: 'Prompt does not exist' });

        lobby.prompts[data.index] = data.prompt;

        updateLobby(lobby);
        return callback({ success: true, message: 'Changed prompt' });
    }

    createListener(playerSocket, 'geobingo', 
        [
            createLobby, 
            joinLobby,
            leaveLobby, 
            removePrompt, 
            addPrompt,
            changePrompt
        ]
    );
};

export const removeLobby = (lobby: Lobby) => {
    console.log('Deleting lobby with id ' + lobby.id + ' because no players are left');
    lobbies = lobbies.filter(object => object.id !== lobby.id);
}

/**
     * Sending an lobby update to all players in the lobby
     * @param lobby
     */
export const updateLobby = (lobby: Lobby) => {
    console.log('Sending update to all players in lobby with id ' + lobby.id);
    lobby.players.forEach(player => player.emit('geobingo:lobbyUpdate', { game: createSendingLobby(lobby) }));
}

/**
 * @param lobby 
 * @returns a lobby object without the playerSocket objects 
 */
const createSendingLobby = (lobby: Lobby) => {
    return { 
        ...lobby,
        players: lobby.players.map(playerSocket => playerSocket.player),
        host: lobby.host.player
    };
}