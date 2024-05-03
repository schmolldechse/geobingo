import { PlayerSocket, createListener } from "../socket/playersocket";
import { Prompt, getRandomPrompt, prompts } from "../objects/prompt";
import { Capture } from "../objects/capture";
import { v4 as uuidv4 } from 'uuid';
import { Vote } from "../objects/vote";
import { Lobby, createSendingLobby, generateLobbyId, lobbies, removeLobby, sendChatMessage } from "../objects/lobby";
import { CommandManager } from "../command/command";
import { SkipCommand } from "../command/map/skip";
import { TimeCommand } from "../command/map/time";
import { TimerManager } from "../timer/timer";
import { InitializingTimer } from "../timer/map/initializingtimer";
import { PlayingTimer } from "../timer/map/playingtimer";
import { VotingTimer } from "../timer/map/votingtimer";

const commandManager = new CommandManager();
commandManager.registerCommand('skip', new SkipCommand());
commandManager.registerCommand('time', new TimeCommand());

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
            phase: 'dashboard',
            prompts: new Array(8).fill(null).map(() => ({
                name: getRandomPrompt()
            })),
            maxSize: 10,
            timers: { initializing: 10, playing: 10 * 60, voting: 15 },
            chat: []
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

        if (lobby.phase !== 'dashboard') return callback({ success: false, message: 'Game already started' });

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

        if (lobby.phase !== 'dashboard') return callback({ success: false, message: 'Game already started' });

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

        if (data.changing.timers?.playing) {
            if (typeof data.changing.timers.playing !== 'number') return callback({ success: false, message: 'Invalid parameter' });
            if (data.changing.timers.playing < 1 || data.changing.timers.playing > 60) return callback({ success: false, message: `Parameter of 'timers.playing' is out of bounds` });

            lobby.timers.playing = data.changing.timers.playing * 60;
        }

        if (data.changing.timers?.voting) {
            if (typeof data.changing.timers.voting !== 'number') return callback({ success: false, message: 'Invalid parameter' });
            if (data.changing.timers.voting < 10 || data.changing.timers.voting > 60) return callback({ success: false, message: `Parameter of 'timers.voting' is out of bounds` });

            lobby.timers.voting = data.changing.timers.voting;
        }

        updateLobby(lobby, { maxSize: lobby.maxSize, timers: lobby.timers });
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

        if (lobby.phase !== 'dashboard') return callback({ success: false, message: 'Game already started' });

        let timeManagement = new TimerManager();
        timeManagement.map.push(new InitializingTimer(lobby.timers.initializing, lobby.id, false));
        timeManagement.map.push(new PlayingTimer(lobby.timers.playing, lobby.id, false));
        timeManagement.map.push(new VotingTimer(lobby.timers.voting, lobby.id, false));
        lobby.timeManagement = timeManagement;

        lobby.timeManagement.first();
        lobby.timeManagement.current().start();
        
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
            //capture.found = prompt.capture.found; unneccessary

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

    const resetLobby = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });

        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        if (lobby.host.player?.id !== playerSocket.player?.id) return callback({ success: false, message: 'Not the host' });

        lobby.phase = "dashboard";
        lobby.prompts = new Array(8).fill(null).map(() => ({
            name: getRandomPrompt()
        }));
        lobby.timeManagement = undefined;
        lobby.votedSkipTime = undefined;
        lobby.timers = { initializing: 10, playing: 10 * 60, voting: 15 };

        updateLobby(lobby, { phase: lobby.phase, prompts: lobby.prompts, timers: lobby.timers });

        return callback({ success: true, message: 'Lobby has been successfully reset' });
    }

    const sendMessage = (
        data: any,
        callback: Function
    ) => {
        if (data.lobbyCode?.length === 0) return callback({ success: false, message: 'No lobby code given' });

        if (!playerSocket.player) return callback({ success: false, message: 'Not authenticated' });

        const lobby = lobbies.find(lobby => lobby.id === data.lobbyCode);
        if (!lobby) return callback({ success: false, message: 'Lobby not found' });

        if (!data.lobbyChatObject) return callback({ success: false, message: 'No chat object given' });
        const lobbyChatObject = data.lobbyChatObject;
        lobbyChatObject.timestamp = Date.now();

        if (!lobbyChatObject.type) return callback({ success: false, message: 'Invalid chat object sent' });
        if (lobbyChatObject.type !== 'player') return callback({ success: false, message: 'Invalid chat object type' });

        const message = lobbyChatObject.message;
        if (!message || message.length <= 0) return callback({ success: false, message: 'No message sent' });
        
        if (message.startsWith('/')) {
            const [commandName, ...args] = message.slice(1).split(' ');
            return commandManager.executeCommand(commandName, args, playerSocket, lobby, callback);
        }
        
        lobby.chat.push(lobbyChatObject);
        sendChatMessage(lobby, lobbyChatObject);

        return callback({ success: true, message: 'Sent message' });
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
            resetLobby,
            sendMessage
        ]
    );
};

/**
 * Sending an lobby update to all players in the lobby with the properties given in "update"
 * @param lobby
 * @param update
 */
export const updateLobby = (lobby: Lobby, update: any) => {
    console.log('Sending update in lobby with id ' + lobby.id);
    lobby.players.forEach(player => player.emit('geobingo:lobbyUpdate', update));
}