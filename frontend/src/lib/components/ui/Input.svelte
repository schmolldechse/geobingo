<script module lang="ts">
	type InputValue = string | number;

	export type { InputValue };
</script>

<script lang="ts">
	import { onDestroy } from "svelte";
	import type { ClassValue, HTMLInputAttributes } from "svelte/elements";

	type Props = {
		type?: "text" | "number" | "search";
		value?: InputValue;
		debounceTime?: number;
		onchange?: (value: InputValue) => void;
		class?: ClassValue;
	} & Omit<HTMLInputAttributes, "type" | "value" | "onchange" | "oninput">;

	let { type = "text", value = $bindable(""), debounceTime = 0, onchange, class: className, ...restProps }: Props = $props();

	let debounceTimer: ReturnType<typeof setTimeout> | undefined = $state(undefined);

	function toNumber(value: unknown): number | undefined {
		if (value == null || value === "") return undefined;
		const numericValue = Number(value);
		return Number.isFinite(numericValue) ? numericValue : undefined;
	}

	function clearDebounce(): void {
		if (!debounceTimer) return;
		clearTimeout(debounceTimer);
		debounceTimer = undefined;
	}

	function getValue(input: HTMLInputElement): InputValue {
		if (type !== "number") return input.value;
		if (input.value === "") return "";

		const rawValue = input.valueAsNumber;
		if (Number.isNaN(rawValue)) return input.value;
		const min = toNumber(restProps.min);
		const max = toNumber(restProps.max);
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

<input
	{...restProps}
	{type}
	{value}
	oninput={handleInput}
	class={[
		"border-border bg-surface text-foreground placeholder:text-muted focus:border-secondary min-h-11 w-full rounded-xl border-2 px-3 py-2 text-sm font-semibold transition-colors outline-none disabled:cursor-not-allowed disabled:opacity-50",
		className
	]}
	data-input
	data-type={type}
/>

<style>
	input[type="search"]::-webkit-search-cancel-button,
	input[type="search"]::-webkit-search-decoration {
		appearance: none;
	}
</style>
