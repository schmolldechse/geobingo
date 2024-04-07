import { useContext, useEffect } from "react"
import { GeoBingoContext } from "../context/GeoBingoContext";
import { faker } from "@faker-js/faker/locale/de";
import socket from "../lib/server/socket";
import { supabase } from "../lib/supabaseClient";
import { Player } from "../lib/objects/player";
import Landing from "./landing";
import Waiting from "./ingame/waiting";

const names = [
    'Alice',
    'Bob',
    'Charlie',
    'David',
    'Eve',
    'Frank',
    'Grace',
    'Heidi',
    'Ivan',
    'Judy',
    'Mallory',
    'Oscar',
    'Peggy',
    'Trent',
    'Walter',
    'Zoe'
];

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

        let urlParameter = new URLSearchParams(window.location.search);
        let lobbyCode = urlParameter.get("lobbyCode");

        if (!lobbyCode) return;
        context.geoBingo.player.join(lobbyCode, (response: any) => {
            if (response.success) context.geoBingo.setGame(response.game);
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

            context.geoBingo.setPlayer(null);
        }
    });

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

    return (
        <>
            {context.geoBingo.game === undefined && (<Landing />)}
            {context.geoBingo.game !== undefined && (
                <Waiting />
            )}
        </>
    )
}