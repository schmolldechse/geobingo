import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { Loader } from "@googlemaps/js-api-loader";
import React, { useContext, useEffect, useRef } from "react";

interface GoogleMapsProps {
    className: string;
    streetView?: boolean;
}

const GoogleMaps: React.FC<GoogleMapsProps> = ({ className, streetView }) => {
    const context = useContext(GeoBingoContext);

    const mapRef = useRef<HTMLDivElement>(null);
    if (!process.env.GOOGLE_MAPS_API) throw new Error('Google Maps API Key not set');

    useEffect(() => {
        const initMap = async () => {
            const loader = new Loader({
                apiKey: process.env.GOOGLE_MAPS_API as string,
                version: "weekly",
            });

            const { Map } = await loader.importLibrary('maps');

            const position = {
                lat: 0,
                lng: 0,
            };

            const mapOptions: google.maps.MapOptions = {
                center: position,
                zoom: 2,
                mapId: 'GEOBINGO_MAP',
            };

            const map = new Map(mapRef.current as HTMLDivElement, mapOptions);

            if (streetView) {
                const panoramaOptions: google.maps.StreetViewPanoramaOptions = {
                    position: position,
                    pov: {
                        heading: 0,
                        pitch: 0,
                    },
                    zoom: 1,
                    visible: true,
                };

                const panorama = new google.maps.StreetViewPanorama(mapRef.current as HTMLDivElement, panoramaOptions);
                map.setStreetView(panorama);
            }

            context.geoBingo.setMap(map);
        }
        initMap();
    }, []);

    return (
        <div className={className} ref={mapRef} />
    )
}

export default GoogleMaps;