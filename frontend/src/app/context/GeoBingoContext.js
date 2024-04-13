import { createContext, useEffect, useRef, useState } from "react";
import { Player } from "../lib/objects/player";
import socket from "../lib/server/socket";
import { toast } from "sonner";

export const GeoBingoContext = createContext(null);

export const GeoBingoProvider = ({ children }) => {
    const [player, setPlayer] = useState(null);
    const [game, setGame] = useState(undefined);
    const [map, setMap] = useState(undefined);

    const gameRef = useRef(game);

    useEffect(() => {
        gameRef.current = game;
    }, [game]);

    useEffect(() => {
        setPlayer(new Player(true, null, null, null));

        const handleLobbyUpdate = (response) => {
            console.log('Handle incoming lobby update with these properties:', response);

            const copy = { ...gameRef.current, ...response };
            setGame(copy);
        }

        const handleImportantMessage = (response) => {
            console.log('Handle incoming important message:', response);

            if (response.kicked) {
                toast.warning(response.message, {
                    style: {
                        background: 'rgb(44, 6, 8)',
                        borderWidth: '0.5px',
                        borderColor: 'rgb(76, 4, 9)',
                        color: 'rgb(254, 158, 161)'
                    }
                });

                setGame(undefined);
                return;
            }

            toast.info(response.message, {
                style: {
                    background: 'rgb(1, 31, 16)',
                    borderWidth: '0.5px',
                    borderColor: 'rgb(2, 62, 30)',
                    color: 'rgb(93, 244, 169)'
                }
            });
        };

        socket.on('geobingo:lobbyUpdate', handleLobbyUpdate);
        socket.on('geobingo:important', handleImportantMessage);

        socket.on("connect", () => console.log("Connecting to server"));
        socket.on("disconnect", (response) => console.log("Disconnected from server", response));
        socket.on("error", (error) => console.error("error while connecting to backend:", error));

        return () => { game.stopSocket() }
    }, []);

    const geoBingo = { player, setPlayer, game, setGame, map, setMap };

    return (
        <GeoBingoContext.Provider value={{ geoBingo }}>
            {children}
        </GeoBingoContext.Provider>
    )
}