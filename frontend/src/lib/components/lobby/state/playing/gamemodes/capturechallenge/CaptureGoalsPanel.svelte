<script lang="ts">
	import Check from "@lucide/svelte/icons/check";
	import ChevronRight from "@lucide/svelte/icons/chevron-right";
	import type { CaptureChallengeCaptureSlotProjection } from "$lib/generated/realtime/GeoBingo.Contracts.GameModes.CaptureChallenge";

	let {
		slots,
		selectedGoalId,
		open,
		onselect,
		ontoggle
	}: {
		slots: CaptureChallengeCaptureSlotProjection[];
		selectedGoalId: string | null;
		open: boolean;
		onselect: (goalId: string) => void;
		ontoggle: () => void;
	} = $props();

	const factorFormatter = new Intl.NumberFormat("en-US", {
		minimumFractionDigits: 1,
		maximumFractionDigits: 1
	});
	const capturedCount = $derived(slots.filter(isCaptured).length);

	function isCaptured(slot: CaptureChallengeCaptureSlotProjection): boolean {
		return Boolean(slot.captureId && slot.position);
	}
</script>

<aside
	class={[
		"border-border bg-surface/95 border-t-secondary pointer-events-auto absolute z-20 grid overflow-hidden border shadow-[0_18px_50px_rgb(0_0_0/0.18)] backdrop-blur-xl transition-[top,right,bottom,width,height,transform,border-radius] duration-200 motion-reduce:transition-none",
		"max-[700px]:top-auto max-[700px]:right-3 max-[700px]:bottom-3 max-[700px]:left-3 max-[700px]:w-auto max-[700px]:grid-rows-[auto_minmax(0,1fr)] max-[700px]:rounded-xl max-[700px]:border-t-4 max-[420px]:right-2 max-[420px]:bottom-2 max-[420px]:left-2",
		open
			? "max-[700px]:h-[166px] max-[420px]:h-[160px] min-[701px]:top-[18px] min-[701px]:right-[18px] min-[701px]:bottom-auto min-[701px]:h-[calc(100%_-_36px)] min-[701px]:w-[min(324px,calc(100vw-36px))] min-[701px]:grid-rows-[auto_minmax(0,1fr)] min-[701px]:rounded-[10px] min-[701px]:border-t-4"
			: "max-[700px]:h-[68px] min-[701px]:top-1/2 min-[701px]:right-0 min-[701px]:bottom-auto min-[701px]:h-[198px] min-[701px]:w-[58px] min-[701px]:-translate-y-1/2 min-[701px]:grid-rows-1 min-[701px]:rounded-l-[10px] min-[701px]:border-r-0"
	]}
	aria-label="Goals panel"
	data-capture-goals
	data-panel-open={open}
