import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { useContext, useEffect, useState } from "react";
import Loading from "../loading";
import GoogleMaps from "../objects/googlemaps";
import Timer from "./objects/timer";
import Prompts from "./objects/prompts";

export default function Ingame() {
    const context = useContext(GeoBingoContext);
    const [difference, setDifference] = useState(10);
    const [state, setState] = useState('initializing');

    useEffect(() => {
        const startingAtTimestamp = new Date(context.geoBingo.game?.startingAt).getTime();
        const intervalId = setInterval(() => {
            const newDifference = Math.floor((startingAtTimestamp - Date.now()) / 1000);
            setDifference(newDifference);

            if (newDifference <= 0) {
                clearInterval(intervalId);
                setState('playing');
            }
        }, 1000);

        return () => clearInterval(intervalId);
    }, []);

    return (
        <>
            <div className="bg-gray-900 h-screen w-screen">
                {state === 'initializing' && (
                    <div className="flex flex-col items-center justify-center h-screen space-y-2">
                        <Loading />
                        {difference <= 5 && (
                            <p className="text-4xl text-center text-white font-bold">
                                Starting in {difference} second{difference > 1 ? 's' : ''} ...
                            </p>
                        )}
                    </div>
                )}

                {state === 'playing' && (
                    <div className="relative">
                        <GoogleMaps className="h-screen" />
                        <div className="absolute top-0 left-1/2 mt-4 z-50">
                            <Timer />
                        </div>
                        <div className="absolute top-1/2 left-0 transform -translate-y-1/2 z-50">
                            <Prompts />
                        </div>
                    </div>
                )}
            </div>
        </>
    );
}