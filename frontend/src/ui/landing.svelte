<script>
    import { getGeoBingo } from "$lib/geobingo";
    import User from "./user.svelte";
    import SignIn from "./signin.svelte";
    import Separator from "$lib/components/ui/separator/separator.svelte";

    let geoBingo = getGeoBingo();
    let lobbyCode = '';

    const messages = [
        "Choose prompts for the Street View search",
        "Capture the Street View images of the encountered prompts",
        "Engage with friends' submissions by reviewing and voting on them",
    ];

    const handleJoinLobby = () => {
        if (!geoBingo.player) throw new Error("Player is not defined");
        geoBingo.player.join(lobbyCode);
    };

    const handleCreateLobby = () => {
        if (!geoBingo.player) throw new Error("Player is not defined");
        geoBingo.player.host();
    };
</script>

<div class="h-screen flex flex-col justify-between lg:p-24 p-4 flex-grow">
    <div
        class="min-h-[600px] grid text-white gap-6 lg:grid-cols-2 bg-[#151951] p-2 lg:p-8 rounded-[20px]"
    >
        <div class="px-4 md:px-6 flex flex-col justify-center space-y-4">
            <div class="flex flex-row items-center">
                <svg height="200px" width="200px" viewBox="0 0 64 64">
                    <path
                        d="M28.3 7.24c-7.07 0-13.41 3.07-17.78 7.95 6.79 2.15 11.22 5.51 10.93 8.33-.17 1.62-1.73 3.15-3.12 3.88-5.87 3.07-8.64 2.29-9.2 4.2-.34 1.14 1.9 3.52 5.17 3.78 1.59.13 4.32.36 7.62-1.04 7.22-3.06 11.71 1.94-.57 10.58 0 0-9.29 3.69-.47 8.86 2.34.77 4.83 1.19 7.43 1.19 13.18 0 23.87-10.68 23.87-23.87S41.48 7.24 28.3 7.24Z"
                        fill="#3767B1"
                        stroke="#231F20"
                        stroke-miterlimit="10"
                        stroke-width="2px"
                    />
                    <path
                        d="M21.34 44.93c12.27-8.64 7.78-13.64.56-10.58-3.3 1.4-6.03 1.16-7.62 1.04-3.27-.27-5.5-2.64-5.17-3.78.57-1.91 3.34-1.13 9.2-4.2 1.39-.73 2.95-2.25 3.12-3.88.29-2.82-4.13-6.19-10.92-8.33h-.02a23.75 23.75 0 0 0-6.08 15.9c0 10.53 6.82 19.46 16.29 22.63.02 0 .03-.02.01-.03-8.57-5.12.56-8.77.6-8.78Zm-.73-36.42s5.95 2.9 6.59 5.91 4.47-1.75 8.71-1.79c2.77-.03 6.2-.99 6.2-.99-.37-.62-11.2-7.13-21.5-3.13Zm28.95 11.74s-10.7.78-11.13 5.51 9.26 10.33 1.18 17.22-9.57 3.49-7.32 11.65c.51-.05 27.98-7.13 17.26-34.38Z"
                        stroke="#231F20"
                        stroke-miterlimit="10"
                        stroke-width="2px"
                        fill="#4BB679"
                    />
                    <circle
                        cx="31.59"
                        cy="30.32"
                        r="12.2"
                        stroke-miterlimit="10"
                        stroke-width="2px"
                        fill="none"
                        stroke="#fff"
                    />
                    <path
                        fill="#fff"
                        stroke-width="0"
                        d="m39.6 39.53 20.03 17.25L64 50.02 40.58 38.58z"
                    />
                </svg>

                <h1 class="text-5xl font-medium italic">GEOBINGO</h1>
            </div>

            <div class="space-y-1">
                <h1 class="text-3xl font-bold tracking-tighter sm:text-3xl">
                    Ready to play geobingo?
                </h1>
                <p
                    class="max-w-[500px] text-gray-500 md:text-base/relaxed lg:text-sm/relaxed xl:text-base/relaxed dark:text-gray-400 text-2xl font-medium"
                >
                    Compete against friends in a multiplayer game, searching for
                    prompts within Google Street View
                </p>
            </div>

            <ul class="flex flex-col gap-2 py-6">
                {#each messages as message, index}
                    <li class="flex flex-row items-center">
                        <svg
                            fill="#fff"
                            width="24px"
                            height="24px"
                            viewBox="0 0 24 24"
                        >
                            <path
                                d="M21.582 5.543a1 1 0 0 1 0 1.414l-11.33 11.33a1 1 0 0 1-1.407.006l-6.546-6.429a1 1 0 1 1 1.402-1.427l5.838 5.735 10.629-10.63a1 1 0 0 1 1.414 0"
                            />
                        </svg>
                        <p>{message}</p>
                    </li>
                {/each}
            </ul>
        </div>

        <div class="px-4 md:px-6 flex flex-col justify-center space-y-4">
            <div>
                <User />

                {#if Object.keys($geoBingo.player.auth).length === 0}
                    <p class="italic text-center mb-2">or</p>
                    <SignIn />
                {/if}
            </div>

            <Separator class="bg-gray-600 w-[75%] h-[2px] self-center rounded-[5px]" />

            <div class="grid gap-2 w-[75%] self-center">
                <p class="font-base font-medium">Enter a lobby code</p>
                <input 
                    class="w-full rounded-lg h-10 p-3 text-black" 
                    bind:value={lobbyCode}
                />
                <button
                    class="w-full bg-[#41BBF5] rounded-lg h-10 text-black text-lg font-medium hover:opacity-90 hover:outline hover:outline-2 hover:outline-offset-2 hover:outline-[#18465C] hover:bg-[#41BBF5]"
                    on:click={handleJoinLobby}
                >
                    Join lobby
                </button>
            </div>

            <div class="flex flex-col justify-center space-y-1 w-[75%] self-center">
                <p class="font-base font-medium">or create a lobby</p>
                <button
                    class="w-full bg-[#41BBF5] rounded-lg h-10 text-black text-lg font-medium hover:opacity-90 hover:outline hover:outline-2 hover:outline-offset-2 hover:outline-[#18465C] hover:bg-[#41BBF5]"
                    on:click={handleCreateLobby}
                >
                    Create a lobby
                </button>
            </div>
        </div>
    </div>
</div>
