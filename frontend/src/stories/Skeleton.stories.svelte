<script module lang="ts">
	import { defineMeta } from "@storybook/addon-svelte-csf";

	import Skeleton from "../lib/components/ui/Skeleton.svelte";

	const listRows = [1, 2, 3, 4];
	const cardRows = [1, 2, 3];

	const { Story } = defineMeta({
		title: "UI/Skeleton",
		component: Skeleton,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen"
		}
	});
</script>

<Story name="Grundformen" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Grundformen</h2>
			<p class="text-muted m-0 text-sm">Rechteck, abgerundete Fläche, Pill und Kreis.</p>
		</header>

		<div
			class="border-border bg-surface flex flex-wrap items-end gap-6 rounded-2xl border-2 p-6"
			aria-label="Inhalt wird geladen"
		>
			<span class="sr-only">Inhalt wird geladen</span>
			<div class="grid gap-2">
				<Skeleton class="h-20 w-32" />
				<span class="text-muted text-xs font-bold">Rechteck</span>
			</div>
			<div class="grid gap-2">
				<Skeleton class="h-20 w-32 rounded-xl" />
				<span class="text-muted text-xs font-bold">Abgerundet</span>
			</div>
			<div class="grid gap-2">
				<Skeleton class="h-4 w-32 rounded-full" />
				<span class="text-muted text-xs font-bold">Pill</span>
			</div>
			<div class="grid gap-2">
				<Skeleton class="size-20 rounded-full" />
				<span class="text-muted text-xs font-bold">Kreis</span>
			</div>
		</div>
	</section>
</Story>

<Story name="Avatar" asChild>
	<section class="p-6 sm:p-8">
		<div
			class="border-border bg-surface flex max-w-md items-center gap-4 rounded-2xl border-2 p-5"
			aria-label="Profil wird geladen"
		>
			<span class="sr-only">Profil wird geladen</span>
			<Skeleton class="size-12 shrink-0 rounded-full" />
			<div class="grid flex-1 gap-2">
				<Skeleton class="h-4 w-32 rounded-full" />
				<Skeleton class="h-3 w-48 max-w-full rounded-full" />
			</div>
		</div>
	</section>
</Story>

<Story name="Inhaltskarte" asChild>
	<section class="p-6 sm:p-8">
		<article
			class="border-foreground bg-surface grid max-w-sm gap-4 rounded-2xl border-2 p-5 shadow-[var(--shadow-paper)]"
			aria-label="Zielkarte wird geladen"
		>
			<span class="sr-only">Zielkarte wird geladen</span>
			<Skeleton class="aspect-[16/9] w-full rounded-xl" />
			<div class="grid gap-2">
				<Skeleton class="h-5 w-2/3 rounded-full" />
				<Skeleton class="h-3 w-full rounded-full" />
				<Skeleton class="h-3 w-5/6 rounded-full" />
			</div>
			<div class="flex gap-3 pt-1">
				<Skeleton class="h-11 flex-1 rounded-xl" />
				<Skeleton class="size-11 rounded-xl" />
			</div>
		</article>
	</section>
</Story>

<Story name="Liste" asChild>
	<section class="p-6 sm:p-8">
		<div
			class="divide-border border-border bg-surface grid max-w-xl divide-y-2 overflow-hidden rounded-2xl border-2"
			aria-label="Spielerliste wird geladen"
		>
			<span class="sr-only">Spielerliste wird geladen</span>
			{#each listRows as row}
				<div class="flex items-center gap-4 p-4">
					<Skeleton class="size-10 shrink-0 rounded-full" />
					<div class="grid flex-1 gap-2">
						<Skeleton class={["h-4 rounded-full", row % 2 === 1 ? "w-32" : "w-40"]} />
						<Skeleton class="h-3 w-24 rounded-full" />
					</div>
					<Skeleton class="h-7 w-16 rounded-full" />
				</div>
			{/each}
		</div>
	</section>
</Story>

<Story name="Seitenbereich" asChild>
	<section class="grid gap-7 p-6 sm:p-8" aria-label="Lobby wird geladen">
		<span class="sr-only">Lobby wird geladen</span>
		<header class="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
			<div class="grid gap-3">
				<Skeleton class="h-7 w-56 rounded-lg" />
				<Skeleton class="h-4 w-72 max-w-full rounded-full" />
			</div>
			<div class="flex gap-3">
				<Skeleton class="h-11 w-28 rounded-xl" />
				<Skeleton class="size-11 rounded-xl" />
			</div>
		</header>

		<div class="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
			{#each cardRows as card}
				<article class="border-border bg-surface grid gap-4 rounded-2xl border-2 p-5">
					<Skeleton class="h-36 w-full rounded-xl" />
					<div class="grid gap-2">
						<Skeleton class={["h-5 rounded-full", card === 2 ? "w-3/4" : "w-2/3"]} />
						<Skeleton class="h-3 w-full rounded-full" />
						<Skeleton class="h-3 w-4/5 rounded-full" />
					</div>
				</article>
			{/each}
		</div>
	</section>
</Story>
