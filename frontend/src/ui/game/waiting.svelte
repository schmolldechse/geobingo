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

    const kickPlayer = (playerId: string) => {
        if (!geoBingo.game) throw new Error("Game is not defined");
        console.log('test kickPlayer');
        geoBingo.game.kickPlayer(playerId);
    };

    const makeHost = (playerId: string) => {
        if (!geoBingo.game) throw new Error("Game is not defined");
        console.log('test makeHost');
        geoBingo.game.makeHost(playerId);
    };

    let maxSize = $geoBingo.game?.maxSize;
    let time = $geoBingo.game?.time;

    let hoveringPlayer = null;
</script>

<div class="p-5 space-y-[2%] overflow-hidden h-screen mx-4">
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

            <div class="flex flex-col space-y-5 mt-2">
                {#each $geoBingo.game.prompts as prompt, index (index)}
                    <div
                        class="relative flex-grow flex items-center space-x-4 h-full"
                    >
                        <input
                            type="text"
                            class="flex-grow bg-[#151951] border-2 rounded-[8px] border-[#018ad3] text-white px-4 h-10 disabled:cursor-not-allowed"
                            placeholder="Enter a prompt"
                            value={prompt}
                            disabled={$geoBingo.game.host.id !== $geoBingo.player.id}
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
                        disabled={$geoBingo.game.host.id !== $geoBingo.player.id}
                        class="w-full disabled:cursor-not-allowed"
                    />

                    <p class="text-white font-bold">{$geoBingo.game.maxSize}</p>
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
                        disabled={$geoBingo.game.host.id !== $geoBingo.player.id}
                        class="w-full disabled:cursor-not-allowed"
                    />

                    <p class="text-white font-bold">{$geoBingo.game.time}</p>
                </div>
            </div>
        </div>

        <div class="flex-1 bg-[#151951] rounded-[20px] p-4 overflow-auto">
            <h1 class="text-white font-bold text-3xl pb-4">Players</h1>

            <div class="flex flex-col space-y-5">
                <!-- svelte-ignore a11y-no-static-element-interactions -->
                <!-- svelte-ignore a11y-mouse-events-have-key-events -->
                {#each $geoBingo.game.players as player}
                    <div
                        class="flex items-center space-x-4 h-full"
                        on:mouseover={() => (hoveringPlayer = player.id)}
                        on:mouseleave={() => (hoveringPlayer = null)}
                    >
                        {#if player.picture.length > 0}
                            <!-- svelte-ignore a11y-img-redundant-alt -->
                            <img
                                src={player.picture}
                                class="w-[50px] h-[50px] rounded-full"
                                alt="player picture"
                            />
                        {:else if player.picture.length === 0}
                            <svg height="50px" width="50px">
                                <circle cx="25" cy="17.81" r="6.58" />
                                <path
                                    d="M25,26.46c-7.35,0-13.3,5.96-13.3,13.3h26.61c0-7.35-5.96-13.3-13.3-13.3Z"
                                />
                            </svg>
                        {/if}

                        <p class="text-white font-bold text-base">
                            {player.name}
                        </p>

                        {#if hoveringPlayer == player.id && $geoBingo.game.host.id === $geoBingo.player.id && player.id !== $geoBingo.player.id && player.id !== $geoBingo.game?.host.id}
                            <div class="ml-auto flex items-center">
                                <Button
                                    class="bg-transparent hover:opacity-80"
                                    on:click={() => kickPlayer(player.id)}
                                >
                                    <svg
                                        width="30"
                                        height="300"
                                        viewBox="0 0 36 36"
                                        fill="darkred"
                                    >
                                        <path
                                            class="clr-i-outline clr-i-outline-path-1"
                                            d="m19.61 18 4.86-4.86a1 1 0 0 0-1.41-1.41l-4.86 4.81-4.89-4.89a1 1 0 0 0-1.41 1.41L16.78 18 12 22.72a1 1 0 1 0 1.41 1.41l4.77-4.77 4.74 4.74a1 1 0 0 0 1.41-1.41Z"
                                        />
                                        <path
                                            class="clr-i-outline clr-i-outline-path-2"
                                            d="M18 34a16 16 0 1 1 16-16 16 16 0 0 1-16 16m0-30a14 14 0 1 0 14 14A14 14 0 0 0 18 4"
                                        />
                                        <path fill="none" d="M0 0h36v36H0z" />
                                    </svg>
                                </Button>

                                <Button
                                    class="bg-transparent hover:opacity-80"
                                    on:click={() => makeHost(player.id)}
                                >
                                    <svg
                                        width="30"
                                        height="30"
                                        viewBox="-2 -4 24 24"
                                        preserveAspectRatio="xMinYMin"
                                        class="jam jam-crown"
                                        fill="orange"
                                    >
                                        <path
                                            d="M2.776 5.106 3.648 11h12.736l.867-5.98-3.493 3.02-3.755-4.827-3.909 4.811zm10.038-1.537-.078.067.141.014 1.167 1.499 1.437-1.242.14.014-.062-.082 2.413-2.086a1 1 0 0 1 1.643.9L18.115 13H1.922L.399 2.7a1 1 0 0 1 1.65-.898L4.35 3.827l-.05.06.109-.008 1.444 1.27 1.212-1.493.109-.009-.06-.052L9.245.976a1 1 0 0 1 1.565.017zM2 14h16v1a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1z"
                                        />
                                    </svg>
                                </Button>
                            </div>
                        {/if}

                        {#if player.points ?? 0 > 0}
                            <div
                                class="ml-auto bg-gray-600 p-1 rounded-full px-3"
                            >
                                <p class="text-white font-bold">
                                    {player.points}
                                </p>
                            </div>
                        {/if}
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
