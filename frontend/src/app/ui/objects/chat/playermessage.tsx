import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { PlayerChatObject } from "@/app/lib/objects/lobbychatobject";
import { useContext } from "react";

interface PlayerMessageProps {
    chatObject: PlayerChatObject;
}

export default function PlayerMessage({ chatObject }: PlayerMessageProps) {
    const context = useContext(GeoBingoContext);

    return (
        <div
            className="flex space-x-2 mt-2 items-start"
            style={{
                textAlign: context.geoBingo.player?.id === chatObject.player.id ? 'right' : 'left',
                flexDirection: context.geoBingo.player?.id === chatObject.player.id ? 'row-reverse' : 'row',
                display: 'flex',
                alignItems: 'flex-start',
            }}
        >
            <img
                src={chatObject.player.picture}
                width={40}
                height={40}
                className={`rounded-full ${context.geoBingo.player?.id === chatObject.player.id ? 'ml-4' : ''}`}
            />

            <div className="flex flex-col">
                <span className="text-white font-medium">{chatObject.player.name}</span>
                <span className="text-gray-400 text-sm text-pretty" style={{ hyphens: 'auto', wordBreak: 'break-word' }}>{chatObject.message}</span>
            </div>
        </div>
    );
}
