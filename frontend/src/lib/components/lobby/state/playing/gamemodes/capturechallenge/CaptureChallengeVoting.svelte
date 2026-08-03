<script lang="ts">
	import Camera from "@lucide/svelte/icons/camera";
	import Check from "@lucide/svelte/icons/check";
	import Clock3 from "@lucide/svelte/icons/clock-3";
	import Eye from "@lucide/svelte/icons/eye";
	import MapPinned from "@lucide/svelte/icons/map-pinned";
	import ThumbsDown from "@lucide/svelte/icons/thumbs-down";
	import ThumbsUp from "@lucide/svelte/icons/thumbs-up";
	import UserRoundCheck from "@lucide/svelte/icons/user-round-check";
	import { onMount } from "svelte";
	import Avatar from "$lib/components/ui/Avatar.svelte";
	import GoogleMaps, { type GoogleMapsController } from "$lib/components/ui/google-maps";
	import {
		VoteValue,
		type CaptureChallengePersonalProjection,
		type CaptureChallengePublicProjection
	} from "$lib/generated/realtime/GeoBingo.Contracts.GameModes.CaptureChallenge";
	import type { LobbyMemberView, LobbySnapshot } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import type { LobbyState } from "$lib/lobbies/lobby-state.svelte";

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
		personalCapture: CaptureChallengePersonalProjection;
		actions: LobbyActions;
	} = $props();

	let googleMaps = $state<GoogleMapsController | null>(null as GoogleMapsController | null);
	let nowMs = $state(Date.now());

	const currentCapture = $derived(personalCapture.currentCapture ?? null);
	const owner = $derived.by((): LobbyMemberView | null => {
		if (!currentCapture) return null;
		return snapshot.members.find((member) => member.userId === currentCapture.ownerUserId) ?? null;
	});
	const ownerName = $derived(owner?.displayName ?? (currentCapture?.isOwner ? "You" : "Former player"));
	const ownerHandle = $derived(owner ? `@${owner.handle}` : currentCapture?.isOwner ? "Your capture" : "Round participant");
	const ownerInitials = $derived(createInitials(ownerName));
	const slotEndsAtMs = $derived.by(() => {
		const parsed = capture.currentCaptureEndsAt ? new Date(capture.currentCaptureEndsAt).getTime() : Number.NaN;
		return Number.isFinite(parsed) ? parsed : null;
	});
	const slotClosed = $derived(slotEndsAtMs === null || nowMs >= slotEndsAtMs);
	const remainingSeconds = $derived(slotEndsAtMs === null ? 0 : Math.max(0, Math.ceil((slotEndsAtMs - nowMs) / 1000)));
	const countdown = $derived(formatCountdown(remainingSeconds));
	const totalCaptures = $derived(capture.releasedCaptureCount);
	const completedCaptures = $derived(Math.max(0, (currentCapture?.sequence ?? totalCaptures + 1) - 1));
	const sequenceProgress = $derived(totalCaptures === 0 ? 100 : Math.min(100, (completedCaptures / totalCaptures) * 100));
	const mutationPending = $derived(
		Boolean(
			currentCapture &&
			(lobby.isPending(`castVote:${currentCapture.captureId}`) || lobby.isPending(`changeVote:${currentCapture.captureId}`))
		)
	);
	const controlsDisabled = $derived(
		!currentCapture || currentCapture.isOwner || slotClosed || lobby.connectionStatus !== "connected" || mutationPending
	);
	const panoramaUnavailable = $derived(googleMaps?.streetViewState === "unavailable");
	const voteAnnouncement = $derived.by(() => {
		if (!currentCapture || currentCapture.isOwner) return "";
		if (mutationPending) return "Saving your vote.";
		if (currentCapture.selectedValue === VoteValue.GOOD) return "Good vote saved.";
		if (currentCapture.selectedValue === VoteValue.BAD) return "Bad vote saved.";
		return "No vote submitted for this capture yet.";
	});

	function formatCountdown(totalSeconds: number): string {
		const minutes = Math.floor(totalSeconds / 60);
		const seconds = totalSeconds % 60;
		return `${minutes.toString().padStart(2, "0")}:${seconds.toString().padStart(2, "0")}`;
	}

	function formatScoreFactor(value: number): string {
		return `×${Number.isInteger(value) ? value.toFixed(1) : value}`;
	}

	function createInitials(name: string): string {
		const parts = name.trim().split(/\s+/).filter(Boolean);
		return (parts.length > 1 ? `${parts[0][0]}${parts.at(-1)?.[0] ?? ""}` : (parts[0]?.slice(0, 2) ?? "?")).toUpperCase();
	}

	function submitVote(value: VoteValue): void {
		if (!currentCapture || controlsDisabled || currentCapture.selectedValue === value) return;
		if (currentCapture.selectedValue === undefined || currentCapture.selectedValue === null) {
			void actions.castVote(currentCapture.captureId, value);
			return;
		}

		void actions.changeVote(currentCapture.captureId, value);
	}

	onMount(() => {
		const interval = window.setInterval(() => {
			nowMs = Date.now();
		}, 250);

		return () => window.clearInterval(interval);
	});
