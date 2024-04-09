import { createContext, useEffect, useRef, useState } from "react";
import { supabase } from "../lib/supabaseClient";
import { Player } from "../lib/objects/player";
import socket from "../lib/server/socket";

export const GeoBingoContext = createContext(null);

export const GeoBingoProvider = ({ children }) => {
    if (!supabase) throw new Error('Supabase client is not defined');

    const [player, setPlayer] = useState(null);
    const [game, setGame] = useState(undefined);
    const [map, setMap] = useState(undefined);

    const gameRef = useRef(game);

    useEffect(() => {
        gameRef.current = game;
    }, [game]);

    useEffect(() => {
        setPlayer(new Player(null));

        const handleLobbyUpdate = (response) => {
            console.log('Handle incoming lobby update with these properties:', response);

            const copy = { ...gameRef.current, ...response };
            setGame(copy);
        }

        socket.on('geobingo:lobbyUpdate', handleLobbyUpdate);

        return () => { game.stopSocket() }
    }, []);

    const geoBingo = { player, setPlayer, game, setGame, map, setMap };

    return (
        <GeoBingoContext.Provider value={{ geoBingo }}>
            {children}
        </GeoBingoContext.Provider>
    )
}