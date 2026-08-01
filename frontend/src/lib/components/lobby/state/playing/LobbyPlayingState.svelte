<script lang="ts">
	import Clock3 from "@lucide/svelte/icons/clock-3";
	import Gamepad2 from "@lucide/svelte/icons/gamepad-2";
	import type { LobbySnapshot } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import type { LobbyState } from "$lib/lobbies/lobby-state.svelte";
	import CaptureChallenge from "./gamemodes/capturechallenge/CaptureChallenge.svelte";

	let {
		lobby,
		snapshot,
		actions
	}: {
		lobby: LobbyState;
		snapshot: LobbySnapshot;
		actions: LobbyActions;
	} = $props();

	const personalProjection = $derived(lobby.personalProjection);
	const projectionsSynchronized = $derived(
		Boolean(
			personalProjection &&
			personalProjection.lobbyId === snapshot.lobbyId &&
			personalProjection.stateVersion === snapshot.stateVersion
		)
	);
	const modeKey = $derived(snapshot.gameMode.modeKey);
	const captureChallenge = $derived(snapshot.gameMode.captureChallenge ?? null);
	const personalCaptureChallenge = $derived(personalProjection?.captureChallenge ?? null);
</script>

{#if !projectionsSynchronized}
	<main
		class="bg-background text-foreground grid min-h-dvh place-items-center p-4 text-center"
		aria-busy="true"
		data-lobby-playing-synchronizing
	>
		<section class="border-border bg-surface grid max-w-md justify-items-center gap-3 rounded-2xl border-2 p-6">
			<Clock3 class="text-primary animate-pulse motion-reduce:animate-none" size={32} aria-hidden="true" />
			<h1 class="m-0 font-[Fredoka_Variable] text-2xl font-[650]">Synchronizing the round</h1>
			<p class="text-muted m-0 text-sm">Waiting for your current player state.</p>
		</section>
	</main>
{:else if modeKey === "capture_challenge" && captureChallenge}
	<CaptureChallenge {lobby} {snapshot} capture={captureChallenge} personalCapture={personalCaptureChallenge} {actions} />
{:else}
	<main
		class="bg-background text-foreground grid min-h-dvh place-items-center p-4 text-center"
		data-lobby-playing-state
		data-mode-key={modeKey}
	>
		<section
			class="border-foreground bg-surface grid max-w-md justify-items-center gap-3 rounded-2xl border-2 p-6 shadow-[4px_4px_0_var(--foreground)]"
		>
			<Gamepad2 class="text-secondary" size={36} aria-hidden="true" />
			<div>
				<p class="text-secondary m-0 text-[0.65rem] font-black tracking-[0.14em] uppercase">Round in progress</p>
				<h1 class="mt-1 mb-0 font-[Fredoka_Variable] text-3xl font-[650]">{snapshot.selectedMode.displayName}</h1>
			</div>
			<p class="text-muted m-0 text-sm leading-6">The playing interface for this game mode is not available yet.</p>
		</section>
	</main>
{/if}