</script>

<main
	class="bg-background text-foreground relative h-dvh min-h-[520px] overflow-hidden p-1.5 min-[621px]:p-2 min-[901px]:p-3"
	data-capture-challenge-voting
>
	<section
		class="border-foreground bg-surface relative isolate h-full min-h-0 overflow-hidden rounded-[20px] border-2 shadow-[3px_3px_0_var(--foreground)] min-[621px]:rounded-[23px] min-[901px]:rounded-[28px] min-[901px]:shadow-[5px_5px_0_var(--foreground)]"
		aria-label="Capture Challenge voting phase"
	>
		{#if currentCapture}
			{#key currentCapture.captureId}
				<GoogleMaps
					bind:controller={googleMaps}
					initialView="street-view"
					initialCamera={{ center: currentCapture.position, zoom: 18 }}
					initialStreetView={{
						latitude: currentCapture.position.latitude,
						longitude: currentCapture.position.longitude,
						heading: currentCapture.position.heading,
						pitch: currentCapture.position.pitch,
						zoom: currentCapture.position.zoom
					}}
					showUnavailableMapAction={false}
					mapOptions={{
						gestureHandling: "none",
						keyboardShortcuts: false,
						clickableIcons: false,
						disableDefaultUi: true
					}}
					streetViewOptions={{
						clickToGo: false,
						scrollwheel: true,
						motionTracking: false,
						controls: {
							address: false,
							close: false,
							fullscreen: false,
							imageDate: false,
							links: false,
							motionTracking: false,
							pan: true,
							zoom: true
						}
					}}
					class="absolute inset-0 h-full min-h-0 rounded-none border-0"
					aria-label={`Fixed Street View capture submitted by ${ownerName} for ${currentCapture.goal.title}`}
				/>
			{/key}

			<div
				class="pointer-events-none absolute inset-0 z-[2] bg-[linear-gradient(to_bottom,rgb(5_14_18/0.18),transparent_24%,transparent_66%,rgb(12_13_12/0.18))]"
				aria-hidden="true"
			></div>

			{#if panoramaUnavailable}
				<div class="bg-surface absolute inset-0 z-20 grid place-items-center p-6 text-center" role="status">
					<div class="grid max-w-sm justify-items-center gap-3">
						<MapPinned class="text-primary" size={36} aria-hidden="true" />
						<h1 class="m-0 font-[Fredoka_Variable] text-2xl font-[650]">Capture unavailable</h1>
						<p class="text-muted m-0 text-sm leading-6">Street View imagery is unavailable near this capture location.</p>
					</div>
				</div>
			{/if}
		{:else}
			<div class="bg-surface-muted absolute inset-0 grid place-items-center p-6 text-center">
				<div class="grid max-w-md justify-items-center gap-3">
					<Check class="text-secondary" size={42} strokeWidth={2.7} aria-hidden="true" />
					<p class="text-secondary m-0 text-[0.65rem] font-black tracking-[0.14em] uppercase">Capture Challenge</p>
					<h1 class="m-0 font-[Fredoka_Variable] text-3xl font-[650]">Voting complete</h1>
					<p class="text-muted m-0 text-sm">Calculating the round results…</p>
				</div>
			</div>
		{/if}

		<div
			class="border-foreground bg-surface/95 pointer-events-none absolute top-[11px] left-[11px] z-30 inline-flex min-h-[38px] items-center gap-2 rounded-[14px] border-2 px-2 py-1.5 shadow-[3px_3px_0_var(--foreground)] backdrop-blur-lg min-[621px]:top-[13px] min-[621px]:left-[13px] min-[901px]:top-[18px] min-[901px]:left-[18px] min-[901px]:min-h-[42px] min-[901px]:gap-2.5 min-[901px]:px-2.5"
			aria-label="Capture Challenge voting phase"
		>
			<span
				class="bg-secondary grid size-[22px] grid-cols-2 place-content-center gap-0.5 rounded-lg min-[901px]:size-[25px]"
				aria-hidden="true"
			>
				<span class="bg-accent size-1.5 rounded-[2px]"></span><span class="bg-accent size-1.5 rounded-[2px]"></span>
				<span class="bg-accent size-1.5 rounded-[2px]"></span><span class="bg-accent size-1.5 rounded-[2px]"></span>
			</span>
			<span class="text-[0.57rem] font-black tracking-[0.1em] uppercase max-[390px]:sr-only min-[901px]:text-[0.7rem]">
				Capture Challenge · Voting
			</span>
		</div>

		<div
			class="border-foreground bg-primary text-primary-foreground pointer-events-none absolute top-[11px] right-[11px] z-30 grid min-w-[123px] grid-cols-[auto_auto] grid-rows-[auto_auto] items-center rounded-[15px] border-2 px-2.5 py-2 shadow-[3px_3px_0_var(--foreground)] min-[621px]:top-[13px] min-[621px]:right-[13px] min-[901px]:top-[18px] min-[901px]:right-[18px] min-[901px]:min-w-[158px] min-[901px]:rounded-[18px] min-[901px]:px-3.5 min-[901px]:py-2.5 min-[901px]:shadow-[4px_4px_0_var(--foreground)]"
			role="timer"
			aria-label={`${remainingSeconds} seconds left for the current capture`}
		>
			<Clock3 class="row-span-2 mr-2 size-[18px] min-[901px]:size-[22px]" aria-hidden="true" />
			<span class="text-[0.48rem] leading-none font-black tracking-[0.15em] uppercase opacity-85 min-[901px]:text-[0.58rem]"
				>Time left</span
			>
			<span class="font-[Fredoka_Variable] text-xl leading-none font-[650] tracking-[0.02em] tabular-nums min-[901px]:text-2xl">
				{countdown}
			</span>
		</div>

		{#if currentCapture}
			<div
				class="pointer-events-none absolute top-[22px] left-1/2 z-20 inline-flex -translate-x-1/2 items-center gap-2 rounded-full border border-black/20 bg-white/90 px-3 py-2 text-[0.68rem] font-bold text-[#3c3933] shadow-[0_3px_12px_rgb(36_33_28/0.14)] max-[900px]:hidden"
				aria-hidden="true"
			>
				<Eye size={14} />
				Fixed Street View · pan and zoom only
			</div>

			<section
				class="border-foreground bg-surface/96 absolute right-2.5 bottom-[calc(2rem+env(safe-area-inset-bottom,0px))] left-2.5 z-30 grid min-h-0 grid-cols-[minmax(0,0.8fr)_minmax(0,1.2fr)] gap-0 rounded-[18px] border-2 p-2.5 shadow-[3px_3px_0_var(--foreground),0_10px_32px_rgb(36_33_28/0.2)] backdrop-blur-xl max-[390px]:grid-cols-1 min-[621px]:right-3.5 min-[621px]:bottom-10 min-[621px]:left-3.5 min-[621px]:rounded-[20px] min-[621px]:p-3 min-[901px]:right-[clamp(18px,3vw,46px)] min-[901px]:bottom-12 min-[901px]:left-[clamp(18px,3vw,46px)] min-[901px]:mx-auto min-[901px]:max-w-[1140px] min-[901px]:grid-cols-[minmax(210px,0.8fr)_minmax(285px,1.35fr)_auto] min-[901px]:rounded-[23px] min-[901px]:shadow-[5px_5px_0_var(--foreground),0_16px_46px_rgb(36_33_28/0.22)]"
				aria-labelledby="current-capture-heading"
			>
				<div
					class="grid min-w-0 grid-cols-[auto_minmax(0,1fr)] items-center gap-2 py-1 pr-2 pl-0.5 max-[390px]:flex max-[390px]:pb-2 min-[621px]:gap-3 min-[621px]:pr-3 min-[901px]:px-2 min-[901px]:pr-[18px]"
				>
					<div class="relative size-[42px] shrink-0 min-[621px]:size-[54px] min-[901px]:size-[62px]">
						<Avatar
							src={owner?.avatarUrl}
							alt=""
							class="border-foreground size-full border-2 font-[Fredoka_Variable] text-sm font-[650] min-[901px]:text-lg"
						>
							{#snippet fallback()}<span aria-hidden="true">{ownerInitials}</span>{/snippet}
						</Avatar>
						<span
							class="border-surface bg-secondary absolute right-0 bottom-0 size-3 rounded-full border-2 min-[901px]:size-[15px]"
							aria-hidden="true"
						></span>
					</div>
					<div class="min-w-0">
						<p
							class="text-secondary m-0 text-[0.48rem] font-black tracking-[0.12em] uppercase min-[621px]:text-[0.58rem] min-[901px]:text-[0.61rem]"
						>
							{currentCapture.isOwner ? "Your submission" : "Submitted by"}
						</p>
						<h2
							class="m-0 overflow-hidden font-[Fredoka_Variable] text-[0.88rem] font-[650] text-ellipsis whitespace-nowrap min-[621px]:text-base min-[901px]:text-lg"
						>
							{ownerName}
						</h2>
						<p
							class="text-muted m-0 overflow-hidden text-[0.62rem] font-semibold text-ellipsis whitespace-nowrap max-[620px]:hidden min-[901px]:text-[0.72rem]"
						>
							{ownerHandle}
						</p>
					</div>
				</div>

				<div
					class="border-border grid min-w-0 content-center border-l px-2.5 py-1 max-[390px]:border-t max-[390px]:border-l-0 max-[390px]:px-0 max-[390px]:py-2 min-[621px]:px-4 min-[901px]:border-r min-[901px]:px-6"
				>
					<div class="mb-1 flex items-center justify-between gap-2">
						<span
							class="text-muted inline-flex items-center gap-1.5 text-[0.51rem] font-black tracking-[0.07em] uppercase min-[621px]:text-[0.62rem] min-[901px]:text-[0.66rem]"
						>
							<Camera class="text-primary max-[620px]:hidden" size={14} aria-hidden="true" />
							Capture {currentCapture.sequence} of {totalCaptures}
						</span>
						<span
							class="border-foreground bg-accent text-accent-foreground rounded-full border px-2 py-0.5 text-[0.58rem] font-black max-[620px]:hidden"
						>
							{formatScoreFactor(currentCapture.goal.scoreFactor)}
						</span>
					</div>
					<h1
						id="current-capture-heading"
						class="m-0 line-clamp-2 font-[Fredoka_Variable] text-[0.88rem] leading-[1.08] font-[650] tracking-[-0.02em] min-[621px]:text-base min-[901px]:line-clamp-1 min-[901px]:text-[clamp(1.05rem,1.8vw,1.42rem)]"
					>
						{currentCapture.goal.title}
					</h1>
					<div
						class="mt-1.5 flex items-center gap-1.5 min-[901px]:mt-2.5 min-[901px]:gap-2.5"
						aria-label={`${completedCaptures} of ${totalCaptures} captures completed`}
					>
						<div
							class="border-foreground bg-surface-muted h-[5px] flex-1 overflow-hidden rounded-full border min-[901px]:h-[7px]"
						>
							<div
								class="bg-accent h-full rounded-full transition-[width] motion-reduce:transition-none"
								style:width={`${sequenceProgress}%`}
							></div>
						</div>
						<span
							class="text-muted min-w-8 text-right text-[0.54rem] font-black tabular-nums min-[901px]:min-w-12 min-[901px]:text-[0.65rem]"
						>
							{completedCaptures} / {totalCaptures}
						</span>
					</div>
				</div>

				{#if currentCapture.isOwner}
					<div
						class="col-span-2 grid min-h-[50px] min-w-0 place-items-center border-t border-[var(--border)] px-3 py-2 text-center min-[901px]:col-span-1 min-[901px]:min-w-[220px] min-[901px]:border-t-0 min-[901px]:pl-4"
					>
						<div class="grid justify-items-center gap-1">
							<UserRoundCheck class="text-secondary" size={25} aria-hidden="true" />
							<p class="m-0 text-xs font-black min-[901px]:text-sm">Your capture is being voted on.</p>
						</div>
					</div>
				{:else}
					<div
						class="col-span-2 grid min-w-0 grid-cols-2 gap-2 border-t border-[var(--border)] pt-2 min-[901px]:col-span-1 min-[901px]:min-w-[250px] min-[901px]:border-t-0 min-[901px]:pt-0 min-[901px]:pl-4"
						aria-label="Vote for this capture"
					>
						<button
							type="button"
							class="border-foreground bg-primary text-primary-foreground grid min-h-[50px] cursor-pointer grid-cols-[auto_auto] place-content-center place-items-center gap-2 rounded-[13px] border-2 px-3 py-2 font-black shadow-[2px_2px_0_var(--foreground)] transition-[transform,box-shadow,filter] hover:brightness-105 active:translate-0.5 active:shadow-[1px_1px_0_var(--foreground)] disabled:cursor-not-allowed disabled:opacity-60 motion-reduce:transition-none min-[901px]:min-h-[82px] min-[901px]:grid-cols-1 min-[901px]:gap-1 min-[901px]:rounded-2xl min-[901px]:shadow-[3px_3px_0_var(--foreground)]"
							class:ring-4={currentCapture.selectedValue === VoteValue.BAD}
							class:ring-accent={currentCapture.selectedValue === VoteValue.BAD}
							disabled={controlsDisabled || currentCapture.selectedValue === VoteValue.BAD}
							onclick={() => submitVote(VoteValue.BAD)}
							aria-pressed={currentCapture.selectedValue === VoteValue.BAD}
						>
							<ThumbsDown size={23} strokeWidth={2.6} aria-hidden="true" />
							<span class="text-[0.65rem] tracking-[0.08em] uppercase">Bad</span>
						</button>
						<button
							type="button"
							class="border-foreground bg-secondary text-secondary-foreground grid min-h-[50px] cursor-pointer grid-cols-[auto_auto] place-content-center place-items-center gap-2 rounded-[13px] border-2 px-3 py-2 font-black shadow-[2px_2px_0_var(--foreground)] transition-[transform,box-shadow,filter] hover:brightness-105 active:translate-0.5 active:shadow-[1px_1px_0_var(--foreground)] disabled:cursor-not-allowed disabled:opacity-60 motion-reduce:transition-none min-[901px]:min-h-[82px] min-[901px]:grid-cols-1 min-[901px]:gap-1 min-[901px]:rounded-2xl min-[901px]:shadow-[3px_3px_0_var(--foreground)]"
							class:ring-4={currentCapture.selectedValue === VoteValue.GOOD}
							class:ring-accent={currentCapture.selectedValue === VoteValue.GOOD}
							disabled={controlsDisabled || currentCapture.selectedValue === VoteValue.GOOD}
							onclick={() => submitVote(VoteValue.GOOD)}
							aria-pressed={currentCapture.selectedValue === VoteValue.GOOD}
						>
							<ThumbsUp size={23} strokeWidth={2.6} aria-hidden="true" />
							<span class="text-[0.65rem] tracking-[0.08em] uppercase">Good</span>
						</button>
					</div>
				{/if}
			</section>

			{#if !currentCapture.isOwner && currentCapture.selectedValue !== undefined && currentCapture.selectedValue !== null}
				<div
					class="border-foreground bg-accent text-accent-foreground absolute left-1/2 z-30 flex -translate-x-1/2 items-center gap-2 rounded-full border-2 px-3 py-2 text-[0.65rem] font-black shadow-[3px_3px_0_var(--foreground)] max-[620px]:bottom-[calc(12rem+env(safe-area-inset-bottom,0px))] min-[621px]:bottom-[219px] min-[901px]:bottom-[184px]"
				>
					<Check size={16} strokeWidth={2.6} aria-hidden="true" />
					{currentCapture.selectedValue === VoteValue.GOOD ? "Good" : "Bad"} vote saved
				</div>
			{/if}
		{/if}

		<p class="sr-only" aria-live="polite">{voteAnnouncement}</p>
	</section>
</main>
