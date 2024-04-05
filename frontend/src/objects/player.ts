import socket from "../server/socket";
import { getGeoBingo } from "$lib/geobingo";
import { Game } from "./game";

interface PlayerProps {
    name: string;
    id: string;
    picture: string;
    points?: number;
    auth: any;
}

export class Player {
    name: string;
    id: string;
    picture: string;
    points?: number;
    auth: any;

    constructor({ name, id, picture, auth }: PlayerProps) {
        this.name = name;
        this.id = id;
        this.picture = picture;
        this.auth = auth;
    }

    join(lobbyCode: string) {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:joinLobby', { lobbyCode: lobbyCode }, (response: any) => {
            console.log('Response:', response);
            if (response.game) {
                getGeoBingo().game = new Game(response.game);
                getGeoBingo().refresh();
            }
        });
    }

    host() {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:createLobby', { privateLobby: true }, (response: any) => {
            console.log('Response:', response);
            if (response.game) {
                getGeoBingo().game = new Game(response.game);
                getGeoBingo().refresh();
            }
        });
    }

    leave() {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:leaveLobby', { }, (response: any) => {
            console.log('Response:', response);
            if (!response.success) return; // toast

            getGeoBingo().endGame();
            getGeoBingo().refresh();
        });
    }

    initMessageListener() {
        if (!socket) throw new Error('Socket is not defined');
        socket.on('geobingo:important', (response: any) => {
            if (response.kicked) {
                getGeoBingo().endGame();
                getGeoBingo().refresh();
            }
            
            console.log('Message:', response.message);
        });
    }
}