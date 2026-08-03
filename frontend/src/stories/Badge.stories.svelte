<script module lang="ts">
	import Crown from "@lucide/svelte/icons/crown";
	import MapPin from "@lucide/svelte/icons/map-pin";
	import UserRound from "@lucide/svelte/icons/user-round";
	import { defineMeta } from "@storybook/addon-svelte-csf";

	import Badge, { type BadgeTone } from "../lib/components/ui/Badge.svelte";

	type PlaygroundArgs = {
		text: string;
		tone: BadgeTone;
	};

	const tones: BadgeTone[] = ["neutral", "primary", "secondary", "accent"];

	const { Story } = defineMeta({
		title: "UI/Badge",
		component: Badge,
		render: playground,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen"
		},
		args: {
			text: "GeoBingo",
			tone: "neutral"
		},
		argTypes: {
			text: { control: "text", description: "Einfacher Textinhalt des Badges" },
			tone: { control: "select", options: tones }
		}
	});
</script>

{#snippet playground(args: PlaygroundArgs)}
	<div class="flex min-h-40 items-center justify-center p-8">
		<Badge {...args} />
	</div>
{/snippet}

<Story name="Playground" />

<Story name="Töne" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">GeoBingo-Farbtöne</h2>
			<p class="text-muted m-0 text-sm">Alle verfügbaren semantischen Farbvarianten.</p>
		</header>

		<div class="border-border bg-surface flex flex-wrap items-center gap-3 rounded-2xl border-2 p-6">
			{#each tones as tone}
				<Badge {tone} text={tone} />
			{/each}
		</div>
	</section>
</Story>

<Story name="Inhalte und Attribute" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Text und Snippet-Komposition</h2>
			<p class="text-muted m-0 text-sm">Einfacher Text oder frei komponierter Inhalt mit dekorativem Icon.</p>
		</header>

		<div class="border-border bg-surface flex flex-wrap items-center gap-3 rounded-2xl border-2 p-6">
			<Badge tone="neutral" text="4 players" />
			<Badge tone="accent">
				<Crown size={12} strokeWidth={2.5} aria-hidden="true" />
				Host
			</Badge>
			<Badge tone="secondary">
				<UserRound size={12} strokeWidth={2.5} aria-hidden="true" />
				You
			</Badge>
			<Badge
				tone="primary"
				text="Featured"
				class="tracking-[0.08em] uppercase"
				aria-label="Featured location"
				title="Native title attribute"
				data-story-example="native-attributes"
			/>
			<Badge tone="accent">
				<MapPin size={12} aria-hidden="true" />
				Berlin
			</Badge>
		</div>
	</section>
</Story>

<Story name="Responsives Rollenlayout" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Mobile und Desktop</h2>
			<p class="text-muted m-0 text-sm">Der Consumer steuert den Umbruch, während die Badges kompakt bleiben.</p>
		</header>

		<div class="grid gap-5 lg:grid-cols-[20rem_minmax(0,1fr)]">
			<div class="border-border bg-surface grid w-full max-w-80 gap-3 rounded-2xl border-2 p-4">
				<span class="text-muted text-xs font-black">320 px Kontext</span>
				<div class="flex min-w-0 flex-wrap items-center gap-1.5">
					<p class="m-0 min-w-0 truncate text-sm font-black">A very long player display name</p>
					<Badge tone="accent" class="shrink-0 tracking-[0.04em] uppercase">
						<Crown size={12} strokeWidth={2.5} aria-hidden="true" />Host
					</Badge>
					<Badge tone="secondary" class="shrink-0 tracking-[0.04em] uppercase">
						<UserRound size={12} strokeWidth={2.5} aria-hidden="true" />You
					</Badge>
				</div>
			</div>

			<div class="border-border bg-surface grid gap-3 rounded-2xl border-2 p-4 sm:p-6">
				<span class="text-muted text-xs font-black">Breiter Kontext</span>
				<div class="flex min-w-0 flex-wrap items-center gap-1.5">
					<p class="m-0 min-w-0 truncate text-sm font-black">A very long player display name</p>
					<Badge tone="accent" class="shrink-0 tracking-[0.04em] uppercase">
						<Crown size={12} strokeWidth={2.5} aria-hidden="true" />Host
					</Badge>
					<Badge tone="secondary" class="shrink-0 tracking-[0.04em] uppercase">
						<UserRound size={12} strokeWidth={2.5} aria-hidden="true" />You
					</Badge>
				</div>
			</div>
		</div>
	</section>
</Story>

{#snippet themeExample(label: string)}
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">{label} GeoBingo theme</h2>
			<p class="text-muted m-0 text-sm">Alle Töne verwenden die aktiven semantischen Theme-Tokens.</p>
		</header>

		<div class="border-border bg-background text-foreground grid gap-3 rounded-2xl border-2 p-5">
			<span class="text-muted text-xs font-black">{label}</span>
			<div class="flex flex-wrap gap-2">
				{#each tones as tone}<Badge {tone} text={tone} />{/each}
			</div>
		</div>
	</section>
{/snippet}

<Story name="Helles Farbschema" asChild globals={{ theme: "light" }}>
	{@render themeExample("Light")}
</Story>

<Story name="Dunkles Farbschema" asChild globals={{ theme: "dark" }}>
	{@render themeExample("Dark")}
</Story>
