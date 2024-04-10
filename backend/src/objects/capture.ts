import { Player } from "./player";
import { Votes } from "./votes";

export class Capture {
    player: Player; // player who captured
    uniqueId: any; // unique id of the capture
    found: boolean;
    panorama: string;
    pov: { heading: number, pitch: number, zoom: number }
    coordinates: { lat: number, lng: number };
    votes?: Votes[];
}