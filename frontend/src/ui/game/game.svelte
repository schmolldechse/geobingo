<script lang="ts">
    import { differenceInSeconds } from "date-fns";
    import { getGeoBingo } from "../../lib/geobingo";
    import { onDestroy, onMount } from "svelte";
    import Loading from "$lib/components/ui/loading.svelte";

    import { fade } from "svelte/transition";

    let geoBingo = getGeoBingo();
    let difference: number;
    let intervalId: number;
    let state: string = "preparing";

    const calculateDifference = () => {
        if (!$geoBingo.game?.startingAt) return;
        difference = differenceInSeconds(
            new Date($geoBingo.game?.startingAt),
            new Date(),
        );

        if (difference <= 0) {
            clearInterval(intervalId);
            //state = "playing";
        }
    };

    onMount(() => {
        intervalId = setInterval(calculateDifference, 1000);
    });

    onDestroy(() => {
        clearInterval(intervalId);
    });
</script>

<div class="h-screen w-screen flex items-center justify-center">
    {#if state === "preparing"}
        <div class="flex flex-col items-center justify-center space-y-5">
            <Loading />
            {#if difference <= 5}
                <p class="text-white text-5xl font-bold">
                    Starting in {difference} ...
                </p>
            {/if}
        </div>
    {:else if state === "playing"}
        <p>game</p>
    {/if}
</div>
