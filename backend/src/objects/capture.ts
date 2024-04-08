export class Capture {
    playerId: string;
    found: boolean;
    panorama: string;
    pov: { heading: number, pitch: number, zoom: number }
    coordinates?: { lat: number, lng: number };
}