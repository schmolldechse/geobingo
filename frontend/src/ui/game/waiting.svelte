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
    }
</script>

<div class="p-5 space-y-[2%] overflow-hidden h-screen">
    <Button class="bg-[#018ad3]" on:click={() => leaveGame()}>
        {" < "}Leave game
    </Button>

    <div class="flex justify-between space-x-[1%] h-[calc(100vh-12.5rem)]">
        <div class="flex-1 bg-[#151951] rounded-[20px] p-4 overflow-auto">
            <h1 class="text-white font-bold text-3xl pb-4">Prompts</h1>

            <div class="flex flex-col space-y-5 ">
                {#each $geoBingo.game.prompts as prompt, index (index)}
                    <div class="flex items-center space-x-4 h-full">
                        <input
                            type="text"
                            class="flex-grow bg-[#151951] border-2 rounded-[8px] border-[#018ad3] text-white px-4 h-10"
                            placeholder="Enter a prompt"
                            value={prompt}
                        />

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
                    </div>
                {/each}
            </div>
        </div>

        <div class="flex-1 bg-[#151951] rounded-[20px]">Container 2</div>
        <div class="flex-1 bg-[#151951] rounded-[20px]">
            <h1 class="text-white font-bold text-3xl pb-4">Players</h1>

            <div class="flex flex-col space-y-5 ">
                {#each $geoBingo.game.players as player}
                    <div class="flex items-center space-x-4 h-full">
                        <p>{player.name}</p>
                    </div>
                {/each}
            </div>
        </div>
    </div>

    <div class="flex justify-center pb-5">
        <Button class="bg-[#018ad3]">
            Test
        </Button>
    </div>
</div>