import { Player } from "./player";

export class Prompt {
    name: string;
    player: Player;
    found: boolean;
    coordinates: { lat: number, lng: number };

    constructor(name: string, player: Player, found: boolean, coordinates: { lat: number, lng: number }) {
        this.name = name;
        this.player = player;
        this.found = found;
        this.coordinates = coordinates;
    }
}