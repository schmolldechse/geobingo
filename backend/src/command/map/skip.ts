import { Lobby, sendChatMessage } from "../../objects/lobby";
import { PlayerSocket } from "../../socket/playersocket";
import { Command } from "../command";
import { v4 as uuidv4 } from 'uuid';

export class SkipCommand implements Command {
    
    execute(args: string[], lobby: Lobby, sender: PlayerSocket, callback: Function): void {
        if (lobby.phase !== 'playing') return sender.sendChatMessage({ id: uuidv4(), type: 'system', timestamp: Date.now(), message: 'You can only vote to skip time in playing phase', data: { error: true } });

        if (!lobby.votedSkipTime) lobby.votedSkipTime = [];

        if (lobby.votedSkipTime?.includes(sender.player.id)) return sender.sendChatMessage({ id: uuidv4(), type: 'system', timestamp: Date.now(), message: 'You already voted to skip time', data: { error: true } });
        lobby.votedSkipTime?.push(sender.player.id);

        const skipThreshold = Math.ceil(lobby.players.length * .7);
        if (lobby.votedSkipTime.length === skipThreshold) {
            lobby.timeManagement.current().remainingTime = 30;
            sendChatMessage(lobby, { id: uuidv4(), type: 'system', timestamp: Date.now(), message: 'Time has been skipped', data: { error: false } });
        }

        sendChatMessage(lobby, { id: uuidv4(), type: 'system', timestamp: Date.now(), message: `${sender.player.name} voted to skip time ${lobby.votedSkipTime.length} / ${skipThreshold}`, data: { error: false } });

        return callback({ success: true, message: 'Successfully executed command' });
    }
}