<script>
    import Button from "$lib/components/ui/button/button.svelte";
    import { getGeoBingo } from "$lib/geobingo";
    import { supabase } from "$lib/supabaseClient";

    let geoBingo = getGeoBingo();

    const handleLogout = async () => {
        if (!supabase) throw new Error("Supabase client is not defined");
        const { error } = await supabase.auth.signOut({
            scope: "local",
        });
    };
</script>

<div class="mx-[5%] items-center">
    <div class="flex gap-3 items-center justify-center">
        {#if Object.keys(geoBingo.player.auth).length !== 0}
            <img
                src={geoBingo.player.picture}
                alt="avatar"
                width={50}
                height={50}
                class="rounded-full"
            />

            <p class="text-white text-lg font-medium">
                {geoBingo.player.name}
            </p>

            <Button class="bg-transparent" onClick={handleLogout}>
                <svg width="25" height="25" viewBox="0 0 48 48">
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
        {:else}
            <div class="rounded-full">
                <svg height="50px" width="50px">
                    <circle cx="25" cy="17.81" r="6.58" />
                    <path
                        d="M25,26.46c-7.35,0-13.3,5.96-13.3,13.3h26.61c0-7.35-5.96-13.3-13.3-13.3Z"
                    />
                </svg>
            </div>

            <p class="text-white text-lg font-medium">
                Guest {geoBingo.player.name}
            </p>
        {/if}
    </div>
</div>
