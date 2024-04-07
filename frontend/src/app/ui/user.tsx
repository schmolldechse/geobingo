import { useContext } from "react";
import { GeoBingoContext } from "../context/GeoBingoContext";
import { supabase } from "../lib/supabaseClient";
import { Button } from "@/components/ui/button";

export default function User() {
    const context = useContext(GeoBingoContext);

    const handleLogout = async () => {
        if (!supabase) throw new Error('Supabase client is not defined');
        const { error } = await supabase.auth.signOut({
            scope: 'local'
        });
    }

    return (
        <>
            <div className="flex gap-3 items-center justify-center">
                {context.geoBingo.player?.auth.length > 0 ? (
                    <>
                        <img src={context.geoBingo.player?.picture}
                            alt="avatar"
                            width={50}
                            height={50}
                            className="rounded-full" />

                        <p className="text-white text-lg font-medium">
                            {context.geoBingo.player?.name}
                        </p>

                        <Button className="bg-transparent" onClick={handleLogout}>
                            <svg width="25" height="25" viewBox="0 0 48 48" fill="none">
                                <path fill-opacity=".01" d="M0 0h48v48H0z" />
                                <path
                                    d="M23.992 6H6v36h18m9-9 9-9-9-9m-17 8.992h26"
                                    stroke="#fff"
                                    stroke-width="4"
                                    stroke-linecap="round"
                                    stroke-linejoin="round"
                                />
                            </svg>
                        </Button>
                    </>
                ) : (
                    <>
                        <div className="rounded-full">
                            <svg height="50px" width="50px">
                                <circle cx="25" cy="17.81" r="6.58" />
                                <path
                                    d="M25,26.46c-7.35,0-13.3,5.96-13.3,13.3h26.61c0-7.35-5.96-13.3-13.3-13.3Z"
                                />
                            </svg>
                        </div>

                        <p className="text-white text-lg font-medium">
                            Guest {context.geoBingo.player?.name}
                        </p>
                    </>
                )}
            </div>
        </>
    )
}