<script lang="ts">
    import Button from "$lib/components/ui/button/button.svelte";
    import { getGeoBingo } from "../../lib/geobingo";

    let geoBingo = getGeoBingo();

    const leaveGame = () => {
        if (!geoBingo.game) throw new Error("Game is not defined");
        if (!geoBingo.player) throw new Error("Player is not defined");
        geoBingo.player.leave();
    };

    const removePrompt = (index: number) => {
        if (!geoBingo.game) throw new Error("Game is not defined");
        geoBingo.game.removePrompt(index);
    };

    const addPrompt = () => {
        if (!geoBingo.game) throw new Error("Game is not defined");
        geoBingo.game.addPrompt();
    };

    const changePrompt = (index: number, prompt: string) => {
        if (!geoBingo.game) throw new Error("Game is not defined");
        if (prompt.length === 0) removePrompt(index);
        else geoBingo.game.changePrompt(index, prompt);
    };

    let maxSize = $geoBingo.game?.maxSize;
    let time = $geoBingo.game?.time;
</script>

<div class="p-5 space-y-[2%] overflow-hidden h-screen">
    <Button class="bg-[#018ad3]" on:click={() => leaveGame()}>
        {" < "}Leave game
    </Button>

    <div class="flex justify-between space-x-[1%] h-[calc(100vh-12.5rem)]">
        <div class="flex-1 bg-[#151951] rounded-[20px] p-4 overflow-auto">
            <div class="flex flex-row">
                <h1 class="text-white font-bold text-3xl pb-4">Prompts</h1>

                {#if $geoBingo.game.host.id === $geoBingo.player.id}
                    <Button
                        class="ml-auto bg-[#FFA500] hover:bg-[#FFA500] hover:opacity-80"
                        on:click={() => addPrompt()}
                    >
                        Add prompt
                    </Button>
                {/if}
            </div>

            <div class="flex flex-col space-y-5">
                {#each $geoBingo.game.prompts as prompt, index (index)}
                    <div
                        class="relative flex-grow flex items-center space-x-4 h-full"
                    >
                        <input
                            type="text"
                            class="flex-grow bg-[#151951] border-2 rounded-[8px] border-[#018ad3] text-white px-4 h-10 disabled:cursor-not-allowed"
                            placeholder="Enter a prompt"
                            value={prompt}
                            disabled={$geoBingo.game.host.id !==
                                $geoBingo.player.id}
                            on:change={(event) =>
                                changePrompt(index, event.target.value)}
                        />

                        {#if $geoBingo.game.host.id === $geoBingo.player.id}
                            <Button
                                class="bg-red-500 hover:bg-red-500 hover:opacity-80"
                                on:click={() => removePrompt(index)}
                            >
                                <svg
                                    width="24px"
                                    height="24px"
                                    viewBox="0 0 24 24"
                                    fill="none"
                                >
                                    <path
                                        d="M7 4a2 2 0 0 1 2-2h6a2 2 0 0 1 2 2v2h4a1 1 0 1 1 0 2h-1.069l-.867 12.142A2 2 0 0 1 17.069 22H6.93a2 2 0 0 1-1.995-1.858L4.07 8H3a1 1 0 0 1 0-2h4zm2 2h6V4H9zM6.074 8l.857 12H17.07l.857-12zM10 10a1 1 0 0 1 1 1v6a1 1 0 1 1-2 0v-6a1 1 0 0 1 1-1m4 0a1 1 0 0 1 1 1v6a1 1 0 1 1-2 0v-6a1 1 0 0 1 1-1"
                                        fill="#0D0D0D"
                                    />
                                </svg>
                            </Button>
                        {/if}
                    </div>
                {/each}
            </div>
        </div>

        <div class="flex-1 bg-[#151951] rounded-[20px] p-4 overflow-auto">
            <h1 class="text-white font-bold text-3xl pb-4">Settings</h1>

            <div class="space-y-[-10px]">
                <p class="text-white font-bold text-lg pb-4">Players</p>
                <div class="relative flex space-x-3 items-center">
                    <input
                        type="range"
                        max="100"
                        min="1"
                        bind:value={$geoBingo.game.maxSize}
                        on:input={(e) => (maxSize = parseInt(e.target.value))}
                        on:mouseup={(e) => {
                            $geoBingo.game?.editLobby({
                                maxSize: parseInt(e.target.value),
                            });
                        }}
                        class="w-full"
                    />

                    <p class="text-white font-bold">{maxSize}</p>
                </div>
            </div>

            <div class="space-y-[-10px] pt-4">
                <p class="text-white font-bold text-lg pb-4">Time (min)</p>
                <div class="relative flex space-x-3 items-center">
                    <input
                        type="range"
                        max="60"
                        min="1"
                        bind:value={$geoBingo.game.time}
                        on:input={(e) => (time = parseInt(e.target.value))}
                        on:mouseup={(e) => {
                            $geoBingo.game?.editLobby({
                                time: parseInt(e.target.value),
                            });
                        }}
                        class="w-full"
                    />

                    <p class="text-white font-bold">{time}</p>
                </div>
            </div>
        </div>

        <div class="flex-1 bg-[#151951] rounded-[20px] p-4 overflow-auto">
            <h1 class="text-white font-bold text-3xl pb-4">Players</h1>

            <div class="flex flex-col space-y-5">
                {#each $geoBingo.game.players as player}
                    <div class="flex items-center space-x-4 h-full">
                        <p>{player.name}</p>
                    </div>
                {/each}
            </div>
        </div>
    </div>

    <div class="flex justify-center pb-5">
        <Button
            class="bg-green-600 text-black font-bold text-lg hover:bg-green-600 hover:opacity-80"
            >Start game</Button
        >
    </div>
</div>
