<script lang="ts">
	import ArrowLeft from "@lucide/svelte/icons/arrow-left";
	import Clock3 from "@lucide/svelte/icons/clock-3";
	import Map from "@lucide/svelte/icons/map";
	import { onMount } from "svelte";
	import GoogleMaps, { type GoogleMapsController } from "$lib/components/ui/google-maps";
	import type {
		CaptureChallengePersonalProjection,
		CaptureChallengePublicProjection
	} from "$lib/generated/realtime/GeoBingo.Contracts.GameModes.CaptureChallenge";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import type { LobbyState } from "$lib/lobbies/lobby-state.svelte";
	import CaptureActionDock from "./CaptureActionDock.svelte";
	import CaptureGoalsPanel from "./CaptureGoalsPanel.svelte";

	let {
		lobby,
		capture,
		personalCapture,
		actions
	}: {
		lobby: LobbyState;
		capture: CaptureChallengePublicProjection;
		personalCapture: CaptureChallengePersonalProjection;
		actions: LobbyActions;
	} = $props();

	// Assigned by Svelte's component binding; TypeScript cannot observe that assignment in script control flow.
	let googleMaps = $state<GoogleMapsController | null>(null as GoogleMapsController | null);
	let selectedGoalId = $state<string | null>(null);
	let sheetOpen = $state(false);
	let nowMs = $state(Date.now());

	const slots = $derived(
		[...personalCapture.captureSlots].sort((left, right) => left.goal.displayOrder - right.goal.displayOrder)
	);
	const selectedSlot = $derived(slots.find((slot) => slot.goal.goalId === selectedGoalId) ?? slots[0] ?? null);
	const captureEndsAtMs = $derived.by(() => {
		const parsed = capture.captureEndsAt ? new Date(capture.captureEndsAt).getTime() : Number.NaN;
		return Number.isFinite(parsed) ? parsed : null;
	});
	const captureDeadlineReached = $derived(captureEndsAtMs === null || nowMs >= captureEndsAtMs);
	const remainingSeconds = $derived(captureEndsAtMs === null ? 0 : Math.max(0, Math.ceil((captureEndsAtMs - nowMs) / 1000)));
	const countdown = $derived(formatCountdown(remainingSeconds));
	const mapActive = $derived(googleMaps?.view !== "street-view");

	function formatCountdown(totalSeconds: number): string {
		const hours = Math.floor(totalSeconds / 3600);
		const minutes = Math.floor((totalSeconds % 3600) / 60);
		const seconds = totalSeconds % 60;

		return hours > 0
			? `${hours.toString().padStart(2, "0")}:${minutes.toString().padStart(2, "0")}:${seconds.toString().padStart(2, "0")}`
			: `${minutes.toString().padStart(2, "0")}:${seconds.toString().padStart(2, "0")}`;
	}

	function selectGoal(goalId: string): void {
		selectedGoalId = goalId;
		sheetOpen = false;
	}

	$effect(() => {
		if (slots.length === 0) {
			selectedGoalId = null;
			return;
		}

		if (!selectedGoalId || !slots.some((slot) => slot.goal.goalId === selectedGoalId)) {
			selectedGoalId = slots[0].goal.goalId;
		}
	});

	onMount(() => {
		const interval = window.setInterval(() => {
			nowMs = Date.now();
		}, 250);

		return () => window.clearInterval(interval);
	});
</script>

<main
	class="bg-background text-foreground relative h-dvh min-h-[520px] overflow-hidden p-[7px] max-[700px]:h-svh min-[701px]:p-3"
	data-capture-challenge-capturing
>
	<section
		class="border-foreground bg-surface relative isolate h-full min-h-[360px] overflow-hidden rounded-[21px] border-2 shadow-[3px_3px_0_var(--foreground)] min-[701px]:rounded-[26px] min-[701px]:shadow-[5px_5px_0_var(--foreground)]"
		aria-label="Google Maps and Street View capture area"
	>
		<GoogleMaps
			bind:controller={googleMaps}
			initialView="map"
			initialCamera={{ center: { latitude: 20, longitude: 0 }, zoom: 2 }}
			mapOptions={{
				gestureHandling: "greedy",
				mapType: "roadmap",
				controls: {
					camera: false,
					fullscreen: false,
					mapType: false,
					rotate: false,
					scale: false,
					streetView: true,
					zoom: true,
					positions: { streetView: "left-center", zoom: "left-center" }
				}
			}}
			streetViewOptions={{
				clickToGo: true,
				showRoadLabels: true,
				controls: { address: true, close: false, fullscreen: false, links: true, pan: true, zoom: true }
			}}
			class="absolute inset-0 h-full min-h-0 rounded-[19px] border-0 min-[701px]:rounded-[24px]"
			aria-label="Choose a map location and use Pegman to enter Street View"
		/>

		<div
			class="border-foreground bg-primary text-primary-foreground pointer-events-none absolute top-3 left-1/2 z-30 grid min-w-[132px] -translate-x-1/2 grid-cols-[auto_auto] grid-rows-[auto_auto] items-center rounded-[15px] border-2 px-3 py-2 shadow-[3px_3px_0_var(--foreground)] min-[701px]:top-[18px] min-[701px]:min-w-[154px] min-[701px]:rounded-[18px] min-[701px]:px-[15px] min-[701px]:pt-2 min-[701px]:pb-[9px] min-[701px]:shadow-[4px_4px_0_var(--foreground)]"
			role="timer"
			aria-label={`${remainingSeconds} seconds remaining`}
		>
			<Clock3 class="row-span-2 mr-2" size={20} aria-hidden="true" />
			<span class="text-[0.56rem] leading-none font-black tracking-[0.15em] uppercase opacity-85">Time left</span>
			<span class="font-[Fredoka_Variable] text-xl leading-none font-[650] tracking-[0.02em] tabular-nums min-[701px]:text-2xl">
				{countdown}
			</span>
		</div>

		{#if mapActive}
			<div
				class="pointer-events-none absolute top-[84px] left-1/2 z-10 inline-flex -translate-x-1/2 items-center gap-2 rounded-full border border-black/20 bg-white/90 px-3 py-2 text-[0.68rem] font-bold text-[#3c3933] shadow-[0_3px_12px_rgb(36_33_28/0.14)] max-[700px]:top-[69px] max-[700px]:text-[0.625rem] max-[460px]:hidden"
			>
				<Map size={15} aria-hidden="true" />
				Drag Pegman onto a street to start
			</div>
		{:else}
			<button
				type="button"
				class="pointer-events-auto absolute top-[84px] left-[18px] z-20 inline-flex min-h-10 cursor-pointer items-center gap-2 rounded-[10px] border border-black/30 bg-white px-3 text-xs font-bold text-[#333] shadow-[0_2px_8px_rgb(36_33_28/0.2)] max-[700px]:top-[69px] max-[700px]:left-3"
				onclick={() => void googleMaps?.showMap().catch(() => undefined)}
			>
				<ArrowLeft size={16} aria-hidden="true" />
				Back to map
			</button>
		{/if}

		<CaptureGoalsPanel
			{slots}
			selectedGoalId={selectedSlot?.goal.goalId ?? null}
			{sheetOpen}
			onselect={selectGoal}
			ontoggle={() => (sheetOpen = !sheetOpen)}
		/>

		{#if selectedSlot}
			<CaptureActionDock {lobby} {actions} slot={selectedSlot} {googleMaps} {captureDeadlineReached} {sheetOpen} />
		{/if}
	</section>
</main>
