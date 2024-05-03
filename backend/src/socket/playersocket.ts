import { Socket } from "socket.io";
import { Player } from "../objects/player";
import { LobbyChatObject } from "../objects/lobbychatobject";

export class PlayerSocket {
    socket: Socket;
    player?: Player;

    constructor(socket: Socket) {
        this.socket = socket;
    }

    /**
     * Sending a chat message to a specific player in the lobby
     * @param player
     * @param lobbyChatObject
     */
    sendChatMessage(lobbyChatObject: LobbyChatObject) {
        this.socket.emit('geobingo:lobbyChatMessage', lobbyChatObject);
    }

    on(event: string, listener: (...args: any[]) => void) {
        this.socket.on(event, listener);
    }

    emit(event: string, ...args: any[]) {
        this.socket.emit(event, ...args);
    }
}

type Functions = (...args: any[]) => any;
export const createListener = (socket: PlayerSocket, nameSpace: string | string[], functions: Functions[]) => {
    functions.forEach((func) => {
        if (typeof nameSpace === 'string') {
            socket.on(nameSpace + ':' + func.name, func);
        }

        for (const a of nameSpace) {
            socket.on(a + ':' + func.name, func);
        }
    });
}