import { PlayerSocket, createListener } from "../socket/playersocket";
import { format } from 'date-fns-tz';
import { add } from 'date-fns';
import { Prompt, prompts } from "../objects/prompt";
import { Capture } from "../objects/capture";
import { v4 as uuidv4 } from 'uuid';
import { Vote } from "../objects/vote";

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
    phase: 'waiting' | 'playing' | 'voting' | 'score';
    prompts: Prompt[];
    maxSize: number;
    time: number;
    votingTime: number; // per prompt
    startingAt?: string;
    endingAt?: string;
    votingPlayers?: string[];
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
            prompts: new Array(8).fill(null).map(() => ({
                name: getRandomPrompt()
            })),
            maxSize: 10,
            time: 10,
            votingTime: 15,
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

        if (lobby.phase !== 'waiting') return callback({ success: false, message: 'Game already started' });

        lobby.players.push(playerSocket);

        updateLobby(lobby, { players: lobby.players.map(playerSocket => playerSocket.player) });
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
        } else if (lobby.host.player?.id === playerSocket.player?.id) {
            let randomPlayer = lobby.players.find(lobbyPlayer => lobbyPlayer.player?.id !== playerSocket.player?.id);
            lobby.host = randomPlayer || lobby.players[0];
            lobby.host.emit('geobingo:important', { message: 'You are the new host now' });

            console.log('New host of lobby ' + lobby.id + ' is now ' + lobby.host.player?.name);
            updateLobby(lobby, { players: lobby.players.map(playerSocket => playerSocket.player), host: lobby.host.player })
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

        updateLobby(lobby, { prompts: lobby.prompts });
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
        lobby.prompts.push(new Prompt(getRandomPrompt()));

        updateLobby(lobby, { prompts: lobby.prompts });
        return callback({ success: true, message: 'Added prompt' });
    }

    const changePrompt = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });

        if (typeof data.index !== 'number') return callback({ success: false, message: 'No prompt index given' });
        if (data.index < 0 || data.index >= prompts.length) return callback({ success: false, message: 'Invalid prompt' });

        if (typeof data.promptName !== 'string') return callback({ success: false, message: 'No prompt given' });

        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        if (lobby.host.player?.id !== playerSocket.player?.id) return callback({ success: false, message: 'Not the host' });

        if (!lobby.prompts[data.index]) return callback({ success: false, message: 'Prompt does not exist' });

        if (lobby.phase !== 'waiting') return callback({ success: false, message: 'Game already started' });

        lobby.prompts[data.index].name = data.promptName;

        updateLobby(lobby, { prompts: lobby.prompts });
        return callback({ success: true, message: 'Changed prompt' });
    }

    const editLobby = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });

        if (!data.changing) return callback({ success: false, message: 'No changes given' });

        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        if (lobby.host.player?.id !== playerSocket.player?.id) return callback({ success: false, message: 'Not the host' });

        if (data.changing.maxSize) {
            if (typeof data.changing.maxSize !== 'number') return callback({ success: false, message: 'Invalid parameter' });
            if (data.changing.maxSize < 1 || data.changing.maxSize > 100) return callback({ success: false, message: `Parameter of 'maxSize' is out of bounds` });

            if (data.changing.maxSize < lobby.players.length) return callback({ success: false, message: `Parameter of 'maxSize' can not be smaller than the amount of players` });

            lobby.maxSize = data.changing.maxSize;
        }

        if (data.changing.time) {
            if (typeof data.changing.time !== 'number') return callback({ success: false, message: 'Invalid parameter' });
            if (data.changing.time < 1 || data.changing.time > 60) return callback({ success: false, message: `Parameter of 'time' is out of bounds` });

            lobby.time = data.changing.time;
        }

        if (data.changing.votingTime) {
            if (typeof data.changing.votingTime !== 'number') return callback({ success: false, message: 'Invalid parameter' });
            if (data.changing.votingTime < 10 || data.changing.votingTime > 60) return callback({ success: false, message: `Parameter of 'votingTime' is out of bounds` });

            lobby.votingTime = data.changing.votingTime;
        }

        updateLobby(lobby, { maxSize: lobby.maxSize, time: lobby.time, votingTime: lobby.votingTime });
        return callback({ success: true, message: 'Updated lobby' });
    }

    const kickPlayer = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });

        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        if (lobby.host.player?.id !== playerSocket.player?.id) return callback({ success: false, message: 'Not the host' });
        const player = lobby.players.find(lobbyPlayer => lobbyPlayer.player?.id === data.playerId);
        if (!player) return callback({ success: false, message: 'Player not in lobby' });

        player.emit('geobingo:important', { kicked: true, message: 'You were kicked' });

        lobby.players = lobby.players.filter(lobbyPlayer => lobbyPlayer.player?.id !== data.playerId);

        updateLobby(lobby, { players: lobby.players.map(playerSocket => playerSocket.player) });
        return callback({ success: true, message: 'Kicked player' });
    }

    const makeHost = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });

        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        if (lobby.host.player?.id !== playerSocket.player?.id) return callback({ success: false, message: 'Not the host' });
        const newHost = lobby.players.find(lobbyPlayer => lobbyPlayer.player?.id === data.playerId);
        if (!newHost) return callback({ success: false, message: 'Player not in lobby' });

        newHost.emit('geobingo:important', { message: 'You are the new host now' });

        lobby.host = newHost;

        updateLobby(lobby, { host: lobby.host.player });
        console.log('Lobbys host is now ' + lobby.host.player?.name);
        return callback({ success: true, message: 'Changed host role' })
    }

    const startGame = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });

        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        if (lobby.host.player?.id !== playerSocket.player?.id) return callback({ success: false, message: 'Not the host' });

        if (lobby.phase !== 'waiting') return callback({ success: false, message: 'Game already started' });

        let startingAt = new Date();
        startingAt.setSeconds(startingAt.getSeconds() + 15); // +15s for preparation

        lobby.startingAt = format(startingAt, 'yyyy-MM-dd HH:mm:ss', { timeZone: 'Europe/Berlin' });
        lobby.endingAt = format(add(startingAt, { minutes: lobby.time }), 'yyy-MM-dd HH:mm:ss', { timeZone: 'Europe/Berlin' });

        lobby.phase = 'playing';

        startLobbyTimer(lobby);
        updateLobby(lobby, { startingAt: lobby.startingAt, endingAt: lobby.endingAt, phase: lobby.phase });
        return callback({ success: true, message: 'Started game' });
    }

    /**
     * retrieves the prompts that the player captured and uploads them to the server
     * @param data prompts array
     * @param callback 
     */
    const uploadCaptures = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });

        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        if (lobby.phase !== 'playing') return callback({ success: false, message: 'Game did not start yet' });

        // reset the captures found by "playerSocket.player"
        lobby.prompts.forEach((prompt: Prompt) => {
            if (!prompt.captures) return;
            prompt.captures = prompt.captures.filter((capture: Capture) => capture.player !== playerSocket.player)
        });

        if (data.prompts.length === 0) return callback({ success: false, message: 'No prompts given to upload' });

        // fetching prompts that the player captured
        data.prompts.forEach((prompt: any) => {
            if (!prompt.capture) return;

            // find prompt object from "lobby.prompts" by incoming prompt data with property "prompt.name"
            const lobbyPrompt = lobby.prompts.find((lobbyPrompt: Prompt) => lobbyPrompt.name === prompt.name);
            if (!lobbyPrompt) return;
            if (!lobbyPrompt.captures) lobbyPrompt.captures = [];

            // create a new capture object | there won't be any votes yet
            const capture = new Capture();
            capture.player = playerSocket.player;
            capture.uniqueId = uuidv4();
            capture.found = prompt.capture.found;

            capture.panorama = prompt.capture.panorama;
            capture.pov = prompt.capture.pov;
            capture.coordinates = prompt.capture.coordinates;

            // add the capture object to the (backend-)prompt object
            lobbyPrompt.captures.push(capture);
        });

        return callback({ success: true, message: 'Uploaded captures' });
    }

    const handleVote = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });

        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        if (lobby.phase !== 'voting') return callback({ success: false, message: 'Voting phase did not start yet' });

        const prompt = lobby.prompts.find((prompt: Prompt) => prompt.name === data.prompt);
        if (!prompt) return callback({ success: false, message: 'Prompt not found' });

        const capture = prompt.captures?.find((capture: Capture) => capture.uniqueId === data.captureId);
        if (!capture) return callback({ success: false, message: 'Capture not found' });
        if (!capture.votes) capture.votes = [];

        // reset vote from player, if he's changing mind
        capture.votes = capture.votes.filter((vote: Vote) => vote.votedPlayerId !== playerSocket.player.id);
        capture.votes.push({ votedPlayerId: playerSocket.player.id, points: data.points });

        return callback({ success: true, message: 'Voted' });
    }

    /**
     * gets called if a players finishes his votings
     */
    const finishVote = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });

        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        const voting = lobby.votingPlayers.find(playerId => playerId === playerSocket.player.id);
        if (!voting) return callback({ success: false, message: 'Already finished voting' });

        // update the amount of players who are still voting
        const votingPlayers = lobby.votingPlayers.filter(playerId => playerId !== playerSocket.player.id);
        lobby.votingPlayers = votingPlayers;

        updateLobby(lobby, { votingPlayers: votingPlayers });
        if (votingPlayers.length === 0) {
            // collect points of each capture and give them to their creator
            lobby.prompts.forEach((prompt: Prompt) => {
                prompt.captures?.forEach((capture: Capture) => {
                    if (!capture.votes) return;
                    const points = capture.votes?.reduce((total: number, vote: Vote) => total + vote.points, 0) || 0;

                    const lobbyPlayer = lobby.players.find(playerSocket => playerSocket.player.id === capture.player.id);
                    if (!lobbyPlayer) {
                        console.log('Could not find player from capture ' + capture.uniqueId);
                        return;
                    }

                    lobbyPlayer.player.points += points;
                })
            });

            updateLobby(lobby, { phase: 'score', players: lobby.players.map(playerSocket => playerSocket.player) });
        }

        return callback({ success: true, message: 'Finished voting' });
    }

    createListener(playerSocket, 'geobingo',
        [
            createLobby,
            joinLobby,
            leaveLobby,
            removePrompt,
            addPrompt,
            changePrompt,
            editLobby,
            kickPlayer,
            makeHost,
            startGame,
            uploadCaptures,
            handleVote,
            finishVote
        ]
    );
};

