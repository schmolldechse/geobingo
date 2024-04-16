import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { useContext, useEffect, useRef, useState } from "react";
import GoogleMaps from "../objects/googlemaps";
import { Button } from "@/components/ui/button";
import { Capture } from "@/app/lib/objects/capture";

export default function Voting() {
    const context = useContext(GeoBingoContext);
    context.geoBingo.map.getStreetView().setVisible(true);
    context.geoBingo.map.getStreetView().setOptions({ enableCloseButton: false });

    /**
    * adjust the position of streetview
    */
    const adjustPosition = async (capture: Capture) => {
        context.geoBingo.map.getStreetView().setPosition({ lat: capture.coordinates.lat, lng: capture.coordinates.lng });
        context.geoBingo.map.getStreetView().setPov({ heading: capture.pov.heading, pitch: capture.pov.pitch });
        context.geoBingo.map.getStreetView().setZoom(capture.pov.zoom);
    }

    /**
     * timeLeft for voting on a capture, resets after every capture
     */
    const timeLeft = useRef(context.geoBingo.game?.votingTime || 15);

    /**
     * either selected like or dislike button, resets after every capture
     */
    const [selected, setSelected] = useState<"like" | "dislike">(null);

    const captures = context.geoBingo.game?.prompts
        .filter(prompt => prompt.captures)
        .flatMap(prompt => prompt.captures.map(capture => ({ prompt: prompt.name, capture })));
    const [currentCapture, setCurrentCapture] = useState(captures[0]);
    adjustPosition(currentCapture.capture); // adjustPosition for the first capture

    /**
     * adjustPosition for every other capture
     */
    useEffect(() => {
        if (!currentCapture) return;
        adjustPosition(currentCapture.capture);
    }, [currentCapture]);

    let captureIndex = 0;

    /**
     * switching every "votingTime" or 15s to the next capture
     */
    useEffect(() => {
        const timer = setInterval(() => {
            timeLeft.current -= 1;

            if (timeLeft.current <= 0) {
                captureIndex++;

                if (captureIndex === captures.length) {
                    const copy = { ...context.geoBingo.game, phase: 'score' };
                    context.geoBingo.setGame(copy);
                    context.geoBingo.game.finishVote();
    
                    clearInterval(timer);
                    return;
                }
    
                setCurrentCapture(captures[captureIndex]);
                timeLeft.current = context.geoBingo.game?.votingTime || 15;
                setSelected(null);
            }

            // DOM-manipulation because useState fucks up "adjustPosition"
            document.getElementById('timeLeft').innerText = timeLeft.current.toString() + ' s';
        }, 1000);

        return () => clearInterval(timer);
    }, []);

    /**
     * handle vote
     */
    const handleVote = (type: 'like' | 'dislike', points: number) => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");

        setSelected(type);
        context.geoBingo.game.handleVote(currentCapture.prompt, currentCapture.capture.uniqueId, points);
    }

    return (
        <div className="bg-gray-900 h-screen w-screen overflow-hidden">
            {currentCapture && (
                <>
                    <GoogleMaps className="h-[80%]" streetViewEnabled={true} />

                    <div className="m-3 h-[20%]">
                        <div className="flex flex-row items-center">
                            <p className="text-white font-bold text-4xl italic">{currentCapture.prompt}</p>

                            <div className="px-2 border-[3px] border-red-500 ml-auto rounded-full flex justify-center items-center">
                                <p  id='timeLeft' className="italic text-xl text-red-500 m-2 font-bold">{timeLeft.current} s</p>
                            </div>
                        </div>

                        <div className="flex flex-row items-center space-x-2">
                            {currentCapture.capture.player.picture.length > 0 ? (
                                <img
                                    src={currentCapture.capture.player.picture}
                                    className="w-[50px] h-[50px] rounded-full"
                                    alt="player picture"
                                />
                            ) : (
                                <svg height="50px" width="50px">
                                    <circle cx="25" cy="17.81" r="6.58" />
                                    <path
                                        d="M25,26.46c-7.35,0-13.3,5.96-13.3,13.3h26.61c0-7.35-5.96-13.3-13.3-13.3Z"
                                    />
                                </svg>
                            )}

                            <p className="text-white font-medium text-xl">
                                {currentCapture.capture.player.id === context.geoBingo.player.id ? (
                                    currentCapture.capture.player.name + ' (you)'
                                ) : (
                                    currentCapture.capture.player.name
                                )}
                            </p>
                        </div>

                        <div className="flex justify-center items-end mt-auto space-x-8">
                            <Button
                                className={`space-x-2 ${selected === 'like' ? 'bg-green-700' : 'bg-transparent'} p-[1.5rem] hover:bg-green-700 hover:opacity-80`}
                                onClick={() => handleVote('like', 1)}
                                disabled={currentCapture.capture.player.id === context.geoBingo.player.id}
                            >
                                <svg width="40px" height="40px" viewBox="0 0 48 48">
                                    <path fill="#F44336" d="M34 9c-4.2 0-7.9 2.1-10 5.4C21.9 11.1 18.2 9 14 9 7.4 9 2 14.4 2 21c0 11.9 22 24 22 24s22-12 22-24c0-6.6-5.4-12-12-12" />
                                </svg>
                                <p className="font-bold text-2xl text-black">Like</p>
                            </Button>

                            <Button
                                className={`space-x-2 ${selected === 'dislike' ? 'bg-red-700' : 'bg-transparent'} p-[1.5rem] hover:bg-red-700 hover:opacity-80`}
                                onClick={() => handleVote('dislike', 0)}
                                disabled={currentCapture.capture.player.id === context.geoBingo.player.id}
                            >
                                <p className="font-bold text-2xl text-black">Dislike</p>
                                <svg width="40px" height="40px" viewBox="0 0 48 48">
                                    <path fill="#F44336" d="M34 9c-4.2 0-7.9 2.1-10 5.4C21.9 11.1 18.2 9 14 9 7.4 9 2 14.4 2 21c0 11.9 22 24 22 24s22-12 22-24c0-6.6-5.4-12-12-12" />
                                    <path fill="black" d="m3.56 6.393 2.828-2.829L44.36 41.536l-2.829 2.828z" />
                                </svg>
                            </Button>
                        </div>
                    </div>
                </>
            )}
        </div>
    );
}