<script lang="ts">
	import Check from "@lucide/svelte/icons/check";
	import MapPin from "@lucide/svelte/icons/map-pin";
	import Save from "@lucide/svelte/icons/save";
	import Button from "$lib/components/ui/Button.svelte";
	import type { GoogleMapsController } from "$lib/components/ui/google-maps";
	import type { CaptureChallengeCaptureSlotProjection } from "$lib/generated/realtime/GeoBingo.Contracts.GameModes.CaptureChallenge";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import type { LobbyState } from "$lib/lobbies/lobby-state.svelte";
	import { sameCaptureStreetViewPosition, toCaptureStreetViewPosition } from "./capture-street-view";

	let {
		lobby,
		actions,
		slot,
		googleMaps,
		captureDeadlineReached,
		sheetOpen
	}: {
		lobby: LobbyState;
		actions: LobbyActions;
		slot: CaptureChallengeCaptureSlotProjection;
		googleMaps: GoogleMapsController | null;
		captureDeadlineReached: boolean;
		sheetOpen: boolean;
	} = $props();

	const slotIncompatible = $derived(Boolean(slot.captureId) !== Boolean(slot.position));
	const acceptedCapture = $derived(
		slot.captureId && slot.position
			? {
					captureId: slot.captureId,
					position: slot.position
				}
			: null
	);
	const draftPosition = $derived(toCaptureStreetViewPosition(googleMaps?.streetViewPosition ?? null));
	const submitOperationName = $derived(`submitCapture:${slot.goal.goalId}`);
	const updateOperationName = $derived(acceptedCapture ? `updateCapture:${acceptedCapture.captureId}` : "");
	const submitPending = $derived(lobby.isPending(submitOperationName));
	const updatePending = $derived(updateOperationName !== "" && lobby.isPending(updateOperationName));
	const mutationPending = $derived(submitPending || updatePending);
	const connectedAndOpen = $derived(lobby.connectionStatus === "connected" && !captureDeadlineReached);
	const streetViewReady = $derived(
		Boolean(googleMaps?.ready && googleMaps.view === "street-view" && googleMaps.streetViewState === "ready" && draftPosition)
	);
	const draftDiffersFromAccepted = $derived(
		draftPosition !== null && (!acceptedCapture || !sameCaptureStreetViewPosition(draftPosition, acceptedCapture.position))
	);
	const primaryDisabled = $derived(
		slotIncompatible || !connectedAndOpen || !streetViewReady || !draftDiffersFromAccepted || mutationPending
	);

	async function saveCapture(): Promise<void> {
		const position = draftPosition;
		if (primaryDisabled || !position) return;

		const positionSnapshot = { ...position };
		if (acceptedCapture) {
			await actions.updateCapture(acceptedCapture.captureId, positionSnapshot);
			return;
		}

		await actions.submitCapture(slot.goal.goalId, positionSnapshot);
	}
</script>

<section
	class={[
		"border-foreground bg-surface/95 pointer-events-auto absolute z-10 grid w-[min(520px,calc(100%_-_420px))] min-w-[380px] grid-cols-[minmax(0,1fr)_auto] items-center gap-4 rounded-[19px] border-2 px-3.5 py-3 shadow-[4px_4px_0_var(--foreground)] backdrop-blur-xl transition-[bottom,opacity] duration-200 motion-reduce:transition-none",
		"bottom-[22px] left-[calc(50%_-_84px)] -translate-x-1/2",
		"max-[980px]:left-[calc(50%_-_128px)] max-[980px]:w-[min(430px,calc(100%_-_360px))] max-[980px]:min-w-[330px]",
		"max-[700px]:right-3 max-[700px]:bottom-[207px] max-[700px]:left-3 max-[700px]:w-auto max-[700px]:min-w-0 max-[700px]:translate-x-0 max-[700px]:gap-2.5 max-[700px]:rounded-[17px] max-[700px]:p-2.5 max-[700px]:shadow-[3px_3px_0_var(--foreground)]",
		sheetOpen && "max-[700px]:pointer-events-none max-[700px]:opacity-0"
	]}
	aria-labelledby="active-capture-goal"
	data-capture-dock
>
	<div class="min-w-0">
		<p
			class="text-secondary m-0 mb-1 flex items-center gap-1.5 text-[0.625rem] leading-none font-black tracking-[0.12em] uppercase max-[700px]:text-[0.5rem]"
		>
			<MapPin size={13} aria-hidden="true" />
			Selected goal
		</p>
		<h2
			id="active-capture-goal"
			class="m-0 truncate font-[Fredoka_Variable] text-[1.06rem] leading-tight font-[650] tracking-[-0.02em] max-[700px]:text-sm"
		>
			{slot.goal.title}
		</h2>
	</div>

	<Button
		class="min-h-[46px] shrink-0 rounded-[13px] px-4 text-xs max-[700px]:min-h-[42px] max-[700px]:px-3 max-[700px]:text-[0.625rem]"
		disabled={primaryDisabled}
		onclick={() => void saveCapture()}
	>
		{#if mutationPending}
			Saving…
		{:else if slotIncompatible}
			Capture unavailable
		{:else if !streetViewReady}
			<Save size={17} aria-hidden="true" />
			<span><span class="max-[460px]:hidden">Enter Street View to </span>save</span>
		{:else if acceptedCapture && !draftDiffersFromAccepted}
			<Check size={17} aria-hidden="true" />
			Saved
		{:else}
			<Save size={17} aria-hidden="true" />
			{acceptedCapture ? "Update capture" : "Save capture"}
		{/if}
	</Button>
</section>
