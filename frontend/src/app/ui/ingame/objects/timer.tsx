import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { useContext, useEffect, useState } from "react";

export default function Timer() {
    const context = useContext(GeoBingoContext);

    const [playingTimer, setPlayingTimer] = useState(context.geoBingo.game?.timers.playing || 600);

    useEffect(() => {
        setPlayingTimer(context.geoBingo.game?.timers.playing);
    }, [context.geoBingo.game?.timers.playing]);

    const hours = Math.floor(playingTimer / 3600);
    const minutes = Math.floor((playingTimer % 3600) / 60);
    const seconds = playingTimer % 60;

    const formattedTimeRemaining = hours > 0
        ? `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
        : `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

    let textColor = 'text-white';
    if (playingTimer === 60 || playingTimer === 30 || (playingTimer < 30 && playingTimer % 2 === 0)) textColor = 'text-red-500';

    return (
        <div className="bg-gray-900 min-w-[150px] px-8 py-2 text-center inline-block rounded-[10px]">
            <p className={`${textColor} font-bold text-2xl`}>{formattedTimeRemaining}</p>
        </div>
    )
}