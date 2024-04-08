import { Player } from "./player";

export const prompts = [
    'Lamborghini',
    'Porsche',
    'Ferrari',
    'Bicycle',
    'Pedestrian crossing',
    'Park',
    'Bus stop',
    'Restaurant',
    'Blue house',
    'Red house',
    'Bridge',
    'Autobahn',
    'Train station',
    'Airport',
    'Mailbox',
    'Stopsign',
    'Traffic light',
    'Fountain',
    'Statue',
    'Cat',
    'Library',
    'Supermarket',
    'Hospital',
    'School',
    'University',
    'Cinema',
    'Museum',
    'Police car',
    'Fire truck',
    'Taxi',
    'ATM machine',
    'Delivery truck',
    'Ambulance',
    'Tram',
    'ICE',
    'Street musician'
]

export class Prompt {
    name!: string;
    player?: Player;
    found?: boolean;
    coordinates?: { lat: number, lng: number };

    constructor(name: string) {
        this.name = name;
    }
}