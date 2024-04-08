import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { Prompt } from "@/app/lib/objects/prompt";
import React from "react";
import { useContext } from "react";

export default function Prompts() {
    const context = useContext(GeoBingoContext);

    return (
        <>
            <div className="bg-gray-900 py-4 text-center inline-block rounded-r-lg">
                {context.geoBingo.game?.prompts.map((prompt: Prompt, index) => (
                    <React.Fragment key={index}>
                        <div className="flex flex-row justify-between items-center space-x-8 px-2 m-2 rounded-lg hover:bg-[#018ad3] hover:opacity-80">
                            <svg width="20" height="20" viewBox="0 0 52 52">
                                <path d="M26 20c-4.4 0-8 3.6-8 8s3.6 8 8 8 8-3.6 8-8-3.6-8-8-8" />
                                <path d="M46 14h-5.2c-1.4 0-2.6-.7-3.4-1.8l-2.3-3.5C34.4 7 32.7 6 30.9 6h-9.8c-1.8 0-3.5 1-4.3 2.7l-2.3 3.5c-.7 1.1-2 1.8-3.4 1.8H6c-2.2 0-4 1.8-4 4v24c0 2.2 1.8 4 4 4h40c2.2 0 4-1.8 4-4V18c0-2.2-1.8-4-4-4M26 40c-6.6 0-12-5.4-12-12s5.4-12 12-12 12 5.4 12 12-5.4 12-12 12" />
                            </svg>

                            <p className="text-white font-bold text-lg">{prompt.name}</p>
                        </div>
                    </React.Fragment>
                ))}
            </div>
        </>
    )
}