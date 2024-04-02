import { io, Socket } from 'socket.io-client';

const domain = import.meta.env.VITE_DOMAIN || 'https://localhost:8000';

// listening to port 8000
const socket: Socket = io(domain, {
    transports: ['websocket'],
    closeOnBeforeunload: true,
});

export default socket;