<script lang="ts">
	import AlertTriangle from "@lucide/svelte/icons/triangle-alert";
	import CheckCircle2 from "@lucide/svelte/icons/circle-check-big";
	import Clock3 from "@lucide/svelte/icons/clock-3";
	import LoaderCircle from "@lucide/svelte/icons/loader-circle";
	import Play from "@lucide/svelte/icons/play";
	import Button from "$lib/components/ui/Button.svelte";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import type { LobbyState } from "$lib/lobbies/lobby-state.svelte";
	import { getLobbyStartPresentation } from "./lobby-waiting-presentation";

	let {
		lobby,
		actions,
		issueMessages
	}: {
		lobby: LobbyState;
		actions: LobbyActions;
		issueMessages: readonly string[];
	} = $props();

	const presentation = $derived(
		getLobbyStartPresentation({
			isHost: lobby.isHost,
			pending: lobby.isPending("startRound"),
			issueMessages
		})
	);
	const uid = $props.id();
	const descriptionId = `${uid}-start-description`;
</script>

<aside
	class="border-foreground bg-surface text-foreground fixed right-2 bottom-[max(0.5rem,env(safe-area-inset-bottom))] left-2 z-30 flex shrink-0 flex-col gap-3 rounded-2xl border-2 p-2.5 shadow-[0_-8px_24px_color-mix(in_srgb,var(--foreground)_12%,transparent)] min-[360px]:flex-row min-[360px]:items-center min-[360px]:justify-between min-[900px]:static min-[900px]:mt-2 sm:right-4 sm:left-4 sm:p-3"
	aria-labelledby={`${uid}-start-title`}
>
	<div class="flex min-w-0 items-center gap-3">
		<span
			class={[
				"grid size-10 shrink-0 place-items-center rounded-xl",
				presentation.kind === "ready"
					? "bg-accent text-accent-foreground"
					: presentation.kind === "blocked"
						? "bg-primary text-primary-foreground"
						: "bg-secondary text-secondary-foreground"
			]}
		>
			{#if presentation.kind === "ready"}
				<CheckCircle2 size={21} aria-hidden="true" />
			{:else if presentation.kind === "blocked"}
				<AlertTriangle size={21} aria-hidden="true" />
			{:else if presentation.kind === "pending"}
				<LoaderCircle class="animate-spin motion-reduce:animate-none" size={21} aria-hidden="true" />
			{:else}
				<Clock3 size={21} aria-hidden="true" />
			{/if}
		</span>

		<div class="min-w-0">
			<h2 id={`${uid}-start-title`} class="m-0 text-sm leading-tight font-black">{presentation.title}</h2>
			<p id={descriptionId} class="mt-1 mb-0 line-clamp-2 text-xs leading-4 opacity-70 sm:line-clamp-1">
				{presentation.description}
			</p>
		</div>
	</div>

	{#if presentation.buttonLabel}
		<Button
			class="w-full shrink-0 min-[360px]:w-auto"
			disabled={!presentation.canStart}
			aria-describedby={descriptionId}
			onclick={() => void actions.startRound()}
		>
			{#if presentation.kind === "pending"}
				<LoaderCircle class="animate-spin motion-reduce:animate-none" size={18} aria-hidden="true" />
			{:else}
				<Play size={18} aria-hidden="true" />
			{/if}
			{presentation.buttonLabel}
		</Button>
	{/if}

	{#if issueMessages.length > 0}
		<ul class="sr-only">
			{#each issueMessages as message, index (`${index}:${message}`)}
				<li>{message}</li>
			{/each}
		</ul>
	{/if}
</aside>
