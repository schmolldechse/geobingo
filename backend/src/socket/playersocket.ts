import { Socket } from "socket.io";
import { Player } from "../objects/player";

export type PlayerSocket = Socket & { player?: Player };

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