>
	<header
		class={[
			"border-border grid min-h-[76px] grid-cols-[minmax(0,1fr)_auto] items-center gap-3 border-b px-3.5 py-3",
			"max-[700px]:min-h-[62px] max-[700px]:px-3 max-[700px]:py-2",
			!open && "max-[700px]:border-b-0 min-[701px]:hidden"
		]}
	>
		<div class="min-w-0">
			<h1
				id="capture-goals-heading"
				class="m-0 font-[Fredoka_Variable] text-[1.32rem] leading-none font-[650] tracking-[-0.035em] max-[700px]:text-lg"
			>
				Goals
			</h1>
			<p class="text-muted m-0 mt-1 text-[0.68rem] leading-none font-bold">
				{capturedCount} of {slots.length} captured
			</p>
		</div>

		<button
			type="button"
			class="border-border text-muted hover:bg-surface-muted hover:text-foreground focus-visible:outline-ring grid size-9 cursor-pointer place-items-center rounded-full border bg-transparent transition-colors focus-visible:outline-3 focus-visible:outline-offset-2"
			aria-label={open ? "Collapse goals" : "Expand goals"}
			aria-expanded={open}
			aria-controls="capture-goals-list"
			onclick={ontoggle}
		>
			<ChevronRight
				size={18}
				class={[
					"transition-transform duration-200 motion-reduce:transition-none",
					open ? "max-[700px]:-rotate-90" : "max-[700px]:rotate-90"
				]}
				aria-hidden="true"
			/>
		</button>
	</header>

	<div
		id="capture-goals-list"
		class={[
			"min-h-0 overflow-y-auto overscroll-contain",
			"max-[700px]:flex max-[700px]:snap-x max-[700px]:snap-proximity max-[700px]:overflow-x-auto max-[700px]:overflow-y-hidden max-[700px]:scroll-smooth max-[700px]:pb-1",
			!open && "hidden max-[700px]:hidden"
		]}
		role="list"
		aria-label="Round goals"
	>
		{#each slots as slot, index (slot.goal.goalId)}
			{@const selected = slot.goal.goalId === selectedGoalId}
			{@const captured = isCaptured(slot)}
			{@const formattedFactor = factorFormatter.format(slot.goal.scoreFactor)}
			<button
				type="button"
				class={[
					"border-border text-foreground hover:bg-secondary/10 focus-visible:outline-ring relative grid min-h-[68px] w-full cursor-pointer grid-cols-[32px_minmax(0,1fr)_auto] items-center gap-2.5 border-0 border-b bg-transparent px-3.5 py-2.5 text-left transition-colors last:border-b-0 focus-visible:z-10 focus-visible:outline-3 focus-visible:outline-offset-[-3px]",
					selected && "bg-primary/10",
					"max-[700px]:min-h-[92px] max-[700px]:w-[min(72vw,236px)] max-[700px]:min-w-[min(72vw,236px)] max-[700px]:flex-none max-[700px]:snap-start max-[700px]:border-r max-[700px]:border-b-0 max-[700px]:px-3 max-[700px]:last:border-r-0"
				]}
				data-goal-id={slot.goal.goalId}
				data-submitted={captured}
				aria-pressed={selected}
				onclick={() => onselect(slot.goal.goalId)}
			>
				{#if selected}
					<span class="bg-primary absolute top-2.5 bottom-2.5 left-0 w-[3px] rounded-r" aria-hidden="true"></span>
				{/if}

				{#if captured}
					<span class="bg-accent text-accent-foreground grid size-7 place-items-center rounded-full" aria-label="Captured">
						<Check size={16} strokeWidth={3} aria-hidden="true" />
					</span>
				{:else}
					<span
						class="border-border text-muted grid size-7 place-items-center rounded-full border font-[Fredoka_Variable] text-xs font-[650]"
						aria-hidden="true"
					>
						{index + 1}
					</span>
				{/if}

				<span class="min-w-0">
					<span class="block text-[0.78rem] leading-tight font-extrabold">{slot.goal.title}</span>
				</span>

				<span
					class="border-border text-muted rounded-full border px-1.5 py-1 text-[0.6rem] leading-none font-extrabold tabular-nums"
					aria-label={`Score factor ${formattedFactor}`}
				>
					× {formattedFactor}
				</span>
			</button>
		{/each}
	</div>

	<button
		type="button"
		class={[
			"text-foreground hover:bg-surface-muted focus-visible:outline-ring hidden h-full w-full cursor-pointer flex-col items-center justify-between gap-2 border-0 bg-transparent px-2 py-3 focus-visible:outline-3 focus-visible:outline-offset-[-4px]",
			!open && "min-[701px]:flex"
		]}
		aria-label="Expand goals"
		aria-expanded={open}
		aria-controls="capture-goals-list"
		onclick={ontoggle}
	>
		<span class="text-muted text-[0.58rem] leading-none font-extrabold tabular-nums">{capturedCount} / {slots.length}</span>
		<span
			class="rotate-180 font-[Fredoka_Variable] text-sm leading-none font-[650] tracking-[0.02em] [writing-mode:vertical-rl]"
		>
			Goals
		</span>
		<span class="border-border grid size-8 place-items-center rounded-full border" aria-hidden="true">
			<ChevronRight size={17} class="rotate-180" />
		</span>
	</button>
</aside>
