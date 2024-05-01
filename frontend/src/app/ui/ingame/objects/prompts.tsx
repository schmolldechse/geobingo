import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { Capture } from "@/app/lib/objects/capture";
import { Prompt } from "@/app/lib/objects/prompt";
import socket from "@/app/lib/server/socket";
import { Button } from "@/components/ui/button";
import React, { useEffect } from "react";
import { useContext } from "react";
import { toast } from "sonner";

const photoSoundPath = '/sounds/photo.mp3';

interface PromptsProps {
    map: google.maps.Map;
}

export default function Prompts({ map }: PromptsProps) {
    const context = useContext(GeoBingoContext);

    const photoSound = new Audio(photoSoundPath);
    photoSound.volume = 0.08;
    photoSound.load();

    /**
     * listening for changes in the prompts array
     */
    useEffect(() => {
        console.log('Captures:', context.geoBingo.game?.prompts.filter((prompt: Prompt) => prompt.capture?.found).length, '/', context.geoBingo.game?.prompts.length);

        const promptsCaptured = context.geoBingo.game?.prompts.filter((prompt: Prompt) => prompt.capture?.found);
        context.geoBingo.game?.uploadCaptures(promptsCaptured);
    }, [context.geoBingo.game?.prompts]);

    const savePrompt = (name: string) => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        if (!map) throw new Error("Google Maps is not defined");

        if (!map.getStreetView().getVisible()) {
            toast.warning("You must be in street view to capture a prompt", {
                style: {
                    background: 'rgb(44, 6, 8)',
                    borderWidth: '0.5px',
                    borderColor: 'rgb(76, 4, 9)',
                    color: 'rgb(254, 158, 161)'
                }
            });
            return;
        }

        const newPrompts = context.geoBingo.game?.prompts.map((prompt: Prompt) => {
            if (prompt.name !== name) return prompt;

            if (prompt.capture && prompt.capture.found) return { ...prompt, capture: undefined };

            const newCapture = new Capture();
            newCapture.found = true;

            newCapture.panorama = map.getStreetView().getPano();
            newCapture.pov = { heading: map.getStreetView().getPov().heading, pitch: map.getStreetView().getPov().pitch, zoom: map.getStreetView().getZoom() };
            newCapture.coordinates = { lat: map.getStreetView().getPosition().lat(), lng: map.getStreetView().getPosition().lng() };

            photoSound.play();

            return { ...prompt, capture: newCapture };
        });

        context.geoBingo.setGame({ ...context.geoBingo.game, prompts: newPrompts });
    }

    return (
        <div className="bg-gray-900 py-4 text-center inline-block rounded-r-lg">
            <div className="max-h-[400px] max-w-[350px] overflow-x-hidden overflow-y-auto">
                <h1 className="text-2xl text-left text-gray-400 font-bold italic pl-4 mb-2">Prompts</h1>

                <div className="space-y-[4px]">
                    {context.geoBingo.game?.prompts.map((prompt: Prompt, index) => (
                        <React.Fragment key={index}>
                            <div
                                className={`flex flex-row justify-between items-center px-2 mx-2 space-x-8 rounded-lg ${prompt.capture?.found ? 'bg-green-700 hover:bg-green-700' : 'hover:bg-[#018ad3]'} hover:opacity-80`}
                                onClick={() => savePrompt(prompt.name)}
                            >
                                {prompt.capture?.found === true ? (
                                    <svg width={20} height={20} viewBox="-3.5 0 19 19">
                                        <path d="M4.63 15.638a1.03 1.03 0 0 1-.79-.37L.36 11.09a1.03 1.03 0 1 1 1.58-1.316l2.535 3.043L9.958 3.32a1.029 1.029 0 0 1 1.783 1.03L5.52 15.122a1.03 1.03 0 0 1-.803.511l-.088.004z" />
                                    </svg>
                                ) : (
                                    <svg width={20} height={20} viewBox="0 0 52 52">
                                        <path d="M26 20c-4.4 0-8 3.6-8 8s3.6 8 8 8 8-3.6 8-8-3.6-8-8-8" />
                                        <path d="M46 14h-5.2c-1.4 0-2.6-.7-3.4-1.8l-2.3-3.5C34.4 7 32.7 6 30.9 6h-9.8c-1.8 0-3.5 1-4.3 2.7l-2.3 3.5c-.7 1.1-2 1.8-3.4 1.8H6c-2.2 0-4 1.8-4 4v24c0 2.2 1.8 4 4 4h40c2.2 0 4-1.8 4-4V18c0-2.2-1.8-4-4-4M26 40c-6.6 0-12-5.4-12-12s5.4-12 12-12 12 5.4 12 12-5.4 12-12 12" />
                                    </svg>
                                )}

                                <p className="text-white font-bold text-lg">{prompt.name}</p>
                            </div>
                        </React.Fragment>
                    ))}
                </div>

                <Button onClick={() => {
                    if (!context.geoBingo.game) throw new Error("Game is not defined");
                    if (!socket) throw new Error("Socket is not defined");

                    socket.emit('geobingo:skip', { lobbyCode: context.geoBingo.game.id }, (response: any) => console.log('Response:', response));
                }}>
                    Skip time
                </Button>
            </div>
        </div>
    )
}