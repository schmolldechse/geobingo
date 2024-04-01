import { Socket } from "socket.io";

interface LobbyHandlerProps {
    socket: Socket;
}

export default ({ socket }: LobbyHandlerProps) => {
    socket.on('geobingo:createLobby', (callback: Function) => {
        console.log('Received createLobby');
        callback({ callback: true, message: 'Created lobby' });
    })

    socket.on('geobingo:joinLobby', (lobbyCode: string, callback: Function) => {
        // test
    });
};