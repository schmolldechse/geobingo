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
	const surfaceContexts = [
		{ label: "Background", className: "bg-background" },
		{ label: "Surface", className: "bg-surface" },
		{ label: "Muted surface", className: "bg-surface-muted" }
	] as const;

	const { Story } = defineMeta({
		title: "UI/Badge",
		component: Badge,
		render: playground,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen"
		},
		args: {
			text: "Badge",
			tone: "neutral"
		},
		argTypes: {
			text: { control: "text", description: "Plain text content for the badge" },
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

<Story name="Tones" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Semantic badge tones</h2>
			<p class="text-muted m-0 text-sm">All supported tones with the shared one-pixel border.</p>
		</header>

		<div class="border-border bg-surface flex flex-wrap items-center gap-3 rounded-2xl border-2 p-6">
			{#each tones as tone}
				<Badge {tone} text={tone} />
			{/each}
		</div>
	</section>
</Story>

<Story name="Content and attributes" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Text and snippet composition</h2>
			<p class="text-muted m-0 text-sm">Plain text or freely composed content with a decorative icon.</p>
		</header>

		<div class="border-border bg-surface flex flex-wrap items-center gap-3 rounded-2xl border-2 p-6">
			<Badge tone="neutral" text="5 items" />
			<Badge tone="neutral" text="5" class="min-w-6 justify-center py-0.5! text-xs!" />
			<Badge tone="accent" text="×1.5" class="py-0.5! text-[0.58rem]!" />
			<Badge tone="accent" text="120 pts" class="shrink-0 text-xs!" />
			<Badge tone="accent">
				<Crown size={12} strokeWidth={2.5} aria-hidden="true" />
				Admin
			</Badge>
			<Badge tone="secondary">
				<UserRound size={12} strokeWidth={2.5} aria-hidden="true" />
				Current
			</Badge>
			<Badge
				tone="primary"
				text="Featured"
				class="tracking-[0.08em] uppercase"
				aria-label="Featured item"
				title="Native title attribute"
				data-story-example="native-attributes"
			/>
			<Badge tone="accent">
				<MapPin size={12} aria-hidden="true" />
				Location
			</Badge>
		</div>
	</section>
</Story>

<Story name="Responsive layout" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Narrow and wide containers</h2>
			<p class="text-muted m-0 text-sm">Consumers control wrapping while badges remain compact.</p>
		</header>

		<div class="grid gap-5 lg:grid-cols-[20rem_minmax(0,1fr)]">
			<div class="border-border bg-surface grid w-full max-w-80 gap-3 rounded-2xl border-2 p-4">
				<span class="text-muted text-xs font-black">320 px container</span>
				<div class="flex min-w-0 flex-wrap items-center gap-1.5">
					<p class="m-0 min-w-0 truncate text-sm font-black">A very long item label</p>
					<Badge tone="accent" class="shrink-0 tracking-[0.04em] uppercase">
						<Crown size={12} strokeWidth={2.5} aria-hidden="true" />Admin
					</Badge>
					<Badge tone="secondary" class="shrink-0 tracking-[0.04em] uppercase">
						<UserRound size={12} strokeWidth={2.5} aria-hidden="true" />Current
					</Badge>
				</div>
			</div>

			<div class="border-border bg-surface grid gap-3 rounded-2xl border-2 p-4 sm:p-6">
				<span class="text-muted text-xs font-black">Wide container</span>
				<div class="flex min-w-0 flex-wrap items-center gap-1.5">
					<p class="m-0 min-w-0 truncate text-sm font-black">A very long item label</p>
					<Badge tone="accent" class="shrink-0 tracking-[0.04em] uppercase">
						<Crown size={12} strokeWidth={2.5} aria-hidden="true" />Admin
					</Badge>
					<Badge tone="secondary" class="shrink-0 tracking-[0.04em] uppercase">
						<UserRound size={12} strokeWidth={2.5} aria-hidden="true" />Current
					</Badge>
				</div>
			</div>
		</div>
	</section>
</Story>

{#snippet themeExample(label: string)}
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">{label} theme</h2>
			<p class="text-muted m-0 text-sm">Every tone across the semantic background and surface tokens.</p>
		</header>

		<div class="grid gap-4">
			{#each surfaceContexts as context}
				<div class={["border-border text-foreground grid gap-3 rounded-2xl border-2 p-5", context.className]}>
					<span class="text-muted text-xs font-black">{context.label}</span>
					<div class="flex flex-wrap gap-2">
						{#each tones as tone}<Badge {tone} text={tone} />{/each}
					</div>
				</div>
			{/each}
		</div>
	</section>
{/snippet}

<Story name="Light theme" asChild globals={{ theme: "light" }}>
	{@render themeExample("Light")}
</Story>

<Story name="Dark theme" asChild globals={{ theme: "dark" }}>
	{@render themeExample("Dark")}
</Story>
