import { v4 as uuidv4 } from 'uuid';
import socket from "../server/socket";
import { faker } from '@faker-js/faker';
import path from 'path';

const guestPath = '/images/guest.png';

export class Player {
    guest: boolean;
    name: string;
    id: string;
    picture: string;
    points: number;

    constructor(guest: boolean, name: string, id: string, picture: string) {
        this.guest = guest;
        this.name = (guest ? faker.person.firstName() : name);
        this.id = (id ? id : uuidv4());
        this.picture = (picture ? picture : path.resolve(__dirname, guestPath));
        this.points = 0;

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