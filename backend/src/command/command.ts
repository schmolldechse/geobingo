import { Lobby } from "../objects/lobby";
import { PlayerSocket } from "../socket/playersocket";
import { v4 as uuidv4 } from 'uuid';

export interface Command {
    execute(args: string[], lobby: Lobby, sender: PlayerSocket, callback: Function): void;
}

export class CommandManager {
    private commands = new Map<string, Command>();

    registerCommand(name: string, command: Command) {
        this.commands.set(name, command);
    }

    executeCommand(name: string, args: string[], sender: PlayerSocket, lobby: Lobby, callback: Function) {
        const command = this.commands.get(name);
        if (!command) return sender.sendChatMessage({ id: uuidv4(), type: 'system', timestamp: Date.now(), message: 'Command not found', data: { error: true } });

        command.execute(args, lobby, sender, callback);
    }
}