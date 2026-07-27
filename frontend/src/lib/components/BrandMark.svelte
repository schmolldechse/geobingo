<script lang="ts">
	type Props = {
		showWordmark?: boolean;
	};

	let { showWordmark = true }: Props = $props();
	let viewportWidth = $state(0);

	const wordmarkVisible = $derived(showWordmark && viewportWidth >= 640);
</script>

<svelte:window bind:innerWidth={viewportWidth} />

<span class="inline-flex items-center gap-2.5">
	<svg id="c" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" class="size-10 shrink-0">
		<defs>
			<style>
				.g {
					fill: var(--color-primary);
				}
				.h {
					fill: var(--color-secondary);
				}
				.i {
					fill: var(--color-accent);
				}
			</style>
		</defs>
		<g id="d">
			<g id="e">
				<path class="g" d="M11.75,23h-7.5v-9c0-5.376,4.374-9.75,9.75-9.75h9v7.5h-9c-1.241,0-2.25,1.01-2.25,2.25v9Z" />
				<path class="h" d="M23,59.75h-9c-5.376,0-9.75-4.374-9.75-9.75v-9h7.5v9c0,1.24,1.009,2.25,2.25,2.25h9v7.5Z" />
				<path class="g" d="M50,59.75h-9v-7.5h9c1.241,0,2.25-1.01,2.25-2.25v-9h7.5v9c0,5.376-4.374,9.75-9.75,9.75Z" />
				<path class="h" d="M59.75,23h-7.5v-9c0-1.24-1.009-2.25-2.25-2.25h-9v-7.5h9c5.376,0,9.75,4.374,9.75,9.75v9Z" />
			</g>
			<circle class="i" cx="32" cy="32" r="6" />
		</g>
	</svg>

	{#if wordmarkVisible}
		<span class="text-xl font-extrabold tracking-tight" aria-label="GeoBingo">
			<span class="text-secondary">Geo</span><span class="text-primary">Bingo</span>
		</span>
	{/if}
</span>
