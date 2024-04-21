import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import socket from "@/app/lib/server/socket";
import { Button } from "@/components/ui/button";
import { useContext } from "react";
import Prompts from "./objects/prompts";
import Settings from "./objects/settings";
import Players from "./objects/players";

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
        <div className="bg-gray-900 p-5 space-y-4 h-screen overflow-y-auto sm:overflow-hidden flex flex-col justify-between">
            <Button className="bg-[#018ad3] self-start" onClick={() => leaveGame()}>
                {" < "}Leave game
            </Button>

            <div className="flex flex-col sm:flex-row space-y-[1.5rem] sm:space-y-0 sm:space-x-[1.5rem] flex-grow">
                <div className="flex-1 flex flex-col items-stretch">
                    <Prompts />
                </div>
                <div className="flex-1 flex flex-col items-stretch">
                    <Settings />
                </div>
                <div className="flex-1 flex flex-col items-stretch">
                    <Players />
                </div>
            </div>

            <div className="flex justify-center">
                <Button
                    className="bg-green-600 text-black font-bold text-lg hover:bg-green-600 hover:opacity-80"
                    onClick={() => startGame()}
                    disabled={context.geoBingo.game.host.id !== context.geoBingo.player.id}
                >
                    Start game
                </Button>
            </div>
        </div>
    )
}