import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { Player } from "@/app/lib/objects/player";
import { Prompt } from "@/app/lib/objects/prompt";
import socket from "@/app/lib/server/socket";
import { Button } from "@/components/ui/button";
import { useContext, useEffect, useState } from "react";

export default function Waiting() {
    const context = useContext(GeoBingoContext);
    const [hoveringPlayer, setHoveringPlayer] = useState(null);
    const [time, setTime] = useState(context.geoBingo.game?.time || 10);
    const [maxSize, setMaxSize] = useState(context.geoBingo.game?.maxSize || 20);

    /**
     * updating time & maxSize from incoming "lobbyUpdate" socket event
     */
    useEffect(() => {
        setTime(context.geoBingo.game?.time || 10);
        setMaxSize(context.geoBingo.game?.maxSize || 20);
    }, [context.geoBingo.game.time, context.geoBingo.game.maxSize]);

    if (!socket) throw new Error('Socket is not defined');

    const leaveGame = () => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        if (!context.geoBingo.player) throw new Error("Player is not defined");
        context.geoBingo.player.leave((response: any) => {
            if (response.success) context.geoBingo.setGame(undefined);
        });
    };

    const removePrompt = (index: number) => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        context.geoBingo.game.removePrompt(index);
    };

    const addPrompt = () => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        context.geoBingo.game.addPrompt();
    };

    const changePrompt = (index: number, prompt: string) => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        if (prompt.length === 0) removePrompt(index);
        else context.geoBingo.game.changePrompt(index, prompt);
    };

    const kickPlayer = (playerId: string) => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        context.geoBingo.game.kickPlayer(playerId);
    };

    const makeHost = (playerId: string) => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        context.geoBingo.game.makeHost(playerId);
    };

    const startGame = () => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        context.geoBingo.game.startGame();
    };

    return (
        <>
            <div className="bg-gray-900 p-5 space-y-[2%] overflow-hidden w-screen h-screen">
                <Button className="bg-[#018ad3]" onClick={() => leaveGame()}>
                    {" < "}Leave game
                </Button>

                <div className="flex justify-between space-x-[1%] h-[calc(100vh-12.5rem)]">
                    <div className="flex-1 bg-[#151951] rounded-[20px] p-4 overflow-auto">
                        <div className="flex flex-row">
                            <h1 className="text-white font-bold text-3xl pb-4">Prompts</h1>
                            {context.geoBingo.game.host.id === context.geoBingo.player.id && (
                                <Button
                                    className="ml-auto bg-[#FFA500] hover:bg-[#FFA500] hover:opacity-80"
                                    onClick={() => addPrompt()}
                                >
                                    Add prompt
                                </Button>
                            )}
                        </div>

                        <div className="flex flex-col space-y-5 mt-2">
                            {context.geoBingo.game?.prompts.map((prompt: Prompt, index) => (
                                <div key={index}
                                    className="relative flex-grow flex items-center space-x-4 h-full"
                                >
                                    <input
                                        type="text"
                                        className="flex-grow bg-[#151951] border-2 rounded-[8px] border-[#018ad3] text-white px-4 h-10 disabled:cursor-not-allowed"
                                        placeholder="Enter a prompt"
                                        value={prompt.name}
                                        disabled={context.geoBingo.game.host.id !== context.geoBingo.player.id}
                                        onChange={(event) => changePrompt(index, event.target.value)}
                                    />

                                    {context.geoBingo.game.host.id === context.geoBingo.player.id && (
                                        <Button
                                            className="bg-red-500 hover:bg-red-500 hover:opacity-80"
                                            onClick={() => removePrompt(index)}
                                        >
                                            <svg
                                                width="24px"
                                                height="24px"
                                                viewBox="0 0 24 24"
                                                fill="none"
                                            >
                                                <path
                                                    d="M7 4a2 2 0 0 1 2-2h6a2 2 0 0 1 2 2v2h4a1 1 0 1 1 0 2h-1.069l-.867 12.142A2 2 0 0 1 17.069 22H6.93a2 2 0 0 1-1.995-1.858L4.07 8H3a1 1 0 0 1 0-2h4zm2 2h6V4H9zM6.074 8l.857 12H17.07l.857-12zM10 10a1 1 0 0 1 1 1v6a1 1 0 1 1-2 0v-6a1 1 0 0 1 1-1m4 0a1 1 0 0 1 1 1v6a1 1 0 1 1-2 0v-6a1 1 0 0 1 1-1"
                                                    fill="#0D0D0D"
                                                />
                                            </svg>
                                        </Button>
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="flex-1 bg-[#151951] rounded-[20px] p-4 overflow-auto">
                        <h1 className="text-white font-bold text-3xl pb-4">Settings</h1>

                        <div className="space-y-[-10px]">
                            <p className="text-white font-bold text-lg pb-4">Players</p>
                            <div className="relative flex space-x-3 items-center">
                                <input
                                    type="range"
                                    max="100"
                                    min="1"
                                    value={maxSize}
                                    onMouseUp={(e) => {
                                        context.geoBingo.game.editLobby({ maxSize: maxSize });
                                    }}
                                    onChange={(e) => {
                                        const newValue = parseInt(e.currentTarget.value);
                                        setMaxSize(newValue);
                                    }}
                                    disabled={context.geoBingo.game.host.id !== context.geoBingo.player.id}
                                    className="w-full disabled:cursor-not-allowed"
                                />

                                <p className="text-white font-bold">{maxSize}</p>
                            </div>
                        </div>

                        <div className="space-y-[-10px] pt-4">
                            <p className="text-white font-bold text-lg pb-4">Time (min)</p>
                            <div className="relative flex space-x-3 items-center">
                                <input
                                    type="range"
                                    max="60"
                                    min="1"
                                    value={time}
                                    onMouseUp={(e) => {
                                        context.geoBingo.game.editLobby({ time: time });
                                    }} 
                                    onChange={(e) => {
                                        const newValue = parseInt(e.currentTarget.value);
                                        setTime(newValue);
                                    }}
                                    disabled={context.geoBingo.game.host.id !== context.geoBingo.player.id}
                                    className="w-full disabled:cursor-not-allowed"
                                />

                                <p className="text-white font-bold">{time}</p>
                            </div>
                        </div>

                        <div className="pt-4 space-y-2">
                            <p className="text-white font-bold text-lg">Share your lobby code</p>

                            <div className="flex items-center space-x-4">
                                <input
                                    disabled={true}
                                    value={"https://" +
                                        window.location.hostname +
                                        "/?lobbyCode=" +
                                        context.geoBingo.game?.id}
                                    className="w-full rounded-lg p-2 text-white font-bold outline outline-2 outline-red-500"
                                />
                                <Button>
                                    <svg
                                        width="30px"
                                        height="30px"
                                        viewBox="0 0 24 24"
                                        fill="darkgray"
                                    >
                                        <path
                                            d="M5.962 2.513a.75.75 0 01-.475.949l-.816.272a.25.25 0 00-.171.237V21.25c0 .138.112.25.25.25h14.5a.25.25 0 00.25-.25V3.97a.25.25 0 00-.17-.236l-.817-.272a.75.75 0 01.474-1.424l.816.273A1.75 1.75 0 0121 3.97v17.28A1.75 1.75 0 0119.25 23H4.75A1.75 1.75 0 013 21.25V3.97a1.75 1.75 0 011.197-1.66l.816-.272a.75.75 0 01.949.475z"
                                        >
                                        </path>
                                        <path
                                            d="M7 1.75C7 .784 7.784 0 8.75 0h6.5C16.216 0 17 .784 17 1.75v1.5A1.75 1.75 0 0115.25 5h-6.5A1.75 1.75 0 017 3.25v-1.5zm1.75-.25a.25.25 0 00-.25.25v1.5c0 .138.112.25.25.25h6.5a.25.25 0 00.25-.25v-1.5a.25.25 0 00-.25-.25h-6.5z"
                                        >
                                        </path>
                                    </svg>
                                </Button>
                            </div>
                        </div>
                    </div>

                    <div className="flex-1 bg-[#151951] rounded-[20px] p-4 overflow-auto">
                        <h1 className="text-white font-bold text-3xl pb-4">Players</h1>

                        <div className="flex flex-col space-y-5">
                            {context.geoBingo.game.players.map((player: Player, index) => (
                                <div key={index}
                                    className="flex items-center space-x-4 h-full"
                                    onMouseEnter={() => setHoveringPlayer(player.id)}
                                    onMouseLeave={() => setHoveringPlayer(null)}
                                >
                                    {player.picture.length > 0 ? (
                                        <img
                                            src={player.picture}
                                            className="w-[50px] h-[50px] rounded-full"
                                            alt="player picture"
                                        />
                                    ) : (
                                        <svg height="50px" width="50px">
                                            <circle cx="25" cy="17.81" r="6.58" />
                                            <path
                                                d="M25,26.46c-7.35,0-13.3,5.96-13.3,13.3h26.61c0-7.35-5.96-13.3-13.3-13.3Z"
                                            />
                                        </svg>
                                    )}

                                    <p className="text-white font-bold text-base">
                                        {player.name}
                                    </p>

                                    {hoveringPlayer == player.id && context.geoBingo.game.host.id === context.geoBingo.player.id && player.id !== context.geoBingo.player.id && (
                                        <div className="ml-auto flex items-center">
                                            <Button
                                                className="bg-transparent hover:opacity-80"
                                                onClick={() => kickPlayer(player.id)}
                                            >
                                                <svg
                                                    width="30"
                                                    height="300"
                                                    viewBox="0 0 36 36"
                                                    fill="darkred"
                                                >
                                                    <path
                                                        className="clr-i-outline clr-i-outline-path-1"
                                                        d="m19.61 18 4.86-4.86a1 1 0 0 0-1.41-1.41l-4.86 4.81-4.89-4.89a1 1 0 0 0-1.41 1.41L16.78 18 12 22.72a1 1 0 1 0 1.41 1.41l4.77-4.77 4.74 4.74a1 1 0 0 0 1.41-1.41Z"
                                                    />
                                                    <path
                                                        className="clr-i-outline clr-i-outline-path-2"
                                                        d="M18 34a16 16 0 1 1 16-16 16 16 0 0 1-16 16m0-30a14 14 0 1 0 14 14A14 14 0 0 0 18 4"
                                                    />
                                                    <path fill="none" d="M0 0h36v36H0z" />
                                                </svg>
                                            </Button>

                                            <Button
                                                className="bg-transparent hover:opacity-80"
                                                onClick={() => makeHost(player.id)}
                                            >
                                                <svg
                                                    width="30"
                                                    height="30"
                                                    viewBox="-2 -4 24 24"
                                                    preserveAspectRatio="xMinYMin"
                                                    className="jam jam-crown"
                                                    fill="orange"
                                                >
                                                    <path
                                                        d="M2.776 5.106 3.648 11h12.736l.867-5.98-3.493 3.02-3.755-4.827-3.909 4.811zm10.038-1.537-.078.067.141.014 1.167 1.499 1.437-1.242.14.014-.062-.082 2.413-2.086a1 1 0 0 1 1.643.9L18.115 13H1.922L.399 2.7a1 1 0 0 1 1.65-.898L4.35 3.827l-.05.06.109-.008 1.444 1.27 1.212-1.493.109-.009-.06-.052L9.245.976a1 1 0 0 1 1.565.017zM2 14h16v1a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1z"
                                                    />
                                                </svg>
                                            </Button>
                                        </div>
                                    )}

                                    {Number(player.points) > 0 && (
                                        <div
                                            className="ml-auto bg-gray-600 p-1 rounded-full px-3"
                                        >
                                            <p className="text-white font-bold">
                                                {player.points}
                                            </p>
                                        </div>
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>
                </div>

                <div className="flex justify-center pb-5">
                    <Button
                        className="bg-green-600 text-black font-bold text-lg hover:bg-green-600 hover:opacity-80"
                        onClick={() => startGame()}
                        disabled={context.geoBingo.game.host.id !== context.geoBingo.player.id}
                    >
                        Start game
                    </Button>
                </div>
            </div>
        </>
    )
}