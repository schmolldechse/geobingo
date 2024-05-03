import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { useContext, useEffect, useRef, useState } from "react";
import Loading from "../objects/loading";
import Timer from "./objects/timer";
import Prompts from "./objects/prompts";

export default function Ingame() {
    const context = useContext(GeoBingoContext);

    const [initializingTimer, setInitializingTimer] = useState(context.geoBingo.game?.timers.initializing || 15);
    const [state, setState] = useState('initializing');

    const mapRef = useRef(null);
    const [map, setMap] = useState<google.maps.Map | null>(null);

    useEffect(() => {
        if (context.geoBingo.game?.timers.initializing <= 0) {
            setState('playing');
            return;
        }

        setInitializingTimer(context.geoBingo.game?.timers.initializing);
    }, [context.geoBingo.game?.timers.initializing]);

    /**
     * Initializing map when state switches to 'playing'
     */
    useEffect(() => {
        if (state === 'playing' && mapRef.current) initMap();
    }, [state]);

    const initMap = () => {
        const mapOptions: google.maps.MapOptions = {
            center: { lat: 0, lng: 0 },
            zoom: 2,
            mapId: 'GEOBINGO_MAP'
        }

        const mapInstance = new google.maps.Map(mapRef.current as HTMLDivElement, mapOptions);

        const panorama = new google.maps.StreetViewPanorama(mapRef.current as HTMLDivElement, { visible: false, enableCloseButton: true });
        mapInstance.setStreetView(panorama);

        setMap(mapInstance);
    }

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
                    <div id="map" className="h-screen" ref={mapRef} />
                    <div className="absolute top-0 left-1/2 transform -translate-x-1/2 mt-4 z-50">
                        <Timer />
                    </div>
                    <div className="absolute top-1/2 left-0 transform -translate-y-1/2 z-50">
                        <Prompts map={map} />
                    </div>
                </div>
            )}
        </div>
    );
}