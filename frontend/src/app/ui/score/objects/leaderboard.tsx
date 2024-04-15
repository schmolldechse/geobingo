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
                {context.geoBingo.game?.players
                    .sort((a, b) => b.points - a.points)
                    .map((player, index) => (
                        <tr key={index}>
                            <td
                                className={
                                    (index === 0 ? 'text-yellow-600' :
                                        index === 1 ? 'text-stone-400' :
                                            index === 2 ? 'text-amber-900' :
                                                'text-white') + ' text-xl font-bold text-left'
                                }
                            >
                                {index + 1}.
                            </td>
                            <td className="flex flex-row items-center justify-center">
                                {player.picture.length > 0 ? (
                                    <img
                                        src={player.picture}
                                        width={50}
                                        height={50}
                                        className="rounded-full"
                                        alt="player-picture"
                                    />
                                ) : (
                                    <svg height={50} width={50}>
                                        <circle cx={25} cy={17.81} r={6.58} />
                                        <path
                                            d="M25,26.46c-7.35,0-13.3,5.96-13.3,13.3h26.61c0-7.35-5.96-13.3-13.3-13.3Z"
                                        />
                                    </svg>
                                )}
                                <p className="text-white font-bold text-xl">{player.name}</p>
                            </td>
                            <td className="text-white font-bold text-xl text-right">{player.points}</td>
                        </tr>
                    ))}
            </tbody>
        </table>
    )
}