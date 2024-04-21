import { Capture } from './capture';

export class Prompt {
    name: string;
    capture?: Capture;
    captures?: Capture[]; // the captures sent from the backend after ingame phase ended

    constructor(name: string, capture?: Capture) {
        this.name = name;
        this.capture = capture;
    }
}