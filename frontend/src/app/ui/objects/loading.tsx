import { useEffect, useRef } from "react";

export default function Loading() {
    const circleElement = useRef(null);
    const pathElement = useRef(null);

    useEffect(() => {
        const intervalId = setInterval(() => {
            const x = Math.floor(Math.random() * 30) - 15;
            const y = Math.floor(Math.random() * 30) - 15;

            const rotation = Math.floor(Math.random() * 45);

            circleElement.current.style.transform = `translate(${x}px, ${y}px) rotate(${rotation}deg)`;
            pathElement.current.style.transform = `translate(${x}px, ${y}px) rotate(${rotation}deg)`;
        }, 1000);

        return () => clearInterval(intervalId);
    }, []);

    return (
        <svg height="300" width="300" viewBox="0 0 64 64">
            <path
                d="M28.3 7.24c-7.07 0-13.41 3.07-17.78 7.95 6.79 2.15 11.22 5.51 10.93 8.33-.17 1.62-1.73 3.15-3.12 3.88-5.87 3.07-8.64 2.29-9.2 4.2-.34 1.14 1.9 3.52 5.17 3.78 1.59.13 4.32.36 7.62-1.04 7.22-3.06 11.71 1.94-.57 10.58 0 0-9.29 3.69-.47 8.86 2.34.77 4.83 1.19 7.43 1.19 13.18 0 23.87-10.68 23.87-23.87S41.48 7.24 28.3 7.24Z"
                fill="#3767B1"
                stroke="#231F20"
                strokeMiterlimit="10"
                strokeWidth="2px"
            />
            <path
                d="M21.34 44.93c12.27-8.64 7.78-13.64.56-10.58-3.3 1.4-6.03 1.16-7.62 1.04-3.27-.27-5.5-2.64-5.17-3.78.57-1.91 3.34-1.13 9.2-4.2 1.39-.73 2.95-2.25 3.12-3.88.29-2.82-4.13-6.19-10.92-8.33h-.02a23.75 23.75 0 0 0-6.08 15.9c0 10.53 6.82 19.46 16.29 22.63.02 0 .03-.02.01-.03-8.57-5.12.56-8.77.6-8.78Zm-.73-36.42s5.95 2.9 6.59 5.91 4.47-1.75 8.71-1.79c2.77-.03 6.2-.99 6.2-.99-.37-.62-11.2-7.13-21.5-3.13Zm28.95 11.74s-10.7.78-11.13 5.51 9.26 10.33 1.18 17.22-9.57 3.49-7.32 11.65c.51-.05 27.98-7.13 17.26-34.38Z"
                stroke="#231F20"
                strokeMiterlimit="10"
                strokeWidth="2px"
                fill="#4BB679"
            />
            <circle
                cx="31.59"
                cy="30.32"
                r="12.2"
                strokeMiterlimit="10"
                strokeWidth="2px"
                fill="none"
                stroke="#fff"
                className="transition-transform duration-1000"
                ref={circleElement}
            />
            <path
                fill="#fff"
                strokeWidth="0"
                d="m39.6 39.53 20.03 17.25L64 50.02 40.58 38.58z"
                className="transition-transform duration-1000"
                ref={pathElement}
            />
        </svg>
    );
}