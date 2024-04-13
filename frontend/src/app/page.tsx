'use client';

import { SessionProvider } from "next-auth/react";
import { GeoBingoProvider } from "./context/GeoBingoContext";
import GeoBingo from "./ui/geobingo";

export default function Home() {
    return (
        <SessionProvider>
            <GeoBingoProvider>
                <GeoBingo />
            </GeoBingoProvider>
        </SessionProvider>
    );
}
