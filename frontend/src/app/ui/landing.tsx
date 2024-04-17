import { useContext, useEffect, useState } from "react";
import { GeoBingoContext } from "../context/GeoBingoContext";
import User from "./objects/user";
import SignIn from "./objects/signin";
import { Separator } from "@/components/ui/separator";
import { Game } from "../lib/objects/game";

const messages = [
    'Choose prompts for the Street View search',
    'Capture the Street View images of the encountered prompts',
    "Engage with friends' submissions by reviewing and voting on them",
]

export default function Landing() {
    const context = useContext(GeoBingoContext);
    const [lobbyCode, setLobbyCode] = useState('' as string);

    const handleJoinLobby = () => {
        if (!context.geoBingo.player) throw new Error('Player is not defined');
        context.geoBingo.player.join(lobbyCode, (response: any) => {
            if (response.success) context.geoBingo.setGame(new Game(response.game));
        });
    }

    const handleCreateLobby = () => {
        if (!context.geoBingo.player) throw new Error('Player is not defined');
        context.geoBingo.player.host((response: any) => {
            if (response.success) context.geoBingo.setGame(new Game(response.game));
        });
    }

    return (
        <div className="bg-gray-900 min-h-screen w-screen p-4 sm:p-24 overflow-y-auto flex items-center justify-center">
            <div className="bg-[#151951] min-h-[600px] rounded-[2em] p-4 flex flex-col items-center lg:flex-row overflow-x-auto">
                <div className="mx-4 w-min lg:w-1/2">
                    <div className="flex flex-row items-center">
                        <svg className="h-[95px] w-[95px] lg:h-[200px] lg:w-[200px]" viewBox="0 0 64 64">
                            <path
                                d="M28.3 7.24c-7.07 0-13.41 3.07-17.78 7.95 6.79 2.15 11.22 5.51 10.93 8.33-.17 1.62-1.73 3.15-3.12 3.88-5.87 3.07-8.64 2.29-9.2 4.2-.34 1.14 1.9 3.52 5.17 3.78 1.59.13 4.32.36 7.62-1.04 7.22-3.06 11.71 1.94-.57 10.58 0 0-9.29 3.69-.47 8.86 2.34.77 4.83 1.19 7.43 1.19 13.18 0 23.87-10.68 23.87-23.87S41.48 7.24 28.3 7.24Z"
                                fill="#3767B1"
                                stroke="#231F20"
                                strokeMiterlimit="10"
                                strokeWidth="2px"
                            />
                            <path
                                d="M21.34 44.93c12.27-8.64 7.78-13.64.56-10.58-3.3 1.4-6.03 1.16-7.62 1.04-3.27-.27-5.5-2.64-5.17-3.78.57-1.91 3.34-1.13 9.2-4.2 1.39-.73 2.95-2.25 3.12-3.88.29-2.82-4.13-6.19-10.92-8.33h-.02a23.75 23.75 0 0 0-6.08 15.9c0 10.53 6.82 19.46 16.29 22.63.02 0 .03-.02.01-.03-8.57-5.12.56-8.77.6-8.78Zm-.73-36.42s5.95 2.9 6.59 5.91 4.47-1.75 8.71-1.79c2.77-.03 6.2-.99 6.2-.99-.37-.62-11.2-7.13-21.5-3.13Zm28.95 11.74s-10.7.78-11.13 5.51 9.26 10.33 1.18 17.22-9.57 3.49-7.32 11.65c.51-.05 27.98-7.13 17.26-34.38Z"
                                stroke="#231F20"
                                strokeMiterlimit="10"
                                strokeWidth="2px"
                                fill="#4BB679"
                            />
                            <circle
                                cx="31.59"
                                cy="30.32"
                                r="12.2"
                                strokeMiterlimit="10"
                                strokeWidth="2px"
                                fill="none"
                                stroke="#fff"
                            />
                            <path
                                fill="#fff"
                                strokeWidth="0"
                                d="m39.6 39.53 20.03 17.25L64 50.02 40.58 38.58z"
                            />
                        </svg>
                        <h1 className="text-white text-4xl sm:text-5xl font-medium italic">GEOBINGO</h1>
                    </div>

                    <div className="space-y-1">
                        <h1 className="text-white text-2xl lg:text-3xl font-bold tracking-tighter">Ready to play geobingo?</h1>
                        <p className="text-gray-400 font-medium sm:text-lg">Compete against friends in a multiplayer game, searching for prompts within Google Street View</p>
                    </div>

                    <div className="gap-2 py-6">
                        {messages.map((message, index) => (
                            <div key={index} className="flex flex-row items-center gap-2">
                                <svg fill="#fff" width="24" height="24" viewBox="-3.5 0 19 19" className="cf-icon-svg">
                                    <path d="M4.63 15.638a1.03 1.03 0 0 1-.79-.37L.36 11.09a1.03 1.03 0 1 1 1.58-1.316l2.535 3.043L9.958 3.32a1.029 1.029 0 0 1 1.783 1.03L5.52 15.122a1.03 1.03 0 0 1-.803.511l-.088.004z" />
                                </svg>
                                <p className="text-white font-medium">{message}</p>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="flex flex-col justify-center px-4 w-min lg:w-1/2 space-y-4">
                    <div>
                        <User />

                        {context.geoBingo.player.guest && (
                            <>
                                <p className="text-white text-center mb-2">or</p>
                                <SignIn />
                            </>
                        )}
                    </div>

                    <Separator className="bg-gray-600 h-[2px] rounded-[1em]" />

                    <div className="grid gap-2">
                        <p className="text-white font-base font-medium">Enter a lobby code</p>
                        <input
                            className="w-full rounded-lg h-10 p-3 text-black"
                            value={lobbyCode}
                            onChange={(e) => setLobbyCode(e.target.value)}
                        />
                        <button
                            className="bg-[#41BBF5] rounded-lg h-10 text-black text-lg font-medium hover:opacity-90 hover:outline hover:outline-2 hover:outline-offset-2 hover:outline-[#18465C] hover:bg-[#41BBF5]"
                            onClick={() => handleJoinLobby()}
                        >
                            Join lobby
                        </button>
                    </div>

                    <div className="grid">
                        <p className="text-white font-base font-medium">or create a lobby</p>
                        <button
                            className="bg-[#41BBF5] rounded-lg h-10 text-black text-lg font-medium hover:opacity-90 hover:outline hover:outline-2 hover:outline-offset-2 hover:outline-[#18465C] hover:bg-[#41BBF5]"
                            onClick={() => handleCreateLobby()}
                        >
                            Create a lobby
                        </button>
                    </div>
                </div>
            </div>
        </div>
    )
}