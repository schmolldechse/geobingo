<script lang="ts">
	import type { LobbyModeSummaryModel } from "./lobby-waiting-presentation";

	let { summary }: { summary: LobbyModeSummaryModel } = $props();

	const uid = $props.id();
	const titleId = `${uid}-mode-title`;
</script>

<section
	class="border-foreground bg-surface flex min-h-0 flex-col overflow-hidden rounded-2xl border-2 shadow-[var(--shadow-paper)]"
	aria-labelledby={titleId}
>
	<header class="bg-secondary text-secondary-foreground relative overflow-hidden p-4 sm:p-5 lg:p-6">
		<div
			class="border-secondary-foreground pointer-events-none absolute -right-16 -bottom-24 size-56 rounded-full border-[1.75rem] opacity-[0.08]"
			aria-hidden="true"
		></div>
		<div class="relative z-10">
			<p class="m-0 text-[0.64rem] font-black tracking-[0.14em] uppercase opacity-70">{summary.eyebrow}</p>
			<h2
				id={titleId}
				class="mt-2 mb-0 max-w-[12ch] font-[Fredoka_Variable] text-[clamp(1.8rem,4vw,2.75rem)] leading-[0.95] font-[650] tracking-[-0.035em]"
			>
				{summary.name}
			</h2>
			<p class="mt-3 mb-0 max-w-[34rem] text-sm leading-6 opacity-75">{summary.description}</p>
		</div>
	</header>

	<div class="min-h-0 overflow-y-auto">
		{#if summary.metrics.length > 0}
			<dl class="grid grid-cols-1 gap-2 p-3 min-[420px]:grid-cols-3 lg:grid-cols-1 xl:grid-cols-3">
				{#each summary.metrics as metric (metric.label)}
					<div class="border-border bg-background/35 min-w-0 rounded-xl border-2 p-3">
						<dt class="text-muted m-0 text-xs font-extrabold">{metric.label}</dt>
						<dd class="mt-1 mb-0 text-base leading-tight font-black">{metric.value}</dd>
					</div>
				{/each}
			</dl>
		{/if}
	</div>
</section>
