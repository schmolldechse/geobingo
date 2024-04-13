import { useContext } from "react";
import { GeoBingoContext } from "../../context/GeoBingoContext";
import { Button } from "@/components/ui/button";
import { signOut } from "next-auth/react";

export default function User() {
    const context = useContext(GeoBingoContext);

    return (
        <>
            <div className="flex gap-3 items-center justify-center">
                {context.geoBingo.player?.guest ? (
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
                ) : (
                    <>
                        <img src={context.geoBingo.player?.picture}
                            alt="avatar"
                            width={50}
                            height={50}
                            className="rounded-full" />

                        <p className="text-white text-lg font-medium">
                            {context.geoBingo.player?.name}
                        </p>

                        <Button className="bg-transparent" onClick={() => signOut({ callbackUrl: '/' })}>
                            <svg width="25" height="25" viewBox="0 0 48 48" fill="none">
                                <path fillOpacity=".01" d="M0 0h48v48H0z" />
                                <path
                                    d="M23.992 6H6v36h18m9-9 9-9-9-9m-17 8.992h26"
                                    stroke="#fff"
                                    strokeWidth="4"
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                />
                            </svg>
                        </Button>
                    </>
                )}
            </div>
        </>
    )
}