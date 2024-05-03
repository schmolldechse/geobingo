import { GeoBingoContext } from "@/app/context/GeoBingoContext";
import { SystemChatObject } from "@/app/lib/objects/lobbychatobject";
import { useContext } from "react";

interface SystemMessageProps {
    chatObject: SystemChatObject;
}

export default function SystemMessage({ chatObject }: SystemMessageProps) {
    const context = useContext(GeoBingoContext);

    return (
        <div
            className="flex space-x-2 mt-2 items-start"
            style={{
                textAlign: 'left',
                flexDirection: 'row',
                display: 'flex',
                alignItems: 'flex-start',
            }}
        >
            {chatObject.data?.error ? (
                <svg width={25} height={25} viewBox="-3.5 0 19 19" fill="red">
                    <path d="M11.383 13.644A1.03 1.03 0 0 1 9.928 15.1L6 11.172 2.072 15.1a1.03 1.03 0 1 1-1.455-1.456l3.928-3.928L.617 5.79a1.03 1.03 0 1 1 1.455-1.456L6 8.261l3.928-3.928a1.03 1.03 0 0 1 1.455 1.456L7.455 9.716z" />
                </svg>
            ) : (
                <svg width={25} height={25} viewBox="-3.5 0 19 19" fill="green">
                    <path d="M4.63 15.638a1.03 1.03 0 0 1-.79-.37L.36 11.09a1.03 1.03 0 1 1 1.58-1.316l2.535 3.043L9.958 3.32a1.029 1.029 0 0 1 1.783 1.03L5.52 15.122a1.03 1.03 0 0 1-.803.511l-.088.004z" />
                </svg>
            )}

            <p className={`${chatObject.data?.error ? 'text-red-500' : 'text-green-500'}`}>{chatObject.message}</p>
        </div>
    );
}