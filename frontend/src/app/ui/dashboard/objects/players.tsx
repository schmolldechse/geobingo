import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { Player } from "@/app/lib/objects/player";
import { Button } from "@/components/ui/button";
import { useContext, useState } from "react";

export default function Players() {
    const context = useContext(GeoBingoContext);

    const [hoveringPlayer, setHoveringPlayer] = useState(null);

    const kickPlayer = (playerId: string) => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        context.geoBingo.game.kickPlayer(playerId);
    };

    const makeHost = (playerId: string) => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        context.geoBingo.game.makeHost(playerId);
    };

    return (
        <div className="flex-1 bg-[#151951] rounded-[1rem] p-5 overflow-y-auto">
            <h1 className="text-white font-bold text-3xl">Players {context.geoBingo.game?.players.length} / {context.geoBingo.game?.maxSize}</h1>

            {context.geoBingo.game?.players.map((player: Player) => (
                <div
                    key={player.id}
                    onMouseEnter={() => setHoveringPlayer(player.id)}
                    onMouseLeave={() => setHoveringPlayer(null)}
                    className="flex items-center my-2"
                >
                    {player.guest ? (
                        <svg
                            height={50}
                            width={50}
                        >
                            <circle cx={25} cy={17.81} r={6.58} />
                            <path
                                d="M25,26.46c-7.35,0-13.3,5.96-13.3,13.3h26.61c0-7.35-5.96-13.3-13.3-13.3Z"
                            />
                        </svg>
                    ) : (
                        <img
                            src={player.picture}
                            width={50}
                            height={50}
                            className="rounded-full"
                            alt="player-picture"
                        />
                    )}

                    <p className="text-white font-bold text-base ml-2">
                        {player.name}
                        {' '}{player.id === context.geoBingo.game?.host.id ? '(Host)' : ''}
                        {' '}{player.id === context.geoBingo.player?.id ? '(you)' : ''}
                    </p>

                    <div className="ml-auto">
                        {hoveringPlayer === player.id && context.geoBingo.game?.host.id === context.geoBingo.player?.id && player.id !== context.geoBingo.player?.id ? (
                            <>
                                <Button
                                    className="bg-transparent hover:opacity-80"
                                    onClick={() => kickPlayer(player.id)}
                                >
                                    <svg
                                        width="30"
                                        height="30"
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
                            </>
                        ) : (
                            <div
                                className="bg-gray-600 p-1 px-3 rounded-full"
                            >
                                <p className="text-white font-bold">
                                    {Number(player.points)}
                                </p>
                            </div>
                        )}
                    </div>
                </div>
            ))}
        </div>
    )
}