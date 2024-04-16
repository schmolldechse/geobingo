import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { useContext, useEffect, useState } from "react";

export default function Timer() {
    const context = useContext(GeoBingoContext);

    const [timeLeft, setTimeLeft] = useState(0);

    useEffect(() => {
        const intervalId = setInterval(() => {
            const currentTime = Date.now();
            const gameEndTime = new Date(context.geoBingo.game?.endingAt).getTime();
            const newTimeLeft = Math.floor((gameEndTime - currentTime) / 1000);
            setTimeLeft(newTimeLeft);
        }, 1000);
    
        return () => clearInterval(intervalId);
    }, []);

    const hours = Math.floor(timeLeft / 3600);
    const minutes = Math.floor((timeLeft % 3600) / 60);
    const seconds = timeLeft % 60;

    const formattedTimeRemaining = hours > 0
        ? `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
        : `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

    let textColor = 'text-white';
    if (timeLeft === 60 || timeLeft === 30 || (timeLeft < 30 && timeLeft % 2 === 0)) {
        textColor = 'text-red-500';
    }

    return (
        <>
        <div className="bg-gray-900 min-w-[150px] px-8 py-2 text-center inline-block rounded-[10px]">
            <p className={`${textColor} font-bold text-2xl`}>{formattedTimeRemaining}</p>
        </div>
        </>
    )
}