import { useContext, useEffect } from "react"
import { GeoBingoContext } from "../context/GeoBingoContext";
import socket from "../lib/server/socket";
import { supabase } from "../lib/supabaseClient";
import { Player } from "../lib/objects/player";
import Landing from "./landing";
import Waiting from "./ingame/waiting";
import Ingame from "./ingame/playing";
import { Game } from "../lib/objects/game";

export default function GeoBingo() {
    const context = useContext(GeoBingoContext);

    const fetchUser = async () => {
        if (!supabase) throw new Error('Supabase client is not defined');
        const {
            data: { user }
        } = await supabase.auth.getUser();
        console.log('Retrieved user data:', user);
    }

    useEffect(() => {
        fetchUser();

        socket.on("connect", () => {
            console.log("Connecting to server");
            context.geoBingo.player?.initMessageListener();
        });

        socket.on("error", (error) => {
            console.error("error while connecting to backend:", error);
        });

        socket.on("disconnect", (response) => {
            console.log("Disconnected from server", response);
        });

        let urlParameter = new URLSearchParams(window.location.search);
        let lobbyCode = urlParameter.get("lobbyCode");

        if (!lobbyCode) return;
        context.geoBingo.player.join(lobbyCode, (response: any) => {
            if (response.success) context.geoBingo.setGame(new Game(response.game));
        });
    }, []);

    supabase.auth.onAuthStateChange((event, session) => {
        if (event === "SIGNED_IN" && session) {
            context.geoBingo.setPlayer(new Player(session.user));
        } else if (event === "SIGNED_OUT") {
            [window.localStorage, window.sessionStorage].forEach((storage) => {
                Object.entries(storage).forEach(([key]) =>
                    storage.removeItem(key),
                );
            });

            context.geoBingo.setPlayer(new Player(null));
        }
    });

    return (
        <>
            {context.geoBingo.game === undefined && (<Landing />)}
            {context.geoBingo.game !== undefined && (
                (() => {
                    switch (context.geoBingo.game.phase) {
                        case 'waiting':
                            return <Waiting />;
                        case 'playing':
                            return <Ingame />;
                        case 'score':
                            return <p>lol2</p>;
                        default:
                            return <p className="text-white">Where did you land?!?!!?</p>
                    }
                })()
            )}
        </>
    )
}