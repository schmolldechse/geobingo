import { fakerDE as faker } from '@faker-js/faker';
import { Player } from '../objects/player.ts';
import { Game } from '../objects/game.ts';
import { WritableClass } from '../utils/writableclass.ts';
import socket from '../server/socket.ts';
import { v4 as uuidv4 } from 'uuid';

export class GeoBingo extends WritableClass {
    player: Player;
    guestName: string = faker.person.firstName();
    game?: Game | undefined;

    constructor(auth: any) {
        super();
        
        this.player = new Player({
            name: (auth ? auth.user_metadata.name : this.guestName),
            id: (auth ? auth.id : uuidv4()),
            picture: (auth ? auth.user_metadata.avatar_url : ''),
            auth: (auth ? auth : {})
        });

        socket.emit('geobingo:initAuth', { player: this.player }, (response: any) => {  });
    }

    refreshPlayer(auth: any) {
        this.player = new Player({
            name: (auth ? auth.user_metadata.name : this.guestName),
            id: (auth ? auth.id : uuidv4()),
            picture: (auth ? auth.user_metadata.avatar_url : ''),
            auth: (auth ? auth : {})
        });
        
        this.refresh();

        socket.emit('geobingo:initAuth', { player: this.player }, (response: any) => {  });
    }

    endGame() {
        if (!this.game) return; // toast
        this.game.stopSocket();
        this.game = undefined; 
    }

}

let geoBingo: GeoBingo;

export function initializeGeoBingo(auth: any) {
    geoBingo = new GeoBingo(auth);
}

export function getGeoBingo() {
    return geoBingo || null;
}