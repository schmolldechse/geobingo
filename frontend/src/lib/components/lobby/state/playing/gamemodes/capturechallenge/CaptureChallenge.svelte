<script lang="ts">
	import Clock3 from "@lucide/svelte/icons/clock-3";
	import {
		CaptureChallengeStatus,
		type CaptureChallengePersonalProjection,
		type CaptureChallengePublicProjection
	} from "$lib/generated/realtime/GeoBingo.Contracts.GameModes.CaptureChallenge";
	import type { LobbySnapshot } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import type { LobbyState } from "$lib/lobbies/lobby-state.svelte";
	import CaptureChallengeCapturing from "./CaptureChallengeCapturing.svelte";
	import CaptureChallengeVoting from "./CaptureChallengeVoting.svelte";

	let {
		lobby,
		snapshot,
		capture,
		personalCapture,
		actions
	}: {
		lobby: LobbyState;
		snapshot: LobbySnapshot;
		capture: CaptureChallengePublicProjection;
		personalCapture: CaptureChallengePersonalProjection | null;
		actions: LobbyActions;
	} = $props();

	const status = $derived(capture.status === personalCapture?.status ? capture.status : null);
</script>

{#if status === CaptureChallengeStatus.CAPTURING && personalCapture}
	<CaptureChallengeCapturing {lobby} {snapshot} {capture} {personalCapture} {actions} />
{:else if status === CaptureChallengeStatus.VOTING && personalCapture}
	<CaptureChallengeVoting {lobby} {snapshot} {capture} {personalCapture} {actions} />
{:else}
	<main
		class="bg-background text-foreground grid min-h-dvh place-items-center p-4 text-center"
		aria-busy="true"
		data-capture-challenge-synchronizing
	>
		<section class="border-border bg-surface grid max-w-md justify-items-center gap-3 rounded-2xl border-2 p-6">
			<Clock3 class="text-primary animate-pulse motion-reduce:animate-none" size={32} aria-hidden="true" />
			<h1 class="m-0 font-[Fredoka_Variable] text-2xl font-[650]">Synchronizing the round</h1>
			<p class="text-muted m-0 text-sm">Waiting for your personal Capture Challenge state.</p>
		</section>
	</main>
{/if}
