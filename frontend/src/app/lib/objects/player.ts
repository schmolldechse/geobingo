import { v4 as uuidv4 } from 'uuid';
import socket from "../server/socket";
import { faker } from '@faker-js/faker';

export class Player {
    name: string;
    id: string;
    picture: string;
    auth: any;
    points?: number;

    constructor(auth: any) {
        faker.seed(123); // ?????

        this.name = (auth ? auth.user_metadata.name : faker.person.firstName());
        this.id = (auth ? auth.id : uuidv4());
        this.picture = (auth ? auth.user_metadata.picture : '');
        this.auth = (auth ? auth : {});

        socket.emit('geobingo:initAuth', { player: this }, (response: any) => {  });
    }

    join = (lobbyCode: string, callback: (response: any) => void) => {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:joinLobby', { lobbyCode: lobbyCode }, (response: any) => {
            console.log('Response:', response);
            callback(response);
        });
    }

    host = (callback: (response: any) => void) => {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:createLobby', { privateLobby: true }, (response: any) => {
            console.log('Response:', response);
            callback(response);
        });
    }
    
    leave = (callback: (response: any) => void) => {
        if (!socket) throw new Error('Socket is not defined');
        socket.emit('geobingo:leaveLobby', { }, (response: any) => {
            console.log('Response:', response);
            callback(response);
        });
    }
}