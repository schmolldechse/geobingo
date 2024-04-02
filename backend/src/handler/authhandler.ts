import { createListener, PlayerSocket } from "../socket/playersocket";

export default (playerSocket: PlayerSocket) => {
    const initAuth = (
        data: any,
        callback: Function
    ) => {
        if (!data.player) return callback({ error: true, data: { message: 'Not authenticated' } });
        playerSocket.player = data.player;

        return callback({ error: false, data: { message: 'Authenticated' } });
    }

    createListener(playerSocket, 'geobingo', [initAuth]);
};