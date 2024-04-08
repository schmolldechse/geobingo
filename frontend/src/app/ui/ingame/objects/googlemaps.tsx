import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { Loader } from "@googlemaps/js-api-loader";
import React, { useContext, useEffect, useRef } from "react";

const GoogleMaps = () => {
    const context = useContext(GeoBingoContext);

    const mapRef = useRef<HTMLDivElement>(null);
    if (!process.env.NEXT_PUBLIC_GOOGLE_MAPS_API) throw new Error('Google Maps API Key not set');

    useEffect(() => {
        const initMap = async () => {
            const loader = new Loader({
                apiKey: process.env.NEXT_PUBLIC_GOOGLE_MAPS_API as string,
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

            context.geoBingo.setMap(map);
        }
        initMap();
    }, []);

    return (
        <div className="h-screen" ref={mapRef} />
    )
}

export default React.memo(GoogleMaps);