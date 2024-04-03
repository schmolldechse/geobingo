<script lang="ts">
    import { supabase } from "$lib/supabaseClient";
    import { onMount } from "svelte";
    import socket from "../server/socket";
    import { getGeoBingo, initializeGeoBingo } from "$lib/geobingo";
    import Landing from "../ui/landing.svelte";
    import Waiting from "../ui/game/waiting.svelte";

    if (!supabase) throw new Error("Supabase client is not defined");
    initializeGeoBingo(null);

    let geoBingo = getGeoBingo();

    onMount(async () => {
        if (!supabase) throw new Error("Supabase client is not defined"); // because of async function?

        const {
            data: { user },
        } = await supabase.auth.getUser();
        console.log("Retrieved user data:", user);
    });

    supabase.auth.onAuthStateChange(async (event, session) => {
        if (event === "SIGNED_IN" && session) {
            geoBingo.refreshPlayer(session.user);
        } else if (event === "SIGNED_OUT") {
            [window.localStorage, window.sessionStorage].forEach((storage) => {
                Object.entries(storage).forEach(([key]) =>
                    storage.removeItem(key),
                );
            });

            geoBingo.refreshPlayer(null);
        }
    });

    socket.on("connect", () => {
        console.log("Connecting to server");
    });

    socket.on("error", (error) => {
        console.error("error while connecting to backend:", error);
    });
</script>

{#if !$geoBingo.game}
    <Landing />
{:else if $geoBingo.game}
    {#if $geoBingo.game.phase === 'waiting'}
        <Waiting />
    {:else if $geoBingo.game.phase === 'playing'}
        <p>Playing</p>
    {:else if $geoBingo.game.phase === 'score'}
        <p>Score</p>
    {/if}
{/if}