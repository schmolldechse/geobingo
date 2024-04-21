import { Player } from "./player";
import { Vote } from "./vote";

export class Capture {
    player: Player; // player who captured
    uniqueId: any; // unique id of the capture
    panorama: string;
    pov: { heading: number, pitch: number, zoom: number }
    coordinates: { lat: number, lng: number };
    votes?: Vote[];
}