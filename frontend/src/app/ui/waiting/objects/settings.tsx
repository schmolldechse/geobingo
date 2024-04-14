import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { Button } from "@/components/ui/button";
import { useContext, useEffect, useState } from "react";
import { toast } from "sonner";

export default function Settings() {
    const context = useContext(GeoBingoContext);

    const [blur, setBlur] = useState(true);

    const [time, setTime] = useState(context.geoBingo.game?.time || 10);
    const [maxSize, setMaxSize] = useState(context.geoBingo.game?.maxSize || 20);
    const [votingTime, setVotingTime] = useState(context.geoBingo.game?.votingTime || 15);

    /**
     * updating time & maxSize from incoming "lobbyUpdate" socket event
     */
    useEffect(() => {
        setTime(context.geoBingo.game?.time || 10);
        setMaxSize(context.geoBingo.game?.maxSize || 20);
        setVotingTime(context.geoBingo.game?.votingTime || 15);
    }, [context.geoBingo.game?.time, context.geoBingo.game?.maxSize, context.geoBingo.game?.votingTime]);

    return (
        <div className="bg-[#151951] rounded-[20px] p-4 overflow-y-auto max-h-[400px] sm:max-h-[750px]">
            <h1 className="text-white font-bold text-3xl">Settings</h1>

            <div className="space-y-4">
                <div className="space-y-[-10px]">
                    <p className="text-white font-bold text-lg pb-2">Players</p>

                    <div className="relative flex space-x-3 items-center">
                        <input
                            type="range"
                            max={100}
                            min={1}
                            value={maxSize}
                            onMouseUp={(e) => {
                                context.geoBingo.game.editLobby({ maxSize: maxSize })
                            }}
                            onChange={(e) => {
                                const newValue = parseInt(e.currentTarget.value);
                                setMaxSize(newValue);
                            }}
                            disabled={context.geoBingo.game?.host.id !== context.geoBingo.player?.id}
                            className="w-full disabled:cursor-not-allowed"
                        />

                        <p className="text-white font-bold">{maxSize}</p>
                    </div>
                </div>

                <div className="space-y-[-10px]">
                    <p className="text-white font-bold text-lg pb-2">Time</p>

                    <div className="relative flex space-x-3 items-center">
                        <input
                            type="range"
                            max={60}
                            min={1}
                            value={time}
                            onMouseUp={(e) => {
                                context.geoBingo.game.editLobby({ time: time })
                            }}
                            onChange={(e) => {
                                const newValue = parseInt(e.currentTarget.value);
                                setTime(newValue);
                            }}
                            disabled={context.geoBingo.game?.host.id !== context.geoBingo.player?.id}
                            className="w-full disabled:cursor-not-allowed"
                        />

                        <p className="text-white font-bold">{time}</p>
                    </div>
                </div>

                <div className="space-y-[-10px]">
                    <p className="text-white font-bold text-lg pb-2">Vote time per prompt (sec)</p>

                    <div className="relative flex space-x-3 items-center">
                        <input
                            type="range"
                            max={60}
                            min={10}
                            value={time}
                            onMouseUp={(e) => {
                                context.geoBingo.game.editLobby({ votingTime: votingTime })
                            }}
                            onChange={(e) => {
                                const newValue = parseInt(e.currentTarget.value);
                                setTime(newValue);
                            }}
                            disabled={context.geoBingo.game?.host.id !== context.geoBingo.player?.id}
                            className="w-full disabled:cursor-not-allowed"
                        />

                        <p className="text-white font-bold">{time}</p>
                    </div>
                </div>
            </div>

            <div className="space-y-2">
                <p className="text-white font-bold text-lg">Share your lobby code</p>

                <div className="flex items-center space-x-4">
                    <input
                        readOnly
                        onDoubleClick={(e) => {
                            navigator.clipboard.writeText((e.target as HTMLInputElement).value)
                                .then(() => {
                                    toast.success('Copied to clipboard', {
                                        style: {
                                            background: 'rgb(1, 31, 16)',
                                            borderWidth: '0.5px',
                                            borderColor: 'rgb(2, 62, 30)',
                                            color: 'rgb(93, 244, 169)'
                                        },
                                        duration: 3000
                                    });
                                });
                        }}
                        value={'https://' + window.location.hostname + '/?lobbyCode=' + context.geoBingo.game?.id}
                        className={`select-none bg-[#151951] w-full rounded-lg p-2 text-white font-bold outline outline-2 outline-[#018AD3] ${blur ? 'blur-sm' : ''}`}
                    />
                    <Button
                        onClick={() => setBlur(!blur)}
                        className="bg-red-500 hover:bg-red-500 hover:opacity-80"
                    >
                        {blur ? (
                            <svg width="25px" height="25px" viewBox="0 0 24 24" fill="none">
                                <path d="M9.764 5.295A8.6 8.6 0 0 1 12 5c3.757 0 6.564 2.44 8.233 4.44a3.96 3.96 0 0 1 0 5.12q-.289.346-.621.704M12.5 9.04a3 3 0 0 1 2.459 2.459M3 3l18 18m-9.5-6.041A3 3 0 0 1 9.17 13M4.35 8.778q-.312.336-.582.661a3.96 3.96 0 0 0 0 5.122C5.435 16.56 8.242 19 12 19a8.6 8.6 0 0 0 2.274-.306" stroke="#000" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                            </svg>
                        ) : (
                            <svg width="25px" height="25px" viewBox="0 0 24 24" fill="none">
                                <path d="M21.257 10.962c.474.62.474 1.457 0 2.076C19.764 14.987 16.182 19 12 19s-7.764-4.013-9.257-5.962a1.69 1.69 0 0 1 0-2.076C4.236 9.013 7.818 5 12 5s7.764 4.013 9.257 5.962" stroke="#000" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                                <circle cx="12" cy="12" r="3" stroke="#000" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                            </svg>
                        )}
                    </Button>
                </div>
            </div>
        </div>
    )
}