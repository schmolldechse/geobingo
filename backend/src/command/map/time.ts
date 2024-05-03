import { Lobby, sendChatMessage } from "../../objects/lobby";
import { PlayerSocket } from "../../socket/playersocket";
import { Command } from "../command";
import { v4 as uuidv4 } from 'uuid';
import parse from "parse-duration";
import prettyMilliseconds from 'pretty-ms';

export class TimeCommand implements Command {
    
    execute(args: string[], lobby: Lobby, sender: PlayerSocket, callback: Function): void {
        if (lobby.phase !== 'playing') return sender.sendChatMessage({ id: uuidv4(), type: 'system', timestamp: Date.now(), message: 'You can only extend the time in playing phase', data: { error: true } });
        if (sender.player?.id !== lobby.host.player?.id) return sender.sendChatMessage({ id: uuidv4(), type: 'system', timestamp: Date.now(), message: 'Only the host can extend the time', data: { error: true } });

        if (args.length !== 1) return sender.sendChatMessage({ id: uuidv4(), type: 'system', timestamp: Date.now(), message: 'Invalid number of arguments', data: { error: true } });

        var duration = parse(args[0]);
        if (duration === 0) return;

        lobby.timeManagement.current().remainingTime += duration / 1000;

        const prettyDuration = prettyMilliseconds(Math.abs(duration), { verbose: true });
        sendChatMessage(lobby, 
            { 
                id: uuidv4(), 
                type: 'system', 
                timestamp: Date.now(), 
                message: `Time has been ${duration < 0 ? 'reduced' : 'extended'} by ${prettyDuration}`, 
                data: { error: false } 
            }
        );

        return callback({ success: true, message: 'Successfully executed command' });
    }
}