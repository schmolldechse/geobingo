import { createServer } from "http";
import { Server } from 'socket.io';

import { PlayerSocket } from "./socket/playersocket";

import authHandler from "./handler/authhandler";
import lobbyHandler from "./handler/lobbyhandler";

const httpServer = createServer();
const io = new Server(httpServer, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"]
    }
});

const onConnection = (playerSocket: PlayerSocket) => {
    authHandler(playerSocket);
    lobbyHandler(playerSocket);

    console.log('Received connection');
}

io.on('connection', onConnection);

httpServer.listen(8000, () => {
    console.log('Server is running on port 8000');
});
