import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { useContext, useEffect, useRef, useState } from "react";
import { Prompt } from "@/app/lib/objects/prompt";
import { Capture } from "@/app/lib/objects/capture";
import { Button } from "@/components/ui/button";
import Leaderboard from "./objects/leaderboard";

export default function Score() {
    const context = useContext(GeoBingoContext);

    const [reviewing, setReviewing] = useState(undefined);

    const mapRef = useRef(null);
    const [map, setMap] = useState<google.maps.Map | null>(null);

    /**
     * leaving street view when pressing escape
     */
    useEffect(() => {
        const initMap = async () => {
            const mapOptions: google.maps.MapOptions = {
                center: { lat: 0, lng: 0 },
                zoom: 2,
                mapId: 'GEOBINGO_MAP',
                // MapUI options
                streetViewControl: false
            }

            const mapInstance = new google.maps.Map(mapRef.current as HTMLDivElement, mapOptions);

            // setting captures[0]'s position
            const panoramaOptions: google.maps.StreetViewPanoramaOptions = {
                position: { lat: 0, lng: 0 },
                pov: { heading: 0, pitch: 0 },
                zoom: 0,
                visible: false,
                // StreetView options
                enableCloseButton: true,
                fullscreenControl: false
            }

            const panorama = new google.maps.StreetViewPanorama(mapRef.current as HTMLDivElement, panoramaOptions);
            panorama.addListener('visible_changed', () => {
                if (!panorama.getVisible()) {
                    setReviewing(undefined);
                    //panorama.setVisible(false);
                }
            })
            mapInstance.setStreetView(panorama);

            // bounds object to fit all the markers (captures)
            const bounds = new google.maps.LatLngBounds();

            // AdvancedMarkerElement 
            const { AdvancedMarkerElement } = await google.maps.importLibrary('marker') as google.maps.MarkerLibrary;

            context.geoBingo.game?.prompts.forEach((prompt: Prompt, index: number) => {
                if (!prompt.captures) return;
                prompt.captures.forEach((capture: Capture) => {
                    // creating marker
                    const marker = new AdvancedMarkerElement({
                        position: capture.coordinates,
                        map: mapInstance,
                    });

                    marker.addListener('click', () => {
                        setReviewing({ capture, name: prompt.name });

                        const position = new google.maps.LatLng(capture.coordinates.lat, capture.coordinates.lng);

                        mapInstance.getStreetView().setPosition(position);
                        mapInstance.getStreetView().setPov({ heading: capture.pov.heading, pitch: capture.pov.pitch });
                        mapInstance.getStreetView().setZoom(capture.pov.zoom);
                        mapInstance.getStreetView().setVisible(true);
                    });

                    // extending bounds
                    bounds.extend(capture.coordinates);
                });
            });

            mapInstance.fitBounds(bounds);

            setMap(mapInstance);
        }
        initMap();
    }, []);

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
                                    map.getStreetView().setVisible(false);
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
            <div id="map" className={`${reviewing ? 'h-full' : 'h-[50%]'} rounded-lg`} ref={mapRef} />

            {!reviewing && (
                <div className="pt-5">
                    {context.geoBingo.game?.votingPlayers?.length > 0 ? (
                        <p className="text-white font-bold text-2xl">
                            {context.geoBingo.game?.votingPlayers.length} Player{context.geoBingo.game?.votingPlayers.length > 1 ? 's are' : ' is'} still voting
                        </p>
                    ) : null}

                    <div className="overflow-y-auto h-[calc(100vh-63vh)]">
                        <Leaderboard />
                    </div>
                </div>
            )}
        </div>
    )
}