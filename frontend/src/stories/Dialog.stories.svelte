<script module lang="ts">
	import MapPinned from "@lucide/svelte/icons/map-pinned";
	import Save from "@lucide/svelte/icons/save";
	import { defineMeta } from "@storybook/addon-svelte-csf";
	import type { ComponentProps } from "svelte";

	import Button from "../lib/components/ui/Button.svelte";
	import Dialog from "../lib/components/ui/Dialog.svelte";

	type PlaygroundArgs = Omit<ComponentProps<typeof Dialog>, "actions" | "children" | "title"> & {
		title: string;
	};

	const longContentSections = [
		"Treffpunkt und Startzeit",
		"Erlaubte Verkehrsmittel",
		"Hinweise zu privaten Grundstücken",
		"Fotoregeln und Einverständnis",
		"Punktevergabe bei Gleichstand",
		"Umgang mit unlösbaren Zielen",
		"Notfallkontakt während des Spiels",
		"Abschluss und gemeinsame Auswertung"
	];

	const { Story } = defineMeta({
		title: "UI/Dialog",
		component: Dialog,
		render: playground,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen"
		},
		args: {
			isVisible: true,
			title: "Lobby-Einstellungen",
			showHeader: true,
			showActions: true,
			showCloseButton: true,
			isModal: true,
			clickOutsideToClose: true
		},
		argTypes: {
			isVisible: { control: "boolean" },
			title: { control: "text" },
			showHeader: { control: "boolean" },
			showActions: { control: "boolean" },
			showCloseButton: { control: "boolean" },
			isModal: { control: "boolean" },
			clickOutsideToClose: { control: "boolean" }
		}
	});
</script>

{#snippet dialogActions()}
	<Button variant="outline">Abbrechen</Button>
	<Button><Save aria-hidden="true" size={17} />Speichern</Button>
{/snippet}

{#snippet customTitle()}
	<div class="flex items-center gap-3">
		<span class="inline-flex size-10 shrink-0 items-center justify-center rounded-xl bg-accent text-accent-foreground">
			<MapPinned aria-hidden="true" size={21} />
		</span>
		<div>
			<span class="block text-xs font-extrabold tracking-widest text-muted uppercase">Nächstes Ziel</span>
			<h2 class="m-0 text-xl font-black tracking-tight text-foreground">Rote Telefonzelle</h2>
		</div>
	</div>
{/snippet}

{#snippet playground(args: PlaygroundArgs)}
	<Dialog {...args} actions={dialogActions}>
		<div class="grid gap-3">
			<p class="m-0 text-sm leading-6 text-muted">
				Passe Titel, Kopfzeile, Aktionen und Schließverhalten über die Controls an.
			</p>
			<div class="rounded-xl border border-border bg-surface-muted p-4 text-sm font-semibold text-foreground">
				4 Teams · 12 Ziele · 90 Minuten
			</div>
		</div>
	</Dialog>
{/snippet}

<Story name="Playground" />

<Story name="Mit Aktionen" asChild>
	<Dialog isVisible title="Änderungen speichern?" actions={dialogActions}>
		<div class="grid gap-3">
			<p class="m-0 leading-6 text-muted">Die neuen Lobby-Einstellungen gelten sofort für alle Teilnehmenden.</p>
			<div class="rounded-xl border-2 border-border bg-surface-muted p-4 text-sm text-foreground">
				<strong class="block">Geänderte Spieldauer</strong>
				60 Minuten → 90 Minuten
			</div>
		</div>
	</Dialog>
</Story>

<Story name="Eigener Titel" asChild>
	<Dialog isVisible title={customTitle} actions={dialogActions}>
		<p class="m-0 leading-6 text-muted">
			Findet die markante Telefonzelle, fotografiert euer Team davor und ladet das Ergebnis hoch.
		</p>
	</Dialog>
</Story>

<Story name="Reduzierte Varianten" asChild>
	<section class="grid gap-6 p-6 lg:grid-cols-2 lg:p-8">
		<div class="grid gap-3">
			<span class="text-xs font-extrabold tracking-widest text-muted uppercase">Ohne Kopfzeile</span>
			<Dialog
				isVisible
				isModal={false}
				clickOutsideToClose={false}
				showHeader={false}
				showActions={false}
				showCloseButton={false}
				aria-label="Kurzer Hinweis"
				class="relative! inset-auto! w-full!"
			>
				<p class="m-0 text-sm leading-6 text-muted">
					Die Runde wurde gespeichert. Du kannst dieses Fenster über Escape schließen.
				</p>
			</Dialog>
		</div>

		<div class="grid gap-3">
			<span class="text-xs font-extrabold tracking-widest text-muted uppercase">Nur Kopfzeile und Inhalt</span>
			<Dialog
				isVisible
				isModal={false}
				clickOutsideToClose={false}
				title="Nur zur Information"
				showActions={false}
				showCloseButton={false}
				class="relative! inset-auto! w-full!"
			>
				<p class="m-0 text-sm leading-6 text-muted">Für diesen Hinweis ist keine direkte Aktion erforderlich.</p>
			</Dialog>
		</div>
	</section>
</Story>

<Story name="Nicht modal" asChild>
	<section class="grid min-h-[30rem] gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="m-0 text-xl font-black text-foreground">Seitenkontext bleibt erreichbar</h2>
			<p class="m-0 text-sm text-muted">Ein nicht-modaler Dialog blockiert die umgebende Oberfläche nicht.</p>
		</header>

		<div class="grid gap-4 rounded-2xl border-2 border-border bg-surface p-5 sm:grid-cols-[1fr_1.2fr]">
			<div class="grid content-start gap-3">
				<Button variant="outline">Lobby-Code kopieren</Button>
				<Button variant="ghost">Spielerübersicht öffnen</Button>
			</div>

			<Dialog
				isVisible
				isModal={false}
				clickOutsideToClose={false}
				title="Tipp"
				showActions={false}
				class="relative! inset-auto! w-full!"
			>
				<p class="m-0 text-sm leading-6 text-muted">Teile den Lobby-Code erst, wenn alle Spielregeln festgelegt sind.</p>
			</Dialog>
		</div>
	</section>
</Story>

<Story name="Langer Inhalt" asChild>
	<Dialog isVisible title="Vollständige Spielregeln" actions={dialogActions} class="m-auto!">
		<div class="grid gap-5">
			<p class="m-0 text-sm leading-6 text-muted">
				Dieser Inhalt überschreitet bewusst die verfügbare Höhe. Kopfzeile, Inhalt und Aktionen bleiben innerhalb des Viewports
				scrollbar.
			</p>

			{#each longContentSections as section, index}
				<section class="grid gap-1.5 rounded-xl border border-border bg-surface-muted p-4">
					<h3 class="m-0 text-sm font-extrabold text-foreground">{index + 1}. {section}</h3>
					<p class="m-0 text-sm leading-6 text-muted">
						Alle Teams bestätigen diese Regel vor dem Start. Rückfragen werden gemeinsam mit der Spielleitung geklärt.
					</p>
				</section>
			{/each}
		</div>
	</Dialog>
</Story>
