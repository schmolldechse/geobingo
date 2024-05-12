import { useContext, useEffect, useState } from "react"
import { GeoBingoContext } from "../context/GeoBingoContext";
import Landing from "./landing";
import Ingame from "./ingame/playing";
import Dashboard from "./dashboard/dashboard";
import Voting from "./voting/voting";
import Score from "./score/score";
import { Toaster } from "sonner";
import { useSession } from "next-auth/react";
import { Chat } from "./objects/chat/chat";

export default function GeoBingo() {
    const context = useContext(GeoBingoContext);

    // NextAuth session
    const { data: session, status } = useSession();

    // Chat
    const [showChat, setShowChat] = useState(false);

    // initialize ldrs library async because of window initialization
    useEffect(() => {
        async function getLoader() {
            const { ring } = await import('ldrs');
            ring.register();
        }
        getLoader();
    }, []);

    /**
     * Initializing chat
     */
    useEffect(() => {
        const handleKeyDown = (event: KeyboardEvent) => {
            if (event.key === 'Tab') {
                event.preventDefault();
                setShowChat(prev => !prev);
            }
        }

        const handleClickOutside = (event: MouseEvent) => {
            const chatElement = document.getElementById('chat') as HTMLElement;
            if (chatElement === null) return;

            if (!showChat) return;
            if (!chatElement.contains(event.target as Node)) setShowChat(false);
        }

        window.addEventListener('keydown', handleKeyDown);
        window.addEventListener('mousedown', handleClickOutside);

        return () => {
            window.removeEventListener('keydown', handleKeyDown);
            window.removeEventListener('mousedown', handleClickOutside);
        }
    }, []);

    useEffect(() => {
        const chatInput = document.getElementById('chat-input') as HTMLInputElement;
        if (chatInput) chatInput.focus();
    }, [showChat]);

    if (status === 'loading' || context === null || context.geoBingo === null) {
        return (
            <div className="bg-gray-900 h-screen w-screen p-24 overflow-hidden">
                <div className="flex justify-center items-center h-full">
                    <l-ring size={150} stroke={10} bg-opacity={0} speed={2} color="#5c5c5c"></l-ring>
                </div>
            </div>
        )
    }

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

            {showChat && context.geoBingo.game && (
                <>
                    <div className="fixed top-0 left-0 w-screen h-screen bg-black opacity-75"></div>
                    <div className="fixed top-0 left-0 w-screen h-screen flex items-center justify-center">
                        <Chat dashboard={false} />
                    </div>
                </>
            )}

            <Toaster className="z-50" />
        </>
    )
}