import { createContext, useState } from "react";
import { supabase } from "../lib/supabaseClient";
import { Player } from "../lib/objects/player";

export const GeoBingoContext = createContext(null);

export const GeoBingoProvider = ({ children }) => {
    if (!supabase) throw new Error('Supabase client is not defined');

    const [player, setPlayer] = useState(new Player(null));
    const [game, setGame] = useState(undefined);

    const geoBingo = { player, setPlayer, game, setGame };

    return (
        <GeoBingoContext.Provider value={{ geoBingo }}>
            {children}
        </GeoBingoContext.Provider>
    )
}