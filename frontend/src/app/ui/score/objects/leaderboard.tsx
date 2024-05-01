import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { useContext } from "react";

export default function Leaderboard() {
    const context = useContext(GeoBingoContext);

    return (
        <table className="table-fixed w-full">
            <thead>
                <tr className="">
                    <th className="text-left text-white font-bold text-2xl">Ranking</th>
                    <th className="text-center text-white font-bold text-2xl">Player</th>
                    <th className="text-right text-white font-bold text-2xl">Points</th>
                </tr>
            </thead>

            <tbody>
                {(() => {
                    let currentPlace = 1;
                    let currentPoints = context.geoBingo.game?.players[0].points;

                    return context.geoBingo.game?.players
                        .sort((a, b) => b.points - a.points)
                        .map((player, index) => {
                            if (player.points < currentPoints) {
                                currentPlace = index + 1;
                                currentPoints = player.points;
                            }

                            return (
                                <tr key={index}>
                                    <td
                                        className={
                                            (currentPlace === 1 ? 'text-yellow-600' :
                                                currentPlace === 2 ? 'text-stone-400' :
                                                    currentPlace === 3 ? 'text-amber-900' :
                                                        'text-white'
                                            ) + ' text-xl font-bold text-left'
                                        }
                                    >
                                        {currentPlace}.
                                    </td>
                                    <td className="flex flex-row items-center justify-center">
                                        <img
                                            src={player.picture}
                                            width={50}
                                            height={50}
                                            className="rounded-full"
                                            alt="player-picture"
                                        />
                                        <p className="text-white font-bold text-xl">{player.name}</p>
                                    </td>
                                    <td className="text-white font-bold text-xl text-right">{player.points}</td>
                                </tr>
                            )
                        })
                })()}
            </tbody>
        </table>
    )
}