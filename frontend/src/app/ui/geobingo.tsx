import { useContext, useEffect } from "react"
import { GeoBingoContext } from "../context/GeoBingoContext";
import Landing from "./landing";
import Ingame from "./ingame/playing";
import Dashboard from "./dashboard/dashboard";
import Voting from "./voting/voting";
import Score from "./score/score";
import { Toaster } from "sonner";
import { useSession } from "next-auth/react";

export default function GeoBingo() {
    const context = useContext(GeoBingoContext);

    // NextAuth session
    const { data: session, status } = useSession();

    // initialize ldrs library async because of window initialization
    useEffect(() => {
        async function getLoader() {
            const { ring } = await import('ldrs');
            ring.register();
        }
        getLoader();
    })

    if (status === 'loading' || context === null || context.geoBingo === null) {
        return (
            <div className="bg-gray-900 h-screen w-screen p-24 overflow-hidden">
                <div className="flex justify-center items-center h-full">
                    <l-ring size={150} stroke={10} bg-opacity={0} speed={2} color="#5c5c5c"></l-ring>
                </div>
            </div>
        )
    }

    console.log('phase:', context.geoBingo.game?.phase);

    return (
        <>
            {context.geoBingo.game === undefined && (<Landing />)}
            {context.geoBingo.game !== undefined && (
                (() => {
                    switch (context.geoBingo.game?.phase) {
                        case 'dashboard':
                            return <Dashboard />;
                        case 'playing':
                            return <Ingame />;
                        case 'voting':
                            return <Voting />;
                        case 'score':
                            return <Score />;
                        default:
                            return <p className="text-black text-3xl font-bold">Where did you land?!?!!?</p>;
                    }
                })()
            )}

            <Toaster className="z-50" />
        </>
    )
}