<script module lang="ts">
	import ArrowRight from "@lucide/svelte/icons/arrow-right";
	import Compass from "@lucide/svelte/icons/compass";
	import ExternalLink from "@lucide/svelte/icons/external-link";
	import Plus from "@lucide/svelte/icons/plus";
	import { defineMeta } from "@storybook/addon-svelte-csf";
	import type { ComponentProps } from "svelte";

	import Button, { type ButtonSize, type ButtonVariant } from "../lib/components/ui/Button.svelte";

	type PlaygroundArgs = Omit<ComponentProps<typeof Button>, "children"> & {
		label: string;
	};

	const variants: ButtonVariant[] = ["primary", "secondary", "accent", "outline", "ghost", "destructive"];
	const sizes: ButtonSize[] = ["sm", "md", "lg", "icon"];

	const { Story } = defineMeta({
		title: "UI/Button",
		component: Button,
		render: playground,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen"
		},
		args: {
			label: "Lobby erstellen",
			variant: "primary",
			size: "md",
			disabled: false
		},
		argTypes: {
			label: { control: "text", description: "Sichtbarer Inhalt der Beispiel-Schaltfläche" },
			variant: { control: "select", options: variants },
			size: { control: "select", options: sizes },
			disabled: { control: "boolean" },
			href: { control: "text" }
		}
	});
</script>

{#snippet playground({ label, ...args }: PlaygroundArgs)}
	<div class="flex min-h-40 items-center justify-center p-8">
		<Button {...args} aria-label={args.size === "icon" ? label : undefined}>
			{#if args.size === "icon"}
				<Compass aria-hidden="true" size={20} />
			{:else}
				{label}
			{/if}
		</Button>
	</div>
{/snippet}

<Story name="Playground" />

<Story name="Varianten" asChild>
	<section class="grid gap-4 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="m-0 text-xl font-black text-foreground">Varianten</h2>
			<p class="m-0 text-sm text-muted">Jede Variante im aktiven und deaktivierten Zustand.</p>
		</header>

		<div class="grid gap-3 rounded-2xl border-2 border-border bg-surface p-4 sm:p-6">
			{#each variants as variant}
				<div class="grid gap-3 border-b border-border pb-3 last:border-0 last:pb-0 sm:grid-cols-[7rem_1fr] sm:items-center">
					<span class="font-mono text-xs font-bold text-muted">{variant}</span>
					<div class="flex flex-wrap items-center gap-3">
						<Button {variant}>Aktiv</Button>
						<Button {variant} disabled>Deaktiviert</Button>
					</div>
				</div>
			{/each}
		</div>
	</section>
</Story>

<Story name="Größen" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="m-0 text-xl font-black text-foreground">Größen</h2>
			<p class="m-0 text-sm text-muted">Von der kompakten Aktion bis zur quadratischen Icon-Schaltfläche.</p>
		</header>

		<div class="flex flex-wrap items-center gap-4 rounded-2xl border-2 border-border bg-surface p-6">
			{#each sizes as size}
				<div class="grid justify-items-center gap-2">
					<Button {size} aria-label={size === "icon" ? "Kompass öffnen" : undefined}>
						{#if size === "icon"}
							<Compass aria-hidden="true" size={20} />
						{:else}
							{size.toUpperCase()}
						{/if}
					</Button>
					<code class="text-xs font-bold text-muted">{size}</code>
				</div>
			{/each}
		</div>
	</section>
</Story>

<Story name="Zustände" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="m-0 text-xl font-black text-foreground">Zustände</h2>
			<p class="m-0 text-sm text-muted">Aktive und nicht verfügbare Aktionen mit klarer visueller Hierarchie.</p>
		</header>

		<div class="grid max-w-2xl gap-4 rounded-2xl border-2 border-border bg-surface p-6 sm:grid-cols-2">
			<Button>Aktiv</Button>
			<Button disabled>Deaktiviert</Button>
			<Button variant="outline">Aktiv – Outline</Button>
			<Button variant="outline" disabled>Deaktiviert – Outline</Button>
			<Button variant="destructive">Löschen</Button>
			<Button variant="destructive" disabled>Löschen nicht möglich</Button>
		</div>
	</section>
</Story>

<Story name="Mit Icons" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="m-0 text-xl font-black text-foreground">Icon-Kompositionen</h2>
			<p class="m-0 text-sm text-muted">Führendes Icon, nachgestelltes Icon und zugängliche Icon-only-Aktion.</p>
		</header>

		<div class="flex flex-wrap items-center gap-4 rounded-2xl border-2 border-border bg-surface p-6">
			<Button><Plus aria-hidden="true" size={18} />Ziel hinzufügen</Button>
			<Button variant="secondary">Weiter<ArrowRight aria-hidden="true" size={18} /></Button>
			<Button variant="outline" href="#details">
				Details<ExternalLink aria-hidden="true" size={17} />
			</Button>
			<Button size="icon" variant="accent" aria-label="Kompass öffnen">
				<Compass aria-hidden="true" size={20} />
			</Button>
		</div>
	</section>
</Story>

<Story name="Button und Link" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="m-0 text-xl font-black text-foreground">Semantische Elemente</h2>
			<p class="m-0 text-sm text-muted">Die gleiche visuelle Sprache für Aktionen und Navigation.</p>
		</header>

		<div class="flex flex-wrap items-center gap-4 rounded-2xl border-2 border-border bg-surface p-6">
			<Button type="button">Native Schaltfläche</Button>
			<Button href="#lobby" variant="secondary">Link zur Lobby</Button>
			<Button href="#gesperrt" variant="outline" disabled>Deaktivierter Link</Button>
		</div>
	</section>
</Story>
