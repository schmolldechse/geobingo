import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { Button } from "@/components/ui/button";
import { useContext, useEffect, useState } from "react";
import { toast } from "sonner";

export default function Settings() {
    const context = useContext(GeoBingoContext);

    const [blur, setBlur] = useState(true);

    const [playingTime, setPlayingTime] = useState((context.geoBingo.game?.timers.playing / 60) || 10);
    const [maxSize, setMaxSize] = useState(context.geoBingo.game?.maxSize || 20);
    const [votingTime, setVotingTime] = useState(context.geoBingo.game?.timers.voting || 15);

    /**
     * updating time & maxSize from incoming "lobbyUpdate" socket event
     */
    useEffect(() => {
        setPlayingTime((context.geoBingo.game?.timers.playing / 60) || 10);
        setMaxSize(context.geoBingo.game?.maxSize || 20);
        setVotingTime(context.geoBingo.game?.timers.voting || 15);
    }, [context.geoBingo.game?.timers, context.geoBingo.game?.maxSize]);

    return (
        <div className="flex-1 bg-[#151951] rounded-[1rem] p-5 overflow-y-auto">
            <h1 className="text-white font-bold text-3xl">Settings</h1>

            <div className="flex flex-col space-y-4">
                <div className="flex flex-col">
                    <p className="text-white font-bold text-lg">Players</p>
                    <div className="flex items-center space-x-2">
                        <input
                            type="range"
                            max={100}
                            min={1}
                            value={maxSize}
                            onMouseUp={(event) => {
                                context.geoBingo.game?.editLobby({ maxSize: maxSize }, (response: any) => {
                                    if (!response.success) setMaxSize(context.geoBingo.game?.maxSize || 20);
                                })
                            }}
                            onChange={(event) => {
                                const newValue = parseInt(event.currentTarget.value);
                                setMaxSize(newValue <= context.geoBingo.game?.players.length ? context.geoBingo.game?.players.length : newValue);
                            }}
                            disabled={context.geoBingo.player?.id !== context.geoBingo.game?.host.id}
                            className="w-full disabled:cursor-not-allowed"
                        />

                        <p className="text-white font-bold">{maxSize}</p>
                    </div>
                </div>

                <div className="flex flex-col">
                    <p className="text-white font-bold text-lg">Time (min)</p>
                    <div className="flex items-center space-x-2">
                        <input
                            type="range"
                            max={60}
                            min={1}
                            value={playingTime}
                            onMouseUp={(event) => context.geoBingo.game?.editLobby({ timers: { playing: playingTime } })}
                            onChange={(event) => {
                                const newValue = parseInt(event.currentTarget.value);
                                setPlayingTime(newValue);
                            }}
                            disabled={context.geoBingo.player?.id !== context.geoBingo.game?.host.id}
                            className="w-full disabled:cursor-not-allowed"
                        />

                        <p className="text-white font-bold">{playingTime}</p>
                    </div>
                </div>

                <div className="flex flex-col">
                    <p className="text-white font-bold text-lg">Vote time per prompt (sec)</p>
                    <div className="flex items-center space-x-2">
                        <input
                            type="range"
                            max={60}
                            min={10}
                            value={votingTime}
                            onMouseUp={(event) => context.geoBingo.game?.editLobby({ timers: { voting: votingTime } })}
                            onChange={(event) => {
                                const newValue = parseInt(event.currentTarget.value);
                                setVotingTime(newValue);
                            }}
                            disabled={context.geoBingo.player?.id !== context.geoBingo.game?.host.id}
                            className="w-full disabled:cursor-not-allowed"
                        />

                        <p className="text-white font-bold">{votingTime}</p>
                    </div>
                </div>

                <div className="flex flex-col space-y-1">
                    <p className="text-white font-bold text-lg">Share your lobby code</p>
                    <div className="flex items-center space-x-2">
                        <input
                            readOnly
                            onDoubleClick={(event) => {
                                navigator.clipboard.writeText((event.target as HTMLInputElement).value)
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
                            className={`select-none bg-[#151951] rounded p-2 text-white font-bold outline outline-2 outline-[#018AD3] w-full ${blur ? 'blur-sm' : ''}`}
                        />

                        <Button
                            onClick={() => setBlur(!blur)}
                            className="bg-red-500"
                        >
                            {blur ? (
                                <svg
                                    width={25}
                                    height={25}
                                    viewBox="0 0 25 25"
                                    fill="none"
                                >
                                    <path d="M9.764 5.295A8.6 8.6 0 0 1 12 5c3.757 0 6.564 2.44 8.233 4.44a3.96 3.96 0 0 1 0 5.12q-.289.346-.621.704M12.5 9.04a3 3 0 0 1 2.459 2.459M3 3l18 18m-9.5-6.041A3 3 0 0 1 9.17 13M4.35 8.778q-.312.336-.582.661a3.96 3.96 0 0 0 0 5.122C5.435 16.56 8.242 19 12 19a8.6 8.6 0 0 0 2.274-.306" stroke="#000" strokeWidth={2} />
                                </svg>
                            ) : (
                                <svg
                                    width={25}
                                    height={25}
                                    viewBox="0 0 25 25"
                                    fill="none"
                                >
                                    <path d="M21.257 10.962c.474.62.474 1.457 0 2.076C19.764 14.987 16.182 19 12 19s-7.764-4.013-9.257-5.962a1.69 1.69 0 0 1 0-2.076C4.236 9.013 7.818 5 12 5s7.764 4.013 9.257 5.962" stroke="#000" strokeWidth={2} />
                                    <circle cx="12" cy="12" r="3" stroke="#000" strokeWidth={2} />
                                </svg>
                            )}
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    )
}