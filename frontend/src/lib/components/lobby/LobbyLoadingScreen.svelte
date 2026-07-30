<script lang="ts">
	import ArrowLeft from "@lucide/svelte/icons/arrow-left";
	import BrandMark from "$lib/components/BrandMark.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import type { LobbyConnectionStatus } from "$lib/lobbies/lobby-state.svelte";

	let {
		code,
		connectionStatus
	}: {
		code: string;
		connectionStatus: LobbyConnectionStatus;
	} = $props();
</script>

<main
	class="loading-screen bg-background text-foreground relative grid min-h-dvh place-content-center justify-items-center gap-6 overflow-x-hidden px-4 py-7 text-center sm:h-dvh sm:min-h-0 sm:gap-7 sm:overflow-hidden sm:px-6 sm:py-8"
	aria-labelledby="lobby-loading-heading"
	aria-describedby="lobby-loading-description"
	aria-busy="true"
	data-lobby-loading-screen
	data-connection-status={connectionStatus}
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

	<a href="/" class="relative z-10 rounded-xl no-underline" aria-label="GeoBingo home">
		<BrandMark alwaysShowWordmark />
	</a>

	<article class="relative z-10 grid w-full max-w-6xl justify-items-center">
		<p class="text-secondary m-0 mb-4 text-xs font-extrabold tracking-[0.17em] uppercase sm:mb-6">Lobby code</p>

		<div class="scan-frame" data-scan-frame>
			<span class="scan-corner scan-corner-top-left" aria-hidden="true"></span>
			<span class="scan-corner scan-corner-top-right" aria-hidden="true"></span>
			<span class="scan-corner scan-corner-bottom-left" aria-hidden="true"></span>
			<span class="scan-corner scan-corner-bottom-right" aria-hidden="true"></span>
			<span class="scan-glow" aria-hidden="true"></span>
			<p class="lobby-code text-primary">
				<span class="sr-only">Lobby code </span>{code}
			</p>
			<span class="scan-line" aria-hidden="true"></span>
		</div>

		<h1
			id="lobby-loading-heading"
			class="mt-7 mb-0 font-[Fredoka_Variable] text-[clamp(2rem,7.5vw,3.4rem)] leading-none font-[620] tracking-[-0.045em] sm:mt-9"
		>
			Finding your lobby
		</h1>
		<p
			id="lobby-loading-description"
			class="text-muted mt-4 mb-0 max-w-xl text-[clamp(0.95rem,2.5vw,1.05rem)] leading-6 sm:leading-7"
		>
			Hold on while we verify the code and load the latest players, settings and rounds.
		</p>

		<Button href="/" variant="outline" size="lg" class="mt-7 w-full max-w-80 sm:mt-9 sm:w-auto">
			<ArrowLeft aria-hidden="true" size={18} strokeWidth={2.25} />
			Back to home
		</Button>
	</article>
</main>

<style>
	.scan-frame {
		position: relative;
		display: grid;
		width: min(100%, 65rem);
		min-height: clamp(8rem, 22vw, 14rem);
		place-items: center;
		overflow: hidden;
		padding: clamp(1.4rem, 4vw, 2.75rem) clamp(0.4rem, 3vw, 2rem);
	}

	.scan-corner {
		position: absolute;
		width: clamp(1.5rem, 4vw, 2.6rem);
		aspect-ratio: 1;
		border-color: var(--foreground);
		border-style: solid;
	}

	.scan-corner-top-left {
		top: 0;
		left: 0;
		border-width: 3px 0 0 3px;
	}

	.scan-corner-top-right {
		top: 0;
		right: 0;
		border-width: 3px 3px 0 0;
	}

	.scan-corner-bottom-left {
		bottom: 0;
		left: 0;
		border-width: 0 0 3px 3px;
	}

	.scan-corner-bottom-right {
		right: 0;
		bottom: 0;
		border-width: 0 3px 3px 0;
	}

	.scan-glow {
		position: absolute;
		z-index: 0;
		inset: 20% 4%;
		background: linear-gradient(90deg, transparent, color-mix(in srgb, var(--secondary) 7%, transparent), transparent);
		filter: blur(1rem);
	}

	.lobby-code {
		position: relative;
		z-index: 1;
		max-width: 100%;
		margin: 0;
		font-family: "Fredoka Variable", Fredoka, ui-rounded, sans-serif;
		font-size: clamp(2rem, 9.4vw, 7.5rem);
		font-variant-numeric: tabular-nums;
		font-weight: 660;
		letter-spacing: 0.055em;
		line-height: 0.9;
		white-space: nowrap;
	}

	.scan-line {
		position: absolute;
		z-index: 2;
		top: 4%;
		left: 4%;
		width: 92%;
		height: 3px;
		border-radius: 9999px;
		background: var(--accent);
		box-shadow:
			0 0 0 1px color-mix(in srgb, var(--accent) 45%, transparent),
			0 0 1.5rem 0.4rem color-mix(in srgb, var(--accent) 38%, transparent);
		animation: scan 2.25s cubic-bezier(0.45, 0, 0.55, 1) infinite;
	}

	@keyframes scan {
		0%,
		100% {
			top: 4%;
			opacity: 0.35;
		}

		50% {
			top: calc(96% - 3px);
			opacity: 1;
		}
	}

	@media (max-width: 480px) {
		.scan-frame {
			width: calc(100vw - 2rem);
		}
	}

	@media (max-height: 700px) and (min-width: 640px) {
		.loading-screen {
			gap: 0.8rem;
			padding-block: 1rem;
		}

		.scan-frame {
			min-height: 7rem;
			padding-block: 1.2rem;
		}
	}

	@media (prefers-reduced-motion: reduce) {
		.scan-line {
			top: 50%;
			animation: none;
			opacity: 0.8;
		}
	}
</style>
