import { io, Socket } from 'socket.io-client';

const domain = process.env.NEXT_PUBLIC_DOMAIN || 'localhost:8000';

const socket: Socket = io(domain, { // listening to port 8000
    transports: ['websocket'],
    closeOnBeforeunload: false,
});

export default socket;