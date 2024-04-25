import { createContext, useEffect, useRef, useState } from "react";
import { Player } from "../lib/objects/player";
import { Game } from "../lib/objects/game";
import socket from "../lib/server/socket";
import { toast } from "sonner";
import { Loader } from "@googlemaps/js-api-loader";
import { useSession } from "next-auth/react";

export const GeoBingoContext = createContext(null);

export const GeoBingoProvider = ({ children }) => {
    const [player, setPlayer] = useState(null);
    const [game, setGame] = useState(undefined);

    const gameRef = useRef(game);

    // initializing player
    const loginReady = useRef(false);
    const { data: session, status } = useSession();
    useEffect(() => {
        if (status === 'authenticated' && session) {
            // @ts-ignore
            setPlayer(new Player(false, session.user.name, session.user.id, session.user.image));
        } else if (status === 'unauthenticated') setPlayer(new Player(true, null, null, null));
        loginReady.current = true;
    }, [status]);

    // joining by directlink after authentication (when loginReady is true)
    useEffect(() => {
        if (!player) return;
        if (loginReady.current === false) return;

        let urlParameter = new URLSearchParams(window.location.search);
        let lobbyCode = urlParameter.get("lobbyCode");
        if (!lobbyCode) return;

        //urlParameter.delete("lobbyCode");
        //window.history.replaceState({}, document.title, window.location.pathname + '?' + urlParameter.toString());

        player.join(lobbyCode, (response) => {
            if (response.success) setGame(new Game(response.game));
        });

        loginReady.current = false;
    }, [player]);

    // updating gameRef for 'lobbyUpdate' socket event
    useEffect(() => {
        gameRef.current = game;
    }, [game]);

    // initializing socket events
    useEffect(() => {
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

    // initializing Google Maps API
    if (!process.env.NEXT_PUBLIC_GOOGLE_MAPS_API) throw new Error('Google Maps API Key not set');
    useEffect(() => {
        const loader = new Loader({
            apiKey: process.env.NEXT_PUBLIC_GOOGLE_MAPS_API,
            version: "weekly",
        });

        loader
            .importLibrary('maps', 'marker')
            .then(async () => console.log('Google Maps API loaded'))
            .catch((e) => console.error('Could not load Google Maps:', e));
    }, []);

    const geoBingo = { player, setPlayer, game, setGame };

    return (
        <GeoBingoContext.Provider value={{ geoBingo }}>
            {children}
        </GeoBingoContext.Provider>
    )
}