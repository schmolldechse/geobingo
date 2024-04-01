import { createServer } from "http";
import { Server, Socket } from 'socket.io';

const httpServer = createServer();
const io = new Server(httpServer, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"]
    }
});

const onConnection = (socket: Socket) => {
    console.log('Received connection');
}

io.on('connection', onConnection);

httpServer.listen(8000, () => {
    console.log('Server is running on port 8000');
});
