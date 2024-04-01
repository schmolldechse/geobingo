import { fakerDE as faker } from '@faker-js/faker';
import { Player } from '../objects/player.ts';

export class GeoBingo {
    player: Player;
    game?: undefined;
    guestName: string = faker.person.firstName();

    constructor(auth: any) {
        this.player = new Player({
            name: (auth ? auth.user_metadata.name : this.guestName),
            id: (auth ? auth.id : ''),
            picture: (auth ? auth.user_metadata.avatar_url : ''),
            auth: (auth ? auth : {})
        });
    }

    refreshPlayer(auth: any) {
        this.player = new Player({
            name: (auth ? auth.user_metadata.name : this.guestName),
            id: (auth ? auth.id : ''),
            picture: (auth ? auth.user_metadata.avatar_url : ''),
            auth: (auth ? auth : {})
        });
    }

}

let geoBingo: GeoBingo;

export function initializeGeoBingo(auth: any) {
    geoBingo = new GeoBingo(auth);
}

export function getGeoBingo() {
    return geoBingo || null;
}