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
        <div className="flex-1 bg-[#151951] rounded-[1rem] p-5 overflow-y-auto">
            <div className="flex items-center justify-between">
                <h1 className="text-white font-bold text-3xl">Prompts</h1>

                {context.geoBingo.player?.id === context.geoBingo.game?.host.id && (
                    <Button
                        className="bg-[#FFA500]"
                        onClick={addPrompt}
                    >
                        Add prompt
                    </Button>
                )}
            </div>

            {context.geoBingo.game?.prompts.map((prompt: Prompt, index: number) => (
                <div
                    key={index}
                    className="flex items-center justify-between mt-4 space-x-4"
                >
                    <input
                        type="text"
                        className="bg-[#151951] border-2 border-[#018AD3] rounded text-white flex-grow disabled:cursor-not-allowed h-10 px-3"
                        placeholder="Enter a prompt"
                        value={prompt.name}
                        disabled={context.geoBingo.player?.id !== context.geoBingo.game?.host.id}
                        onChange={(event) => changePrompt(index, (event.target as HTMLInputElement).value)}
                        onKeyDown={(event) => {
                            if (event.key !== 'Enter') return;
                            if ((event.target as HTMLInputElement).value.length !== 0) return;

                            removePrompt(index);
                        }}
                    />

                    {context.geoBingo.player?.id === context.geoBingo.game?.host.id && (
                        <Button
                            className="bg-red-500 h-10"
                            onClick={() => removePrompt(index)}
                        >
                            <svg height={25} width={25} viewBox="0 0 25 25" fill="none">
                                <path d="M7 4a2 2 0 0 1 2-2h6a2 2 0 0 1 2 2v2h4a1 1 0 1 1 0 2h-1.069l-.867 12.142A2 2 0 0 1 17.069 22H6.93a2 2 0 0 1-1.995-1.858L4.07 8H3a1 1 0 0 1 0-2h4zm2 2h6V4H9zM6.074 8l.857 12H17.07l.857-12zM10 10a1 1 0 0 1 1 1v6a1 1 0 1 1-2 0v-6a1 1 0 0 1 1-1m4 0a1 1 0 0 1 1 1v6a1 1 0 1 1-2 0v-6a1 1 0 0 1 1-1" fill="#0D0D0D"/>
                            </svg>
                        </Button>
                    )}
                </div>
            ))}
        </div>
    )
}