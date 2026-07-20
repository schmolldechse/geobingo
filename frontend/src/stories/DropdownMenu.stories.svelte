<script module lang="ts">
	import ChevronDown from "@lucide/svelte/icons/chevron-down";
	import Copy from "@lucide/svelte/icons/copy";
	import LogOut from "@lucide/svelte/icons/log-out";
	import Settings from "@lucide/svelte/icons/settings";
	import UserPlus from "@lucide/svelte/icons/user-plus";
	import UserRound from "@lucide/svelte/icons/user-round";
	import { defineMeta } from "@storybook/addon-svelte-csf";
	import type { ComponentProps } from "svelte";

	import Button from "../lib/components/ui/Button.svelte";
	import * as DropdownMenu from "../lib/components/ui/dropdown-menu/index";
	import type { DropdownMenuAlign, DropdownMenuTriggerChildProps } from "../lib/components/ui/dropdown-menu/index";

	type PlaygroundArgs = Omit<ComponentProps<typeof DropdownMenu.Root>, "children">;

	const alignments: DropdownMenuAlign[] = ["start", "center", "end"];
	const longMenuItems = [
		"Altstadt",
		"Bahnhof",
		"Botanischer Garten",
		"Flussufer",
		"Fußgängerzone",
		"Industriehafen",
		"Kunstmuseum",
		"Marktplatz",
		"Rathaus",
		"Stadtbibliothek",
		"Stadtpark",
		"Wasserturm"
	];

	const { Story } = defineMeta({
		title: "UI/Dropdown Menu",
		component: DropdownMenu.Root,
		render: playground,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen"
		},
		args: {
			open: true,
			disabled: false,
			closeOnEscape: true,
			closeOnInteractOutside: true,
			loop: true
		},
		argTypes: {
			open: { control: "boolean" },
			disabled: { control: "boolean" },
			closeOnEscape: { control: "boolean" },
			closeOnInteractOutside: { control: "boolean" },
			loop: { control: "boolean" }
		}
	});
</script>

