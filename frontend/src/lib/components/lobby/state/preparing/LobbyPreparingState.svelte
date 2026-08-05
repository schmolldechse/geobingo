<script lang="ts">
	import { LobbyStatus, type LobbySnapshot } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
	import { onMount } from "svelte";

	const CENTISECOND_MS = 10;

	let { snapshot }: { snapshot: LobbySnapshot } = $props();

	function getRemainingCentiseconds(endsAtMs: number, nowMs: number): number {
		return Math.max(0, Math.ceil((endsAtMs - nowMs) / CENTISECOND_MS));
	}

	const preparation = $derived.by(() => {
		const preparingEndsAt = snapshot.preparingEndsAt;
		const currentRound = snapshot.currentRound;

		if (snapshot.status !== LobbyStatus.PREPARING || !preparingEndsAt || !currentRound) {
			throw new Error("LobbyPreparingState requires a complete PREPARING snapshot.");
		}

		const endsAtMs = new Date(preparingEndsAt).getTime();

		if (!Number.isFinite(endsAtMs)) {
			throw new Error("LobbyPreparingState requires a valid preparation deadline.");
		}

		return {
			endsAtMs,
			roundNumber: currentRound.roundNumber,
			modeDisplayName: snapshot.selectedMode.displayName
		};
	});

	let nowMs = $state(Date.now());

	const remainingCentiseconds = $derived(getRemainingCentiseconds(preparation.endsAtMs, nowMs));
	const countdownValue = $derived((remainingCentiseconds / 100).toFixed(2));
	const remainingWholeSeconds = $derived(Math.ceil(remainingCentiseconds / 100));
	const countdownLabel = $derived(
		remainingCentiseconds === 0
			? "The countdown reached zero"
			: `${remainingWholeSeconds} ${remainingWholeSeconds === 1 ? "second" : "seconds"} until the round begins`
	);

	onMount(() => {
		let animationFrame: number;
		let lastRenderedCentiseconds = remainingCentiseconds;

		const updateCountdown = () => {
			const currentTime = Date.now();
			const nextCentiseconds = getRemainingCentiseconds(preparation.endsAtMs, currentTime);

			if (nextCentiseconds !== lastRenderedCentiseconds) {
				lastRenderedCentiseconds = nextCentiseconds;
				nowMs = currentTime;
			}

			animationFrame = window.requestAnimationFrame(updateCountdown);
		};

		updateCountdown();

		return () => window.cancelAnimationFrame(animationFrame);
	});
</script>

<main
	class="preparing-screen bg-background text-foreground relative grid min-h-dvh place-items-center overflow-hidden px-4 py-7 text-center sm:h-dvh sm:min-h-0 sm:px-6 sm:py-8"
	aria-label={`Preparing Round ${preparation.roundNumber} in ${preparation.modeDisplayName}`}
	aria-busy="true"
	data-lobby-preparing-state
>
	<div
		class="border-secondary/20 pointer-events-none absolute -top-28 -left-24 size-72 rotate-[-17deg] border-2 sm:-top-44 sm:-left-36 sm:size-[26rem]"
		style="border-radius: 48% 52% 67% 33% / 35% 37% 63% 65%"
		aria-hidden="true"
	></div>
	<div
		class="border-accent/15 pointer-events-none absolute -right-36 -bottom-44 size-80 rounded-full border-[2.75rem] sm:-right-44 sm:-bottom-60 sm:size-[32rem] sm:border-[4.5rem]"
		aria-hidden="true"
	></div>

	<article class="launch-stage relative z-10 grid w-full max-w-4xl justify-items-center">
		<header class="round-context">
			<h1 class="round-number">Round {preparation.roundNumber}</h1>
			<p class="round-mode">{preparation.modeDisplayName}</p>
		</header>

		<div class="timer-stage">
			<div class="timer-shell" role="timer" aria-label={countdownLabel}>
				<span class="countdown-value" aria-hidden="true">{countdownValue}</span>
				<span class="countdown-unit" aria-hidden="true">seconds</span>
			</div>
		</div>
	</article>
</main>

<style>
	.launch-stage {
		min-width: 0;
	}

	.round-context {
		display: grid;
		justify-items: center;
		gap: 0.42rem;
	}

	.round-number {
		margin: 0;
		color: var(--primary);
		font-family: "Fredoka Variable", Fredoka, ui-rounded, sans-serif;
		font-size: clamp(2.3rem, 6vw, 3.4rem);
		font-weight: 650;
		letter-spacing: -0.04em;
		line-height: 0.95;
	}

	.round-mode {
		margin: 0;
		color: var(--secondary);
		font-size: clamp(0.92rem, 2.2vw, 1.05rem);
		font-weight: 850;
		letter-spacing: 0.035em;
	}

	.timer-stage {
		display: grid;
		width: min(100%, 48rem);
		min-height: clamp(12rem, 35vh, 20rem);
		margin-top: clamp(0.85rem, 2.5vh, 1.35rem);
		place-items: center;
	}

	.timer-shell {
		display: grid;
		min-width: 0;
		justify-items: center;
	}

	.countdown-value {
		display: block;
		width: 4.2ch;
		color: var(--primary);
		font-family: "Fredoka Variable", Fredoka, ui-rounded, sans-serif;
		font-size: clamp(6.5rem, 24vw, 12rem);
		font-feature-settings: "tnum" 1;
		font-variant-numeric: tabular-nums;
		font-weight: 660;
		letter-spacing: -0.08em;
		line-height: 0.78;
		text-align: center;
		white-space: nowrap;
	}

	.countdown-unit {
		margin-top: 0.7rem;
		color: var(--muted);
		font-size: 0.65rem;
		font-weight: 850;
		letter-spacing: 0.16em;
		text-transform: uppercase;
	}

	@media (max-height: 720px) and (min-width: 640px) {
		.preparing-screen {
			padding-block: 1rem;
		}

		.timer-stage {
			min-height: 10rem;
			margin-top: 0.35rem;
		}

		.countdown-value {
			font-size: 7.5rem;
		}

		.countdown-unit {
			margin-top: 0.4rem;
		}
	}

	@media (max-height: 640px) and (max-width: 639px) {
		.preparing-screen {
			padding-block: 1rem;
		}

		.timer-stage {
			min-height: 9rem;
			margin-top: 0.35rem;
		}

		.countdown-value {
			font-size: 6rem;
		}

		.countdown-unit {
			margin-top: 0.35rem;
		}
	}
</style>
