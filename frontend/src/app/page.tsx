'use client';

import { GeoBingoProvider } from "./context/GeoBingoContext";
import GeoBingo from "./ui/geobingo";

export default function Home() {
    return (
        <GeoBingoProvider>
            <GeoBingo />
        </GeoBingoProvider>
    );
}
