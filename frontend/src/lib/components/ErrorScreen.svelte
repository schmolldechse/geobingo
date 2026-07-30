<script lang="ts">
	import ArrowLeft from "@lucide/svelte/icons/arrow-left";
	import BrandMark from "$lib/components/BrandMark.svelte";
	import Button from "$lib/components/ui/Button.svelte";

	type BaseProps = {
		status: number;
		message: string;
		actionLabel: string;
	};

	type Props = BaseProps &
		(
			| {
					actionHref: string;
					onaction?: never;
			  }
			| {
					actionHref?: never;
					onaction: () => void;
			  }
		);

	let { status, message, actionLabel, actionHref, onaction }: Props = $props();
</script>

<main
	class="bg-background text-foreground relative grid min-h-dvh place-content-center justify-items-center gap-7 overflow-x-hidden px-4 py-8 text-center sm:h-dvh sm:min-h-0 sm:gap-9 sm:overflow-hidden sm:px-6 sm:py-10"
	aria-labelledby="error-heading"
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

	<article class="relative z-10 w-full max-w-3xl">
		<p class="error-status text-primary m-0 leading-[0.74] font-[680] tracking-[-0.075em]">
			<span class="sr-only">Error status </span>{status}
		</p>

		<h1
			id="error-heading"
			class="mx-auto mt-8 mb-0 max-w-2xl text-[clamp(1.75rem,9vw,3rem)] leading-[1.05] font-semibold tracking-[-0.045em] sm:mt-10"
		>
			{message}
		</h1>

		{#if actionHref !== undefined}
			<Button href={actionHref} size="lg" class="mt-8 w-full max-w-80 sm:mt-9 sm:w-auto">
				<ArrowLeft aria-hidden="true" size={18} strokeWidth={2.25} />
				{actionLabel}
			</Button>
		{:else}
			<Button size="lg" class="mt-8 w-full max-w-80 sm:mt-9 sm:w-auto" onclick={onaction}>
				<ArrowLeft aria-hidden="true" size={18} strokeWidth={2.25} />
				{actionLabel}
			</Button>
		{/if}
	</article>
</main>

<style>
	.error-status {
		font-family: "Fredoka Variable", Fredoka, ui-rounded, sans-serif;
		font-size: clamp(7.5rem, min(42vw, 28dvh), 17.5rem);
	}
</style>
