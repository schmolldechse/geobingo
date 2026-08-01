<script lang="ts">
	import Check from "@lucide/svelte/icons/check";
	import type { CaptureChallengeCaptureSlotProjection } from "$lib/generated/realtime/GeoBingo.Contracts.GameModes.CaptureChallenge";

	let {
		slots,
		selectedGoalId,
		roundNumber,
		sheetOpen,
		onselect,
		ontoggle
	}: {
		slots: CaptureChallengeCaptureSlotProjection[];
		selectedGoalId: string | null;
		roundNumber: number;
		sheetOpen: boolean;
		onselect: (goalId: string) => void;
		ontoggle: () => void;
	} = $props();

	const capturedCount = $derived(slots.filter(isCaptured).length);

	function isCaptured(slot: CaptureChallengeCaptureSlotProjection): boolean {
		return Boolean(slot.captureId && slot.position);
	}

	function stateLabel(slot: CaptureChallengeCaptureSlotProjection): string {
		const selected = slot.goal.goalId === selectedGoalId;
		const captured = isCaptured(slot);

		if (selected) return captured ? "Current · submitted" : "Current goal";
		return captured ? "Capture submitted" : "Not captured";
	}
</script>

<aside
	class={[
		"border-foreground bg-surface/95 pointer-events-auto absolute z-20 grid border-2 p-4 shadow-[4px_4px_0_var(--foreground)] backdrop-blur-xl transition-[height] duration-200 motion-reduce:transition-none",
		"top-[18px] right-[18px] bottom-[18px] w-[min(324px,calc(100vw-36px))] grid-rows-[auto_minmax(0,1fr)] rounded-[22px]",
		"max-[700px]:top-auto max-[700px]:right-3 max-[700px]:bottom-3 max-[700px]:left-3 max-[700px]:w-auto max-[700px]:grid-rows-[auto_auto_minmax(0,1fr)] max-[700px]:rounded-[21px] max-[700px]:p-[8px_10px_10px] max-[700px]:shadow-[3px_3px_0_var(--foreground)]",
		sheetOpen ? "max-[700px]:h-[min(54svh,430px)]" : "max-[700px]:h-[184px]"
	]}
	aria-labelledby="capture-goals-heading"
	data-capture-goals
	data-sheet-open={sheetOpen}
>
	<button
		type="button"
		class="hidden h-[22px] w-full cursor-pointer place-items-center border-0 bg-transparent before:block before:h-1 before:w-[42px] before:rounded-full before:bg-[var(--border)] before:content-[''] max-[700px]:grid"
		aria-label={sheetOpen ? "Collapse goals" : "Expand goals"}
		aria-expanded={sheetOpen}
		onclick={ontoggle}
	></button>

	<header class="flex items-end justify-between gap-3 px-0.5 pt-0.5 pb-3.5 max-[700px]:items-center max-[700px]:pb-2">
		<h1
			id="capture-goals-heading"
			class="m-0 font-[Fredoka_Variable] text-[1.56rem] leading-none font-[650] tracking-[-0.035em] max-[700px]:text-xl"
		>
			Goals
		</h1>
		<span
			class="border-foreground bg-accent text-accent-foreground inline-flex shrink-0 items-center gap-1 rounded-full border-2 px-2.5 py-1.5 text-[0.68rem] leading-none font-black"
		>
			{capturedCount}/{slots.length} captured
		</span>
	</header>

	<div
		class={[
			"grid content-start gap-2.5 overflow-y-auto pt-0.5 pr-1.5 pb-2 pl-0.5",
			"max-[700px]:flex max-[700px]:gap-2 max-[700px]:overflow-x-auto max-[700px]:overflow-y-hidden max-[700px]:scroll-smooth max-[700px]:pr-1 max-[700px]:pb-2",
			sheetOpen && "max-[700px]:grid max-[700px]:overflow-x-hidden max-[700px]:overflow-y-auto"
		]}
		role="list"
		aria-label="Round goals"
	>
		{#each slots as slot, index (slot.goal.goalId)}
			{@const selected = slot.goal.goalId === selectedGoalId}
			{@const captured = isCaptured(slot)}
			<button
				type="button"
				class={[
					"text-foreground relative grid min-h-[68px] w-full cursor-pointer grid-cols-[34px_minmax(0,1fr)_auto] items-center gap-2.5 rounded-[15px] border-2 p-2.5 text-left transition-[transform,background-color,border-color,box-shadow] duration-150 motion-reduce:transition-none",
					"border-border bg-surface-muted hover:border-foreground hover:-translate-y-px",
					selected && "border-foreground bg-secondary text-secondary-foreground shadow-[3px_3px_0_var(--foreground)]",
					"max-[700px]:min-h-16 max-[700px]:w-[min(72vw,252px)] max-[700px]:min-w-[min(72vw,252px)] max-[700px]:flex-none",
					sheetOpen && "max-[700px]:w-full max-[700px]:min-w-0"
				]}
				data-submitted={captured}
				aria-pressed={selected}
				onclick={() => onselect(slot.goal.goalId)}
			>
				{#if captured}
					<span
						class="border-foreground bg-accent text-accent-foreground grid size-8 place-items-center rounded-[10px] border-2"
						aria-label="Capture submitted"
					>
						<Check size={17} strokeWidth={3} aria-hidden="true" />
					</span>
				{:else}
					<span
						class="grid size-8 place-items-center rounded-[10px] border-2 border-current font-[Fredoka_Variable] text-sm font-[650]"
						aria-hidden="true"
					>
						{index + 1}
					</span>
				{/if}

				<span class="min-w-0">
					<span class="block truncate text-[0.8rem] leading-tight font-black">{slot.goal.title}</span>
					<span
						class={[
							"text-muted mt-1 block text-[0.625rem] leading-none font-bold tracking-[0.07em] uppercase",
							selected && "text-secondary-foreground/70"
						]}
					>
						{stateLabel(slot)}
					</span>
				</span>

				<span
					class={["bg-border size-2 rounded-full", captured && "bg-accent shadow-[0_0_0_2px_var(--foreground)]"]}
					aria-hidden="true"
				></span>
			</button>
		{/each}
	</div>
</aside>
