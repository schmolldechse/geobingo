//import { socket } from '../server/socket.ts';

interface PlayerProps {
    name: string;
    id: string;
    picture: string;
    auth: any;
}

export class Player {
    name: string;
    id: string;
    picture: string;
    auth: any;

    constructor({ name, id, picture, auth }: PlayerProps) {
        this.name = name;
        this.id = id;
        this.picture = picture;
        this.auth = auth;
    }

    join(lobbyCode: string) {
        /**
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:joinLobby', lobbyCode, (response: any) => {
            console.log('Response:', response);
        });
        */
    }

    host() {
        /**
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:createLobby', (response: any) => {
            console.log('Response:', response);
        });
        */
    }
}