import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { useContext, useEffect, useState } from "react";
import Loading from "../objects/loading";
import GoogleMaps from "../objects/googlemaps";
import Timer from "./objects/timer";
import Prompts from "./objects/prompts";

export default function Ingame() {
    const context = useContext(GeoBingoContext);

    const [initializingTimer, setInitializingTimer] = useState(context.geoBingo.game?.timers.initializing || 15);
    const [state, setState] = useState('initializing');

    useEffect(() => {
        let timer = setInterval(() => {
            setInitializingTimer((prev) => {
                if (prev <= 0) {
                    clearInterval(timer);
                    setState('playing');
                    return 0;
                }
                return prev - 1;
            });
        }, 1000);

        return () => clearInterval(timer);
    }, []);

    console.log('initializingTimer', initializingTimer);

    return (
        <div className="bg-gray-900 h-screen w-screen">
            {state === 'initializing' && (
                <div className="flex flex-col items-center justify-center h-screen space-y-2">
                    <Loading />
                    {initializingTimer <= 5 && (
                        <p className="text-4xl text-center text-white font-bold">
                            Starting in {initializingTimer} second{initializingTimer > 1 ? 's' : ''} ...
                        </p>
                    )}
                </div>
            )}

            {state === 'playing' && (
                <div className="relative">
                    <GoogleMaps className="h-screen" streetViewEnabled={false} />
                    <div className="absolute top-0 left-1/2 transform -translate-x-1/2 mt-4 z-50">
                        <Timer />
                    </div>
                    <div className="absolute top-1/2 left-0 transform -translate-y-1/2 z-50">
                        <Prompts />
                    </div>
                </div>
            )}
        </div>
    );
}