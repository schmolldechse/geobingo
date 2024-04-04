import { createServer } from "http";
import { Server } from 'socket.io';

import { PlayerSocket } from "./socket/playersocket";

import authHandler from "./handler/authhandler";
import lobbyHandler, { lobbies, removeLobby, updateLobby } from "./handler/lobbyhandler";

const httpServer = createServer();
const io = new Server(httpServer, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"]
    },
    pingInterval: 20000,
    pingTimeout: 5000,
    cookie: {
        name: 'geobingo-session',
        sameSite: 'strict',
    }
});

const onConnection = (playerSocket: PlayerSocket) => {
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
        updateLobby(lobby);
    });
}

io.on('connection', onConnection);

httpServer.listen(8000, () => {
    console.log('Server is running on port 8000');
});
