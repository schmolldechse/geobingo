import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { useContext, useEffect, useState } from "react";
import GoogleMaps from "../objects/googlemaps";
import { Prompt } from "@/app/lib/objects/prompt";
import { Capture } from "@/app/lib/objects/capture";
import { Button } from "@/components/ui/button";
import Leaderboard from "./objects/leaderboard";

export default function Score() {
    const context = useContext(GeoBingoContext);
    context.geoBingo.map?.getStreetView().setOptions({ enableCloseButton: false, fullScreenControl: false }); // remove maybe because not working

    const [reviewing, setReviewing] = useState(undefined);

    /**
     * leaving street view when pressing escape
     */
    useEffect(() => {
        const handleKeyDown = (event: KeyboardEvent) => {
            if (event.key === 'Escape' && reviewing) {
                setReviewing(undefined);
                context.geoBingo.map.getStreetView().setVisible(false);
            }
        };

        window.addEventListener('keydown', handleKeyDown);

        return () => {
            window.removeEventListener('keydown', handleKeyDown);
        };
    }, []);

    /**
     * updating map
     */
    useEffect(() => {
        context.geoBingo.map.getStreetView().setVisible(false);
        context.geoBingo.map.setOptions({ mapTypeControl: false, zoomControl: true, disableDoubleClickZoom: true, streetViewControl: false, fullScreenControl: false });

        const initMap = async () => {
            const prompts = context.geoBingo.game?.prompts;
            if (!prompts) return;

            // bounds object to fit all the markers (captures)
            const bounds = new google.maps.LatLngBounds();

            prompts.forEach((prompt: Prompt, index: number) => {
                if (!prompt.captures) return;
                prompt.captures.forEach((capture: Capture) => {
                    createMarker(capture, prompt.name);
                    bounds.extend(capture.coordinates);
                });
            });

            context.geoBingo.map.fitBounds(bounds);
        }
        initMap();
    }, [context.geoBingo.game?.prompts]);

    const createMarker = (capture: Capture, name: string) => {
        const advancedMarker = new google.maps.marker.AdvancedMarkerElement({
            position: capture.coordinates,
            map: context.geoBingo.map,
        });

        advancedMarker.addListener('click', () => {
            setReviewing({ capture, name });

            const position = new google.maps.LatLng(capture.coordinates.lat, capture.coordinates.lng);

            context.geoBingo.map.getStreetView().setPosition(position);
            context.geoBingo.map.getStreetView().setPov({ heading: capture.pov.heading, pitch: capture.pov.pitch });
            context.geoBingo.map.getStreetView().setZoom(capture.pov.zoom);
            context.geoBingo.map.getStreetView().setVisible(true);
        })
    }

    return (
        <div className="bg-gray-900 h-screen w-screen overflow-y-auto p-5">
            {reviewing ? (
                <div className="absolute top-1/2 :left-[10px] transform -translate-y-1/2 z-50">
                    <div className="bg-gray-900 p-4 text-left inline-block rounded-r-lg">
                        <div className="flex flex-row items-center space-x-6">
                            <h1 className="text-white font-bold text-2xl italic">{reviewing.name}</h1>

                            <Button
                                className="p-2 border-2 border-red-500 rounded-[20px]"
                                onClick={() => {
                                    setReviewing(undefined);
                                    context.geoBingo.map.getStreetView().setVisible(false);
                                }}
                            >
                                <svg
                                    width={25}
                                    height={25}
                                    viewBox="-6 -6 24 24"
                                    preserveAspectRatio="xMinYMin"
                                    fill="red"
                                >
                                    <path d="m7.314 5.9 3.535-3.536A1 1 0 1 0 9.435.95L5.899 4.485 2.364.95A1 1 0 1 0 .95 2.364l3.535 3.535L.95 9.435a1 1 0 1 0 1.414 1.414l3.535-3.535 3.536 3.535a1 1 0 1 0 1.414-1.414L7.314 5.899z" />
                                </svg>
                            </Button>
                        </div>
                        <p className="text-gray-400 font-bold text-xl pt-5">Captured by</p>

                        <div className="flex flex-row items-center">
                            {reviewing.capture.player.picture.length > 0 ? (
                                <img
                                    src={reviewing.capture.player.picture}
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
                            <p className="text-white font-bold text-xl">{reviewing.capture.player.name}</p>
                        </div>
                    </div>
                </div>
            ) : (
                <h1 className="text-white font-bold text-3xl">Overview</h1>
            )}
            <GoogleMaps className={`${reviewing ? 'h-full' : 'h-[50%]'} rounded-lg`} streetViewEnabled={false} />

            {!reviewing && (
                <div className="pt-5">
                    {context.geoBingo.game?.votingPlayers.length > 0 && (
                        <p className="text-white font-bold text-2xl">
                            Player{context.geoBingo.game?.votingPlayers.length > 1 ? 's' : ''} are still voting {context.geoBingo.game?.votingPlayers.length} / {context.geoBingo.game?.players.length}
                        </p>
                    )}

                    <div className="overflow-y-auto h-[calc(100vh-63vh)]">
                        <Leaderboard />
                    </div>
                </div>
            )}
        </div>
    )
}