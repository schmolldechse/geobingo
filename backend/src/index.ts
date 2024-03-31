import { createServer } from "http";
import { Server } from 'socket.io';

const httpServer = createServer();
const io = new Server(httpServer, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"]
    }
});

io.on('connection', (data) => {
    console.log('Received connection');
});

httpServer.listen(8000, () => {
    console.log('Server is running on port 8000');
});
