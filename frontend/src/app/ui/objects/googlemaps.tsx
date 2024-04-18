import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import React, { useContext, useEffect, useRef } from "react";

interface GoogleMapsProps {
    className: string;
    streetViewEnabled?: boolean;
}

const GoogleMaps: React.FC<GoogleMapsProps> = ({ className, streetViewEnabled }) => {
    const context = useContext(GeoBingoContext);
    
    const mapRef = useRef(null);

    useEffect(() => {
        const mapOptions: google.maps.MapOptions = {
            center: { lat: 0, lng: 0 },
            zoom: 2,
            mapId: 'GEOBINGO_MAP'
        }

        const map = new google.maps.Map(mapRef.current as HTMLDivElement, mapOptions);

        const panoramaOptions: google.maps.StreetViewPanoramaOptions = {
            position: { lat: 0, lng: 0 },
            pov: { heading: 0, pitch: 0 },
            zoom: 1,
            visible: streetViewEnabled
        }

        const panorama = new google.maps.StreetViewPanorama(mapRef.current as HTMLDivElement, panoramaOptions);
        //panorama.setOptions({ enableCloseButton: true });
        map.setStreetView(panorama);

        context.geoBingo.setMap(map);
    }, [])

    return (
        <div id='map' className={className} ref={mapRef} />
    )
}

export default GoogleMaps;