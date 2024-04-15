import { Capture } from './capture';

export class Prompt {
    name: string;
    capture?: Capture;
    captures?: Capture[];

    constructor(name: string, capture?: Capture) {
        this.name = name;
        this.capture = capture;
    }
}