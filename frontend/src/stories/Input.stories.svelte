<script module lang="ts">
	import { defineMeta } from "@storybook/addon-svelte-csf";
	import type { ComponentProps } from "svelte";

	import Input, { type InputValue } from "../lib/components/ui/Input.svelte";

	type PlaygroundArgs = Omit<ComponentProps<typeof Input>, "value"> & {
		label: string;
		value: InputValue;
	};

	const inputTypes = ["text", "search", "number"] as const;

	const { Story } = defineMeta({
		title: "UI/Input",
		component: Input,
		render: playground,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen"
		},
		args: {
			label: "Zielname",
			type: "text",
			value: "Rote Telefonzelle",
			placeholder: "Ziel eingeben",
			debounceTime: 0,
			disabled: false,
			readonly: false
		},
		argTypes: {
			label: { control: "text", description: "Beschriftung des Story-Beispiels" },
			type: { control: "select", options: inputTypes },
			value: { control: "text" },
			placeholder: { control: "text" },
			debounceTime: { control: { type: "number", min: 0, step: 100 } },
			disabled: { control: "boolean" },
			readonly: { control: "boolean" }
		}
	});
</script>

{#snippet fieldLabel(forId: string, label: string, description?: string)}
	<div class="grid gap-0.5">
		<label for={forId} class="text-foreground text-sm font-extrabold">{label}</label>
		{#if description}
			<span class="text-muted text-xs">{description}</span>
		{/if}
	</div>
{/snippet}

{#snippet playground({ label, ...args }: PlaygroundArgs)}
	<div class="grid max-w-md gap-2 p-6 sm:p-8">
		<label for="input-playground" class="text-foreground text-sm font-extrabold">{label}</label>
		<Input id="input-playground" {...args} />
	</div>
{/snippet}

<Story name="Playground" />

<Story name="Typen" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Eingabetypen</h2>
			<p class="text-muted m-0 text-sm">Text, Suche und Zahl mit der jeweils passenden nativen Eingabe.</p>
		</header>

		<div class="border-border bg-surface grid max-w-3xl gap-5 rounded-2xl border-2 p-5 md:grid-cols-3">
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-type-text", "Text", 'type="text"')}
				<Input id="input-type-text" type="text" value="Fernsehturm" />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-type-search", "Suche", 'type="search"')}
				<Input id="input-type-search" type="search" value="Museum" placeholder="Ziele durchsuchen" />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-type-number", "Zahl", 'type="number"')}
				<Input id="input-type-number" type="number" value={4} min={1} max={10} />
			</div>
		</div>
	</section>
</Story>

<Story name="Inhaltszustände" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Inhaltszustände</h2>
			<p class="text-muted m-0 text-sm">Leer, mit Platzhalter und mit einem vorhandenen Wert.</p>
		</header>

		<div class="border-border bg-surface grid max-w-3xl gap-5 rounded-2xl border-2 p-5 md:grid-cols-3">
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-empty", "Leer")}
				<Input id="input-empty" value="" aria-label="Leeres Eingabefeld" />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-placeholder", "Platzhalter")}
				<Input id="input-placeholder" value="" placeholder="Lobby-Code eingeben" />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-filled", "Ausgefüllt")}
				<Input id="input-filled" value="BER-204" />
			</div>
		</div>
	</section>
</Story>

<Story name="Interaktionszustände" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Interaktionszustände</h2>
			<p class="text-muted m-0 text-sm">Standard, fokussiert, deaktiviert und schreibgeschützt.</p>
		</header>

		<div class="border-border bg-surface grid max-w-4xl gap-5 rounded-2xl border-2 p-5 sm:grid-cols-2">
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-default", "Standard")}
				<Input id="input-default" value="Alexanderplatz" />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-focus", "Fokus", "Wird beim Öffnen der Story automatisch fokussiert")}
				<Input id="input-focus" value="Brandenburger Tor" autofocus />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-disabled", "Deaktiviert")}
				<Input id="input-disabled" value="Nicht verfügbar" disabled />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-readonly", "Schreibgeschützt")}
				<Input id="input-readonly" value="Fester Lobby-Code" readonly />
			</div>
		</div>
	</section>
</Story>

<Story name="Formular-Komposition" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Formular-Komposition</h2>
			<p class="text-muted m-0 text-sm">Beschriftung, Hilfetext und ein expliziter Fehlerzustand.</p>
		</header>

		<form
			class="border-border bg-surface grid max-w-lg gap-6 rounded-2xl border-2 p-5"
			onsubmit={(event) => event.preventDefault()}
		>
			<div class="grid gap-2">
				<label for="input-composed-name" class="text-foreground text-sm font-extrabold">Öffentlicher Teamname</label>
				<Input id="input-composed-name" value="Kartenfüchse" aria-describedby="input-composed-name-help" />
				<p id="input-composed-name-help" class="text-muted m-0 text-xs leading-5">
					Dieser Name ist für alle Teilnehmenden der Lobby sichtbar.
				</p>
			</div>

			<div class="grid gap-2">
				<label for="input-composed-code" class="text-foreground text-sm font-extrabold">Lobby-Code</label>
				<Input
					id="input-composed-code"
					value="BER20"
					aria-invalid="true"
					aria-describedby="input-composed-code-error"
					class="border-primary focus:border-primary"
				/>
				<p id="input-composed-code-error" class="text-primary m-0 text-xs leading-5 font-bold">
					Der Lobby-Code muss aus sechs Zeichen bestehen.
				</p>
			</div>
		</form>
	</section>
</Story>

<Story name="Zahlenbereich" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Zahlenbereich</h2>
			<p class="text-muted m-0 text-sm">Numerischer Wert mit nativen Grenzen und Schrittweite.</p>
		</header>

		<div class="border-border bg-surface grid max-w-sm gap-2 rounded-2xl border-2 p-5">
			{@render fieldLabel("input-range", "Punktefaktor", "Erlaubt sind Werte von 1 bis 10")}
			<Input id="input-range" type="number" value={3} min={1} max={10} step={1} />
		</div>
	</section>
</Story>
