<script module lang="ts">
	import { defineMeta } from "@storybook/addon-svelte-csf";
	import type { ComponentProps } from "svelte";

	import Separator, { type SeparatorOrientation } from "../lib/components/ui/Separator.svelte";

	type PlaygroundArgs = ComponentProps<typeof Separator>;

	const orientations: SeparatorOrientation[] = ["horizontal", "vertical"];

	const { Story } = defineMeta({
		title: "UI/Separator",
		component: Separator,
		render: playground,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen"
		},
		args: {
			orientation: "horizontal"
		},
		argTypes: {
			orientation: { control: "inline-radio", options: orientations }
		}
	});
</script>

{#snippet playground(args: PlaygroundArgs)}
	<div class={args.orientation === "vertical" ? "flex h-32 items-stretch p-8" : "p-8"}>
		<Separator {...args} />
	</div>
{/snippet}

<Story name="Playground" />

<Story name="Horizontal" asChild>
	<section class="grid max-w-2xl gap-4 p-6 sm:p-8">
		<div>
			<h2 class="text-foreground m-0 text-lg font-black">Lobby-Einstellungen</h2>
			<p class="text-muted mt-1 mb-0 text-sm">Sichtbarkeit und Spielregeln</p>
		</div>
		<Separator orientation="horizontal" />
		<div class="text-foreground grid gap-1 text-sm">
			<span class="font-extrabold">Öffentliche Lobby</span>
			<span class="text-muted">Alle Personen mit dem Code können beitreten.</span>
		</div>
	</section>
</Story>

<Story name="Vertikal" asChild>
	<section class="p-6 sm:p-8">
		<div class="border-border bg-surface flex h-20 max-w-xl items-stretch gap-5 rounded-2xl border-2 p-5">
			<div class="grid flex-1 content-center text-center">
				<strong class="text-foreground text-xl font-black">12</strong>
				<span class="text-muted text-xs font-bold">Spieler</span>
			</div>
			<Separator orientation="vertical" />
			<div class="grid flex-1 content-center text-center">
				<strong class="text-foreground text-xl font-black">3</strong>
				<span class="text-muted text-xs font-bold">Teams</span>
			</div>
			<Separator orientation="vertical" />
			<div class="grid flex-1 content-center text-center">
				<strong class="text-foreground text-xl font-black">90</strong>
				<span class="text-muted text-xs font-bold">Minuten</span>
			</div>
		</div>
	</section>
</Story>

<Story name="In Inhaltsgruppen" asChild>
	<section class="p-6 sm:p-8">
		<article
			class="border-foreground bg-surface grid max-w-2xl gap-5 rounded-2xl border-2 p-5 shadow-[var(--shadow-paper)] sm:p-6"
		>
			<header>
				<span class="text-primary text-xs font-extrabold tracking-widest uppercase">Spielübersicht</span>
				<h2 class="text-foreground mt-1 mb-0 text-xl font-black">Berlin-Mitte</h2>
			</header>

			<Separator orientation="horizontal" />

			<div class="grid gap-4 sm:grid-cols-[1fr_auto_1fr_auto_1fr] sm:items-stretch">
				<div>
					<strong class="text-foreground block text-sm">Start</strong>
					<span class="text-muted text-sm">Alexanderplatz</span>
				</div>
				<Separator orientation="vertical" class="hidden sm:block" />
				<div>
					<strong class="text-foreground block text-sm">Ziele</strong>
					<span class="text-muted text-sm">12 Orte</span>
				</div>
				<Separator orientation="vertical" class="hidden sm:block" />
				<div>
					<strong class="text-foreground block text-sm">Ende</strong>
					<span class="text-muted text-sm">18:30 Uhr</span>
				</div>
			</div>

			<Separator orientation="horizontal" />

			<p class="text-muted m-0 text-sm leading-6">Alle Teams erhalten dieselbe Reihenfolge der Ziele.</p>
		</article>
	</section>
</Story>

<Story name="Angepasst" asChild>
	<section class="grid gap-8 p-6 sm:p-8">
		<div class="grid max-w-2xl gap-3">
			<span class="text-foreground text-sm font-extrabold">Primäre, stärkere Trennlinie</span>
			<Separator orientation="horizontal" class="bg-primary! h-1! rounded-full" />
		</div>

		<div class="flex h-24 items-stretch gap-5">
			<span class="text-foreground self-center text-sm font-extrabold">Sekundär</span>
			<Separator orientation="vertical" class="bg-secondary! w-1! rounded-full" />
			<span class="text-muted self-center text-sm">Eigene Farbe und Stärke über die Klassen-API</span>
		</div>
	</section>
</Story>
