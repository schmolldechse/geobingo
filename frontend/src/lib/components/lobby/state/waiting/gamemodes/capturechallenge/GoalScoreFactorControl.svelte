<script lang="ts">
	import Minus from "@lucide/svelte/icons/minus";
	import Plus from "@lucide/svelte/icons/plus";
	import Button from "$lib/components/ui/Button.svelte";

	const MINIMUM_GOAL_SCORE_FACTOR = 0;
	const MAXIMUM_GOAL_SCORE_FACTOR = 10;
	const GOAL_SCORE_FACTOR_STEP = 0.5;

	type Props = {
		value?: number;
		disabled?: boolean;
		labelledby?: string;
		onchange?: (value: number) => void;
	};
	let { value = 1, disabled = false, labelledby, onchange }: Props = $props();

	const factorFormatter = new Intl.NumberFormat("en-US", {
		minimumFractionDigits: 1,
		maximumFractionDigits: 1
	});
	const formattedValue = $derived(factorFormatter.format(value));

	const change = (direction: -1 | 1): void => {
		onchange?.(getNextGoalScoreFactor(value, direction));
	};

	const getNextGoalScoreFactor = (current: number, direction: -1 | 1): number => {
		const next = Math.round((current + direction * GOAL_SCORE_FACTOR_STEP) * 2) / 2;
		return Math.min(MAXIMUM_GOAL_SCORE_FACTOR, Math.max(MINIMUM_GOAL_SCORE_FACTOR, next));
	};
</script>

<div
	class="border-foreground bg-surface-muted grid w-full max-w-56 grid-cols-[auto_minmax(5rem,1fr)_auto] items-center gap-1.5 rounded-xl border-2 p-1.5"
	role="group"
	aria-label={labelledby ? undefined : "Score factor"}
	aria-labelledby={labelledby}
	data-score-factor-control
>
	<Button
		size="icon"
		variant="ghost"
		class="text-foreground size-10 min-h-10 rounded-lg p-0"
		aria-label="Decrease score factor"
		title="Decrease by 0.5"
		disabled={disabled || value <= MINIMUM_GOAL_SCORE_FACTOR}
		onclick={() => change(-1)}
	>
		<Minus size={18} strokeWidth={3} aria-hidden="true" />
	</Button>

	<output
		class="border-foreground bg-accent text-accent-foreground grid min-h-10 place-items-center rounded-lg border-2 px-3 font-[Fredoka_Variable] text-base font-semibold tabular-nums"
		aria-live="polite"
		aria-atomic="true"
	>
		× {formattedValue}
	</output>

	<Button
		size="icon"
		variant="ghost"
		class="text-foreground size-10 min-h-10 rounded-lg p-0"
		aria-label="Increase score factor"
		title="Increase by 0.5"
		disabled={disabled || value >= MAXIMUM_GOAL_SCORE_FACTOR}
		onclick={() => change(1)}
	>
		<Plus size={18} strokeWidth={3} aria-hidden="true" />
	</Button>
</div>
