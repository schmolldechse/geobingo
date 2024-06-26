import { createServer } from "http";
import { Server, Socket } from 'socket.io';

import { PlayerSocket } from "./socket/playersocket";

import authHandler from "./handler/authhandler";
import lobbyHandler, { updateLobby } from "./handler/lobbyhandler";
import { lobbies, removeLobby } from "./objects/lobby";

import dotenv from 'dotenv';
dotenv.config();

const allowedOrigins = process.env.ALLOWED_ORIGINS?.split(',') || "*";
const port = process.env.PORT || 4010;

const httpServer = createServer();
const io = new Server(httpServer, {
    cors: {
        origin: allowedOrigins,
        methods: ["GET", "POST"]
    },
    pingInterval: 20000,
    pingTimeout: 5000,
    cookie: {
        name: 'geobingo-session',
        sameSite: 'strict',
    }
});

const onConnection = (socket: Socket) => {
    const playerSocket = new PlayerSocket(socket);

    authHandler(playerSocket);
    lobbyHandler(playerSocket);

    console.log('Received connection');

    playerSocket.on('disconnect', (reason) => {
        console.log(`Player ${playerSocket.player?.id} disconnected:`, reason);

        const lobby = lobbies.find(lobby => lobby.players.includes(playerSocket));
        if (!lobby) return;

        lobby.players = lobby.players.filter(lobbyPlayer => lobbyPlayer !== playerSocket);
        if (lobby.players.length === 0) {
            removeLobby(lobby);
            return;
        }
        
        updateLobby(lobby, { players: lobby.players.map(playerSocket => playerSocket.player) });
        if (lobby.host.player?.id !== playerSocket.player?.id) return;

        let randomPlayer = lobby.players.find(lobbyPlayer => lobbyPlayer.player?.id !== playerSocket.player?.id);
        lobby.host = randomPlayer || lobby.players[0];
        lobby.host.emit('geobingo:important', { message: 'You are the new host now' });

        console.log('New host of lobby ' + lobby.id + ' is now ' + lobby.host.player?.name);
        updateLobby(lobby, { host: lobby.host.player })
    });
}

io.on('connection', onConnection);

httpServer.listen(port, () => {
    console.log(`Server is running on port ${port}`);
});