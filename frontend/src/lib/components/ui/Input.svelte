<script module lang="ts">
	type InputValue = string | number;

	export type { InputValue };
</script>

<script lang="ts">
	import { onDestroy } from "svelte";
	import type { ClassValue, HTMLInputAttributes } from "svelte/elements";

	type Props = {
		type?: "text" | "number" | "range" | "search";
		value?: InputValue;
		debounceTime?: number;
		onchange?: (value: InputValue) => void;
		rangeMinLabel?: string;
		rangeMaxLabel?: string;
		class?: ClassValue;
	} & Omit<HTMLInputAttributes, "type" | "value" | "onchange" | "oninput">;

	let {
		type = "text",
		value = $bindable(""),
		debounceTime = 0,
		onchange,
		rangeMinLabel,
		rangeMaxLabel,
		class: className,
		...restProps
	}: Props = $props();

	let debounceTimer: ReturnType<typeof setTimeout> | undefined = $state(undefined);

	function toNumber(value: unknown): number | undefined {
		if (value == null || value === "") return undefined;
		const numericValue = Number(value);
		return Number.isFinite(numericValue) ? numericValue : undefined;
	}

	const rangeMin = $derived(toNumber(restProps.min) ?? 0);
	const rangeMax = $derived(toNumber(restProps.max) ?? 100);
	const effectiveRangeValue = $derived.by(() => {
		const fallbackValue = rangeMin + (rangeMax - rangeMin) / 2;
		const numericValue = toNumber(value) ?? fallbackValue;

		if (rangeMax <= rangeMin) return rangeMin;
		return Math.min(rangeMax, Math.max(rangeMin, numericValue));
	});
	const rangeProgress = $derived(rangeMax > rangeMin ? ((effectiveRangeValue - rangeMin) / (rangeMax - rangeMin)) * 100 : 0);
	const effectiveRangeMinLabel = $derived(rangeMinLabel ?? String(rangeMin));
	const effectiveRangeMaxLabel = $derived(rangeMaxLabel ?? String(rangeMax));

	function clearDebounce(): void {
		if (!debounceTimer) return;
		clearTimeout(debounceTimer);
		debounceTimer = undefined;
	}

	function getValue(input: HTMLInputElement): InputValue {
		if (type !== "number" && type !== "range") return input.value;
		if (type === "number" && input.value === "") return "";

		const rawValue = input.valueAsNumber;
		if (Number.isNaN(rawValue)) return input.value;
		const min = toNumber(restProps.min);
		const max = toNumber(restProps.max);
		if (min !== undefined && max !== undefined && max <= min) return min;
		return Math.min(max ?? rawValue, Math.max(min ?? rawValue, rawValue));
	}

	function scheduleChange(): void {
		clearDebounce();
		if (debounceTime <= 0) {
			onchange?.(value);
			return;
		}

		debounceTimer = setTimeout(() => {
			debounceTimer = undefined;
			onchange?.(value);
		}, debounceTime);
	}

	function handleInput(event: Event): void {
		value = getValue(event.currentTarget as HTMLInputElement);
		scheduleChange();
	}

	onDestroy(clearDebounce);
</script>

{#if type === "range"}
	<div class="grid w-full gap-1" data-range-root data-disabled={restProps.disabled ? "" : undefined}>
		<input
			{...restProps}
			{type}
			{value}
			oninput={handleInput}
			class={[
				"range-input min-h-11 w-full cursor-pointer appearance-none bg-transparent p-0 outline-none disabled:cursor-not-allowed disabled:opacity-50",
				className
			]}
			style:--range-progress={`${rangeProgress}%`}
			data-input
			data-type={type}
		/>
		<div class="text-muted flex items-center justify-between gap-3 text-xs font-bold" data-range-labels aria-hidden="true">
			<span>{effectiveRangeMinLabel}</span>
			<span class="text-right">{effectiveRangeMaxLabel}</span>
		</div>
	</div>
{:else}
	<input
		{...restProps}
		{type}
		{value}
		oninput={handleInput}
		class={[
			"border-border bg-surface text-foreground placeholder:text-muted focus:border-secondary min-h-11 w-full rounded-xl border-2 px-3 py-2 text-sm font-semibold transition-colors outline-none disabled:cursor-not-allowed disabled:opacity-50 motion-reduce:transition-none",
			className
		]}
		data-input
		data-type={type}
	/>
{/if}

<style>
	input[type="search"]::-webkit-search-cancel-button,
	input[type="search"]::-webkit-search-decoration {
		appearance: none;
	}

	.range-input {
		--range-track-height: 0.625rem;
		--range-track-border-width: 2px;
		--range-thumb-size: 1.375rem;
	}

	.range-input::-webkit-slider-runnable-track {
		height: var(--range-track-height);
		border: var(--range-track-border-width) solid var(--foreground);
		border-radius: 999px;
		background: linear-gradient(
			to right,
			var(--secondary) 0,
			var(--secondary) var(--range-progress),
			var(--surface-muted) var(--range-progress),
			var(--surface-muted) 100%
		);
	}

	.range-input::-webkit-slider-thumb {
		width: var(--range-thumb-size);
		height: var(--range-thumb-size);
		margin-top: calc((var(--range-track-height) - var(--range-thumb-size)) / 2 - var(--range-track-border-width));
		appearance: none;
		border: 2px solid var(--foreground);
		border-radius: 999px;
		background: var(--accent);
		box-shadow: 0 1px 2px color-mix(in srgb, var(--foreground) 28%, transparent);
	}

	.range-input::-moz-range-track {
		height: var(--range-track-height);
		border: var(--range-track-border-width) solid var(--foreground);
		border-radius: 999px;
		background: linear-gradient(
			to right,
			var(--secondary) 0,
			var(--secondary) var(--range-progress),
			var(--surface-muted) var(--range-progress),
			var(--surface-muted) 100%
		);
	}

	.range-input::-moz-range-thumb {
		width: var(--range-thumb-size);
		height: var(--range-thumb-size);
		border: 2px solid var(--foreground);
		border-radius: 999px;
		background: var(--accent);
		box-shadow: 0 1px 2px color-mix(in srgb, var(--foreground) 28%, transparent);
	}

	.range-input:focus-visible {
		outline: 2px solid var(--ring);
		outline-offset: 2px;
	}

	[data-range-root][data-disabled] [data-range-labels] {
		opacity: 0.5;
	}
</style>
