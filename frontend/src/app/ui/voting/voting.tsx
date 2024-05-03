import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { useContext, useEffect, useRef, useState } from "react";
import { Button } from "@/components/ui/button";
import { Capture } from "@/app/lib/objects/capture";

export default function Voting() {
    const context = useContext(GeoBingoContext);

    /**
     * GoogleMaps
     */
    const mapRef = useRef(null);
    const [map, setMap] = useState<google.maps.Map | null>(null);

    useEffect(() => {
        const initMap = () => {
            const mapOptions: google.maps.MapOptions = {
                center: { lat: 0, lng: 0 },
                zoom: 2,
                mapId: 'GEOBINGO_MAP'
            }

            const mapInstance = new google.maps.Map(mapRef.current as HTMLDivElement, mapOptions);

            // setting captures[0]'s position
            const panoramaOptions: google.maps.StreetViewPanoramaOptions = {
                position: { lat: context.geoBingo.game?.currentCapture?.capture.coordinates.lat, lng: context.geoBingo.game?.currentCapture?.capture.coordinates.lng },
                pov: { heading: context.geoBingo.game?.currentCapture?.capture.pov.heading, pitch: context.geoBingo.game?.currentCapture?.capture.pov.pitch },
                zoom: context.geoBingo.game?.currentCapture?.capture.pov.zoom,
                visible: true,
                // StreetView options
                enableCloseButton: false,
                fullscreenControl: false
            }

            const panorama = new google.maps.StreetViewPanorama(mapRef.current as HTMLDivElement, panoramaOptions);
            mapInstance.setStreetView(panorama);

            setMap(mapInstance);
        }

        initMap();
    }, []);

    /**
    * adjust the position of streetview
    */
    const adjustPosition = (capture: Capture) => {
        map.getStreetView().setPosition({ lat: capture.coordinates.lat, lng: capture.coordinates.lng });
        map.getStreetView().setPov({ heading: capture.pov.heading, pitch: capture.pov.pitch });
        map.getStreetView().setZoom(capture.pov.zoom);
    }

    /**
     * either selected like or dislike button, resets after every capture
     */
    const [selected, setSelected] = useState<'like' | 'dislike' | null>(null);

    /**
     * adjustPosition for every other capture
     */
    useEffect(() => {
        if (!map || !context.geoBingo.game?.currentCapture) return;
        adjustPosition(context.geoBingo.game?.currentCapture.capture);
        setSelected(null);
    }, [context.geoBingo.game?.currentCapture]);

    /**
     * handle vote
     */
    const handleVote = (type: 'like' | 'dislike', points: number) => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");

        setSelected(type);
        context.geoBingo.game.handleVote(context.geoBingo.game?.currentCapture.prompt, context.geoBingo.game?.currentCapture.capture.uniqueId, points, (response: any) => {
            if (!response.success) return;
        });
    }

    return (
        <div className="bg-gray-900 h-screen w-screen overflow-hidden">
            {context.geoBingo.game?.currentCapture && (
                <>
                    <div id='map' className="h-[80%]" ref={mapRef} />

                    <div className="m-3 h-[20%]">
                        <div className="flex flex-row items-center">
                            <p className="text-white font-bold text-4xl italic">{context.geoBingo.game?.currentCapture?.prompt}</p>

                            <div className="px-2 border-[3px] border-red-500 ml-auto rounded-full flex justify-center items-center">
                                <p id='timeLeft' className="italic text-xl text-red-500 m-2 font-bold">{context.geoBingo.game?.timers.voting} s</p>
                            </div>
                        </div>

                        <div className="flex flex-row items-center space-x-2">
                            <img
                                src={context.geoBingo.game?.currentCapture?.capture.player.picture}
                                className="w-[50px] h-[50px] rounded-full"
                                alt="player picture"
                            />

                            <p className="text-white font-medium text-xl">
                                {context.geoBingo.game?.currentCapture?.capture.player.id === context.geoBingo.player?.id ? (
                                    context.geoBingo.game?.currentCapture?.capture.player.name + ' (you)'
                                ) : (
                                    context.geoBingo.game?.currentCapture?.capture.player.name
                                )}
                            </p>
                        </div>

                        <div className="flex justify-center items-end mt-auto space-x-8">
                            <Button
                                className={`space-x-2 bg-transparent p-[1.5rem] hover:bg-green-700 hover:opacity-80 ${selected === 'like' ? 'bg-green-700' : 'bg-transparent'}`}
                                onClick={() => handleVote('like', 1)}
                                disabled={context.geoBingo.game?.currentCapture?.capture.player.id === context.geoBingo.player?.id}
                            >
                                <svg width="40px" height="40px" viewBox="0 0 48 48">
                                    <path fill="#F44336" d="M34 9c-4.2 0-7.9 2.1-10 5.4C21.9 11.1 18.2 9 14 9 7.4 9 2 14.4 2 21c0 11.9 22 24 22 24s22-12 22-24c0-6.6-5.4-12-12-12" />
                                </svg>
                                <p className="font-bold text-2xl text-black">Like</p>
                            </Button>

                            <Button
                                className={`space-x-2 bg-transparent p-[1.5rem] hover:bg-red-700 hover:opacity-80 ${selected === 'dislike' ? 'bg-red-700' : 'bg-transparent'}`}
                                onClick={() => handleVote('dislike', 0)}
                                disabled={context.geoBingo.game?.currentCapture?.capture.player.id === context.geoBingo.player?.id}
                            >
                                <p className="font-bold text-2xl text-black">Dislike</p>
                                <svg width="40px" height="40px" viewBox="0 0 48 48">
                                    <path fill="#F44336" d="M34 9c-4.2 0-7.9 2.1-10 5.4C21.9 11.1 18.2 9 14 9 7.4 9 2 14.4 2 21c0 11.9 22 24 22 24s22-12 22-24c0-6.6-5.4-12-12-12" />
                                    <path fill="black" d="m3.56 6.393 2.828-2.829L44.36 41.536l-2.829 2.828z" />
                                </svg>
                            </Button>
                        </div>
                    </div>
                </>
            )}
        </div>
    );
}