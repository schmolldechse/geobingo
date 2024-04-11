import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { useContext } from "react";

export default function Score() {
    const context = useContext(GeoBingoContext);

    console.log('geoBingo:', context.geoBingo.game);

    return (
        <>
        <div className="bg-gray-900 h-screen w-screen">

        </div>
        </>
    )
}