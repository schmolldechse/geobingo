<script lang="ts">
	import Gauge from "@lucide/svelte/icons/gauge";
	import type { LobbySnapshot } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import type { LobbyState } from "$lib/lobbies/lobby-state.svelte";
	import CaptureChallengeSettings from "./gamemodes/capturechallenge/CaptureChallengeSettings.svelte";

	let {
		lobby,
		snapshot,
		actions
	}: {
		lobby: LobbyState;
		snapshot: LobbySnapshot;
		actions: LobbyActions;
	} = $props();
</script>

{#if snapshot.gameMode.modeKey === "capture_challenge" && snapshot.gameMode.captureChallenge}
	<CaptureChallengeSettings {lobby} capture={snapshot.gameMode.captureChallenge} {actions} />
{:else}
	<section
		class="border-foreground bg-surface grid min-h-56 place-items-center rounded-2xl border-2 p-6 text-center shadow-[var(--shadow-paper-raised)] min-[900px]:h-full"
		aria-labelledby="unsupported-mode-settings-title"
	>
		<div class="max-w-md">
			<span class="bg-secondary/15 text-secondary mx-auto grid size-12 place-items-center rounded-2xl">
				<Gauge size={24} aria-hidden="true" />
			</span>
			<p class="text-primary mt-4 mb-0 text-[0.65rem] font-black tracking-[0.15em] uppercase">Game mode settings</p>
			<h2 id="unsupported-mode-settings-title" class="mt-1 mb-0 text-xl font-black">
				{snapshot.selectedMode.displayName}
			</h2>
			<p class="text-muted mt-2 mb-0 text-sm leading-6">{snapshot.selectedMode.description}</p>
			<p class="text-muted mt-4 mb-0 text-xs font-bold">No configurable settings are available for this mode yet.</p>
		</div>
	</section>
{/if}
