import { Capture } from './capture';

export class Prompt {
    name: string;
    capture?: Capture;

    constructor(name: string, capture?: Capture) {
        this.name = name;
        this.capture = capture;
    }
}