export const removeLobby = (lobby: Lobby) => {
    console.log('Deleting lobby with id ' + lobby.id + ' because no players are left');
    lobbies = lobbies.filter(object => object.id !== lobby.id);
}

/**
 * Sending an lobby update to all players in the lobby with the properties given in "update"
 * @param lobby
 */
export const updateLobby = (lobby: Lobby, update: any) => {
    console.log('Sending update in lobby with id ' + lobby.id);
    lobby.players.forEach(player => player.emit('geobingo:lobbyUpdate', update));
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

const startLobbyTimer = (lobby: Lobby) => {
    console.log('Lobby with id ' + lobby.id + ' started');

    let intervalId = setInterval(() => {
        const current = new Date();

        if (lobby.phase !== 'playing') clearInterval(intervalId);

        if (current.getTime() >= new Date(lobby.endingAt).getTime()) {
            const totalCaptures = lobby.prompts.map((prompt: Prompt) => prompt.captures?.length || 0).reduce((a, b) => a + b, 0);

            lobby.phase = (totalCaptures > 1 ? 'voting' : 'score');
            lobby.startingAt = undefined;
            lobby.endingAt = undefined;

            // add player ids to votingPlayers
            lobby.votingPlayers = lobby.players.map(playerSocket => playerSocket.player.id);

            updateLobby(lobby, { phase: lobby.phase, prompts: lobby.prompts, startingAt: lobby.startingAt, endingAt: lobby.endingAt, votingPlayers: lobby.votingPlayers });

            clearInterval(intervalId);

            console.log(`Lobby with id ' + lobby.id + ' ended. Switching to ${lobby.phase} phase`);
        }
    }, 1000);
}