{#snippet accountItems()}
	<DropdownMenu.Group aria-label="Konto und Lobby">
		<DropdownMenu.GroupHeading>
			<span class="text-foreground block text-sm tracking-normal normal-case">Alex Beispiel</span>
			<span class="block font-semibold tracking-normal normal-case">alex@geobingo.de</span>
		</DropdownMenu.GroupHeading>
		<DropdownMenu.Separator />
		<DropdownMenu.Item><UserRound aria-hidden="true" size={17} />Profil ansehen</DropdownMenu.Item>
		<DropdownMenu.Item><UserPlus aria-hidden="true" size={17} />Spieler einladen</DropdownMenu.Item>
		<DropdownMenu.Item disabled><Settings aria-hidden="true" size={17} />Admin-Einstellungen</DropdownMenu.Item>
		<DropdownMenu.Separator />
		<DropdownMenu.Item class="text-primary"><LogOut aria-hidden="true" size={17} />Abmelden</DropdownMenu.Item>
	</DropdownMenu.Group>
{/snippet}

{#snippet playground(args: PlaygroundArgs)}
	<div class="min-h-96 p-8">
		<DropdownMenu.Root {...args}>
			<DropdownMenu.Trigger>Kontomenü <ChevronDown aria-hidden="true" size={16} /></DropdownMenu.Trigger>
			<DropdownMenu.Content align="start" class="w-72">
				{@render accountItems()}
			</DropdownMenu.Content>
		</DropdownMenu.Root>
	</div>
{/snippet}

<Story name="Playground" />

<Story name="Komplettes Menü" asChild>
	<section class="min-h-96 p-6 sm:p-8">
		<header class="mb-6 grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Komplettes Menü</h2>
			<p class="text-muted m-0 text-sm">Gruppen, Überschriften, Icons, Trenner und nicht verfügbare Einträge.</p>
		</header>

		<DropdownMenu.Root open closeOnInteractOutside={false}>
			<DropdownMenu.Trigger>Lobby verwalten <ChevronDown aria-hidden="true" size={16} /></DropdownMenu.Trigger>
			<DropdownMenu.Content class="w-72">
				<DropdownMenu.Group aria-label="Lobby">
					<DropdownMenu.GroupHeading>Lobby-Code: BER-204</DropdownMenu.GroupHeading>
					<DropdownMenu.Item closeOnSelect={false}>
						<Copy aria-hidden="true" size={17} />Code kopieren
					</DropdownMenu.Item>
					<DropdownMenu.Item><UserPlus aria-hidden="true" size={17} />Spieler einladen</DropdownMenu.Item>
					<DropdownMenu.Item disabled><Settings aria-hidden="true" size={17} />Spiel läuft bereits</DropdownMenu.Item>
					<DropdownMenu.Separator />
					<DropdownMenu.Item class="text-primary"><LogOut aria-hidden="true" size={17} />Lobby verlassen</DropdownMenu.Item>
				</DropdownMenu.Group>
			</DropdownMenu.Content>
		</DropdownMenu.Root>
	</section>
</Story>

<Story name="Ausrichtung" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Ausrichtung</h2>
			<p class="text-muted m-0 text-sm">Der Inhalt kann am Anfang, mittig oder am Ende des Triggers ausgerichtet werden.</p>
		</header>

		<div class="grid gap-6 lg:grid-cols-3">
			{#each alignments as align}
				<div class="border-border bg-surface min-h-72 rounded-2xl border-2 p-5">
					<div class={align === "center" ? "text-center" : align === "end" ? "text-right" : "text-left"}>
						<DropdownMenu.Root open closeOnInteractOutside={false}>
							<DropdownMenu.Trigger>{align} <ChevronDown aria-hidden="true" size={16} /></DropdownMenu.Trigger>
							<DropdownMenu.Content {align} class="w-52 text-left">
								<DropdownMenu.Item>Erster Eintrag</DropdownMenu.Item>
								<DropdownMenu.Item>Zweiter Eintrag</DropdownMenu.Item>
							</DropdownMenu.Content>
						</DropdownMenu.Root>
					</div>
				</div>
			{/each}
		</div>
	</section>
</Story>

<Story name="Zustände" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Zustände</h2>
			<p class="text-muted m-0 text-sm">
				Geschlossen, vollständig deaktiviert, deaktivierter Trigger und deaktivierter Eintrag.
			</p>
		</header>

		<div class="grid gap-5 md:grid-cols-2 xl:grid-cols-4">
			<div class="border-border bg-surface min-h-64 rounded-2xl border-2 p-5">
				<p class="text-muted mt-0 text-xs font-extrabold tracking-widest uppercase">Geschlossen</p>
				<DropdownMenu.Root>
					<DropdownMenu.Trigger>Menü <ChevronDown aria-hidden="true" size={16} /></DropdownMenu.Trigger>
					<DropdownMenu.Content><DropdownMenu.Item>Eintrag</DropdownMenu.Item></DropdownMenu.Content>
				</DropdownMenu.Root>
			</div>

			<div class="border-border bg-surface min-h-64 rounded-2xl border-2 p-5">
				<p class="text-muted mt-0 text-xs font-extrabold tracking-widest uppercase">Root deaktiviert</p>
				<DropdownMenu.Root open disabled closeOnInteractOutside={false}>
					<DropdownMenu.Trigger>Menü <ChevronDown aria-hidden="true" size={16} /></DropdownMenu.Trigger>
					<DropdownMenu.Content class="w-52"><DropdownMenu.Item>Eintrag</DropdownMenu.Item></DropdownMenu.Content>
				</DropdownMenu.Root>
			</div>

			<div class="border-border bg-surface min-h-64 rounded-2xl border-2 p-5">
				<p class="text-muted mt-0 text-xs font-extrabold tracking-widest uppercase">Trigger deaktiviert</p>
				<DropdownMenu.Root open closeOnInteractOutside={false}>
					<DropdownMenu.Trigger disabled>Menü <ChevronDown aria-hidden="true" size={16} /></DropdownMenu.Trigger>
					<DropdownMenu.Content class="w-52"><DropdownMenu.Item>Eintrag</DropdownMenu.Item></DropdownMenu.Content>
				</DropdownMenu.Root>
			</div>

			<div class="border-border bg-surface min-h-64 rounded-2xl border-2 p-5">
				<p class="text-muted mt-0 text-xs font-extrabold tracking-widest uppercase">Eintrag deaktiviert</p>
				<DropdownMenu.Root open closeOnInteractOutside={false}>
					<DropdownMenu.Trigger>Menü <ChevronDown aria-hidden="true" size={16} /></DropdownMenu.Trigger>
					<DropdownMenu.Content class="w-52">
						<DropdownMenu.Item>Verfügbar</DropdownMenu.Item>
						<DropdownMenu.Item disabled>Nicht verfügbar</DropdownMenu.Item>
					</DropdownMenu.Content>
				</DropdownMenu.Root>
			</div>
		</div>
	</section>
</Story>

<Story name="Trigger-Komposition" asChild>
	<section class="min-h-80 p-6 sm:p-8">
		<header class="mb-6 grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Eigener Trigger</h2>
			<p class="text-muted m-0 text-sm">Der Trigger reicht Zustand und Interaktionen an eine Button-Komponente weiter.</p>
		</header>

		<div class="flex max-w-md justify-end">
			<DropdownMenu.Root open closeOnInteractOutside={false}>
				<DropdownMenu.Trigger>
					{#snippet child({ props, open }: DropdownMenuTriggerChildProps)}
						<Button {...props} variant={open ? "accent" : "outline"}>
							<UserRound aria-hidden="true" size={18} />Alex<ChevronDown aria-hidden="true" size={16} />
						</Button>
					{/snippet}
				</DropdownMenu.Trigger>
				<DropdownMenu.Content align="end" class="w-64">
					{@render accountItems()}
				</DropdownMenu.Content>
			</DropdownMenu.Root>
		</div>
	</section>
</Story>

<Story name="Langes Menü" asChild>
	<section class="min-h-[34rem] p-6 sm:p-8">
		<header class="mb-6 grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Langes Menü</h2>
			<p class="text-muted m-0 text-sm">Triggerbreite, maximale Höhe und Scrollverhalten bei vielen Einträgen.</p>
		</header>

		<DropdownMenu.Root open closeOnInteractOutside={false}>
			<DropdownMenu.Trigger class="w-64 justify-between!">
				Gebiet auswählen <ChevronDown aria-hidden="true" size={16} />
			</DropdownMenu.Trigger>
			<DropdownMenu.Content matchTriggerWidth>
				<DropdownMenu.Group aria-label="Gebiete">
					<DropdownMenu.GroupHeading>Berlin</DropdownMenu.GroupHeading>
					{#each longMenuItems as item, index}
						<DropdownMenu.Item textValue={item}>
							<span class="text-muted w-5 text-xs">{String(index + 1).padStart(2, "0")}</span>{item}
						</DropdownMenu.Item>
					{/each}
				</DropdownMenu.Group>
			</DropdownMenu.Content>
		</DropdownMenu.Root>
	</section>
</Story>
