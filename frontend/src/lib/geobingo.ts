import { fakerDE as faker } from '@faker-js/faker';
import { Player } from '../objects/player.ts';
import { WritableClass } from '../utils/writableclass.ts';
import socket from '../server/socket.ts';

export class GeoBingo extends WritableClass {
    player: Player;
    game?: undefined;
    guestName: string = faker.person.firstName();

    constructor(auth: any) {
        super();
        
        this.player = new Player({
            name: (auth ? auth.user_metadata.name : this.guestName),
            id: (auth ? auth.id : ''),
            picture: (auth ? auth.user_metadata.avatar_url : ''),
            auth: (auth ? auth : {})
        });

        socket.emit('geobingo:initAuth', { player: this.player }, (response: any) => {  });
    }

    refreshPlayer(auth: any) {
        this.player = new Player({
            name: (auth ? auth.user_metadata.name : this.guestName),
            id: (auth ? auth.id : ''),
            picture: (auth ? auth.user_metadata.avatar_url : ''),
            auth: (auth ? auth : {})
        });
        
        this.refresh();

        socket.emit('geobingo:initAuth', { player: this.player }, (response: any) => {  });
    }

}

let geoBingo: GeoBingo;

export function initializeGeoBingo(auth: any) {
    geoBingo = new GeoBingo(auth);
}

export function getGeoBingo() {
    return geoBingo || null;
}