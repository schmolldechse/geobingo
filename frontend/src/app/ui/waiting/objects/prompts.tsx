import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { Prompt } from "@/app/lib/objects/prompt";
import { Button } from "@/components/ui/button";
import { useContext } from "react";

export default function Prompts() {
    const context = useContext(GeoBingoContext);

    const addPrompt = () => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        context.geoBingo.game.addPrompt();
    };

    const changePrompt = (index: number, prompt: string) => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        context.geoBingo.game.changePrompt(index, prompt);
    };

    const removePrompt = (index: number) => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        context.geoBingo.game.removePrompt(index);
    };

    return (
        <div className="bg-[#151951] rounded-[20px] p-4 overflow-y-auto max-h-[400px] sm:max-h-[750px]">
            <div className="flex flex-row justify-center pb-1">
                <h1 className="text-white font-bold text-3xl">Prompts</h1>

                {context.geoBingo.game?.host.id === context.geoBingo.player?.id && (
                    <Button
                        className="bg-[#FFA500] hover:bg-[#FFA500] hover:opacity-80"
                        onClick={() => addPrompt()}
                        style={{ marginLeft: 'auto' }}
                    >
                        Add prompt
                    </Button>
                )}
            </div>

            <div className="flex flex-col space-y-5 mt-2">
                {context.geoBingo.game?.prompts.map((prompt: Prompt, index: number) => (
                    <div
                        key={index}
                        className="relative flex items-center space-x-4 h-full"
                    >
                        <input
                            type="text"
                            className="bg-[#151951] border-2 border-[#018AD3] rounded-[8px] text-white px-4 h-10 disabled:cursor-not-allowed"
                            placeholder="Enter a prompt"
                            value={prompt.name}
                            disabled={context.geoBingo.game?.host.id !== context.geoBingo.player?.id}
                            onChange={(eevent) => changePrompt(index, (event.target as HTMLInputElement).value)}
                            onKeyDown={(event) => {
                                if (event.keyCode === 13 && (event.target as HTMLInputElement).value.length === 0) removePrompt(index);
                            }}
                        />

                        {context.geoBingo.game?.host.id === context.geoBingo.player?.id && (
                            <Button
                                className="bg-red-500 hover:bg-red-500 hover:opacity-80"
                                onClick={() => removePrompt(index)}
                            >
                                <svg
                                    width="24px"
                                    height="24px"
                                    viewBox="0 0 24 24"
                                    fill="none"
                                >
                                    <path
                                        d="M7 4a2 2 0 0 1 2-2h6a2 2 0 0 1 2 2v2h4a1 1 0 1 1 0 2h-1.069l-.867 12.142A2 2 0 0 1 17.069 22H6.93a2 2 0 0 1-1.995-1.858L4.07 8H3a1 1 0 0 1 0-2h4zm2 2h6V4H9zM6.074 8l.857 12H17.07l.857-12zM10 10a1 1 0 0 1 1 1v6a1 1 0 1 1-2 0v-6a1 1 0 0 1 1-1m4 0a1 1 0 0 1 1 1v6a1 1 0 1 1-2 0v-6a1 1 0 0 1 1-1"
                                        fill="#0D0D0D"
                                    />
                                </svg>
                            </Button>
                        )}
                    </div>
                ))}
            </div>
        </div>
    )
}