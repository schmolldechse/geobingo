<script lang="ts">
    import Button from "$lib/components/ui/button/button.svelte";
    import { supabase } from "$lib/supabaseClient";

    const handleSignIn = async (provider: "twitch" | "google") => {
        if (!supabase) throw new Error("Supabase client is not defined");
        const { data, error } = await supabase.auth.signInWithOAuth({
            provider: provider,
        });
    };
</script>

<div class="mx-4 flex flex-row space-x-8">
    <Button
        class="bg-[#653DA2] w-full rounded-[10px] p-3 py-6 flex gap-3 items-center inline-flex hover:outline hover:outline-2 hover:outline-offset-2 hover:outline-[#522891] hover:bg-[#653DA2]"
        on:click={() => handleSignIn("twitch")}
    >
        <svg width="30px" height="30px" viewBox="0 0 16 16" fill="none">
            <g fill="#000">
                <path
                    d="M4.5 1 2 3.5v9h3V15l2.5-2.5h2L14 8V1zM13 7.5l-2 2H9l-1.75 1.75V9.5H5V2h8z"
                />
                <path d="M11.5 3.75h-1v3h1zm-2.75 0h-1v3h1z" />
            </g>
        </svg>

        <p class="text-black text-lg">Continue with Twitch</p>
    </Button>

    <Button
        class="bg-white w-full rounded-[10px] p-3 py-6 flex gap-3 items-center inline-flex hover:outline hover:outline-2 hover:outline-offset-2 hover:outline-blue-400 hover:bg-white"
        on:click={() => handleSignIn("google")}
    >
        <svg
            width="30"
            height="30"
            viewBox="0 0 16 16"
            fill="none"
            ><path
                fill="#4285F4"
                d="M14.9 8.161c0-.476-.039-.954-.121-1.422h-6.64v2.695h3.802a3.24 3.24 0 0 1-1.407 2.127v1.75h2.269c1.332-1.22 2.097-3.02 2.097-5.15"
            /><path
                fill="#34A853"
                d="M8.14 15c1.898 0 3.499-.62 4.665-1.69l-2.268-1.749c-.631.427-1.446.669-2.395.669-1.836 0-3.393-1.232-3.952-2.888H1.85v1.803A7.04 7.04 0 0 0 8.14 15"
            /><path
                fill="#FBBC04"
                d="M4.187 9.342a4.17 4.17 0 0 1 0-2.68V4.859H1.849a6.97 6.97 0 0 0 0 6.286z"
            /><path
                fill="#EA4335"
                d="M8.14 3.77a3.84 3.84 0 0 1 2.7 1.05l2.01-1.999a6.8 6.8 0 0 0-4.71-1.82 7.04 7.04 0 0 0-6.29 3.858L4.186 6.66c.556-1.658 2.116-2.89 3.952-2.89z"
            /></svg
        >

        <p class="text-black text-lg">Continue with Google</p>
    </Button>
</div>
