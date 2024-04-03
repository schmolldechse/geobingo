import socket from "../server/socket";
import { getGeoBingo } from "$lib/geobingo";
import { Game } from "./game";

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
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:joinLobby', { lobbyCode: lobbyCode }, (response: any) => {
            console.log('Response:', response);
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
        socket.emit('geobingo:leaveLobby', {}, (response: any) => {
            console.log('Response:', response);
            if (!response.success) // toast

            getGeoBingo().game?.stopSocket();
            getGeoBingo().game = undefined;
            getGeoBingo().refresh();
        });
    }
}