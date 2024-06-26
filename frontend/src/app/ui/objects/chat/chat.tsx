import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { LobbyChatObject, PlayerChatObject, SystemChatObject } from "@/app/lib/objects/lobbychatobject";
import { useContext } from "react";
import { v4 as uuidv4 } from 'uuid';
import PlayerMessage from "./playermessage";
import SystemMessage from "./systemmessage";

export function Chat({ dashboard }: { dashboard: boolean }) {
    const context = useContext(GeoBingoContext);

    const sendMessage = (lobbyChatObject: LobbyChatObject) => {
        if (!context.geoBingo.game) throw new Error("Game is not defined");
        context.geoBingo.game?.sendMessage(lobbyChatObject);
    };

    const chatClass = dashboard 
    ? "flex-1 flex flex-col bg-[#151951] rounded-[1rem] p-3 overflow-auto"
    : "bg-gray-900 flex flex-col p-2 rounded-[1rem] overflow-auto w-[75%] h-[75%]"

    return (
        <div id="chat" className={chatClass}>
            <div className="overflow-y-auto pb-2 flex-1 flex flex-col-reverse">
                {context.geoBingo.game?.chat.slice().reverse().map((chatObject: LobbyChatObject, index: number) => {
                    switch (chatObject.type) {
                        case 'player':
                            const playerChatObject = chatObject as PlayerChatObject;
                            return <PlayerMessage key={chatObject.id} chatObject={playerChatObject} />;
                        case 'system':
                            const systemChatObject = chatObject as SystemChatObject;
                            return <SystemMessage key={chatObject.id} chatObject={systemChatObject} />;
                        default:
                            return null;
                    }
                })}
            </div>

            <input
                id="chat-input"
                placeholder="Type a message..."
                className="mt-auto w-full rounded-md p-2 bg-[#151951] border-2 border-[#018AD3] outline-none text-white"
                type="text"
                maxLength={250}
                onKeyDown={(event) => {
                    if (event.key !== 'Enter') return;
                    if (event.currentTarget.value.length === 0) return;

                    event.preventDefault();

                    const lobbyChatObject: LobbyChatObject = {
                        id: uuidv4(),
                        type: 'player',
                        timestamp: Date.now(),
                        player: context.geoBingo.player!,
                        message: event.currentTarget.value
                    }

                    sendMessage(lobbyChatObject);
                    event.currentTarget.value = '';
                }}
            />
        </div>
    )
}