import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import socket from "@/app/lib/server/socket";
import { Button } from "@/components/ui/button";
import { useContext } from "react";
import Prompts from "./objects/prompts";
import Settings from "./objects/settings";
import Players from "./objects/players";
import Chat from "./objects/chat";

export default function Dashboard() {
    const context = useContext(GeoBingoContext);

    if (!socket) throw new Error('Socket is not defined');

    const leaveGame = () => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        if (!context.geoBingo.player) throw new Error("Player is not defined");
        context.geoBingo.player.leave((response: any) => {
            if (response.success) context.geoBingo.setGame(undefined);
        });
    };

    const startGame = () => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        context.geoBingo.game.startGame();
    };

    return (
        <div className="flex flex-col justify-between bg-gray-900 p-5 h-screen overflow-y-auto sm:overflow-y-hidden space-y-5">
            <Button
                className="self-start bg-[#018AD3] text-white"
                onClick={leaveGame}
            >
                &lt; Leave Game
            </Button>

            <div className="flex flex-col sm:flex-row flex-grow space-y-5 sm:space-y-0 sm:space-x-5">
                <div className="flex-1 flex flex-col overflow-y-auto max-h-[calc(100vh-10rem)] ">
                    <Prompts />
                </div>
                <Settings />
                <Players />
            </div>

            <Button
                className="self-center bg-green-600 text-white"
                onClick={startGame}
                disabled={context.geoBingo.game?.host.id !== context.geoBingo.player?.id}
            >
                Start Game
            </Button>
        </div>
    )
}