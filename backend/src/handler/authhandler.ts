import { createListener, PlayerSocket } from "../socket/playersocket";

export default (playerSocket: PlayerSocket) => {
    const initAuth = (
        data: any,
        callback: Function
    ) => {
        if (typeof callback !== "function") return;

        if (!data.player) return callback({ success: false, data: { message: 'Not authenticated' } });
        playerSocket.player = data.player;

        return callback({ success: true, data: { message: 'Authenticated' } });
    }

    createListener(playerSocket, 'geobingo', [initAuth]);
};