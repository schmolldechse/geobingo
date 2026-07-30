<script module lang="ts">
	import { defineMeta } from "@storybook/addon-svelte-csf";
	import type { ComponentProps } from "svelte";

	import LoadingDots, { type LoadingDotsSize, type LoadingDotsVariant } from "$lib/components/ui/LoadingDots.svelte";
	import Button from "$lib/components/ui/Button.svelte";

	type PlaygroundArgs = ComponentProps<typeof LoadingDots>;

	const sizes: LoadingDotsSize[] = ["sm", "md", "lg"];
	const variants: LoadingDotsVariant[] = ["brand", "current"];

	const { Story } = defineMeta({
		title: "UI/LoadingDots",
		component: LoadingDots,
		render: playground,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen"
		},
		args: {
			size: "md",
			variant: "brand"
		},
		argTypes: {
			size: { control: "inline-radio", options: sizes },
			variant: { control: "inline-radio", options: variants }
		}
	});
</script>

{#snippet playground(args: PlaygroundArgs)}
	<div class="grid min-h-44 place-items-center p-6 sm:p-8">
		<div class="text-foreground inline-flex items-center gap-3" role="status">
			<LoadingDots {...args} />
			<span class="text-sm font-bold">Loading…</span>
		</div>
	</div>
{/snippet}

<Story name="Playground" />

<Story name="Sizes" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Sizes</h2>
			<p class="text-muted m-0 text-sm">Three scales cover compact controls, inline status, and prominent loading views.</p>
		</header>

		<div class="border-border bg-surface flex flex-wrap items-end gap-8 rounded-2xl border-2 p-6">
			{#each sizes as size (size)}
				<div class="grid justify-items-center gap-3">
					<LoadingDots {size} />
					<code class="text-muted text-xs font-bold">{size}</code>
				</div>
			{/each}
		</div>
	</section>
</Story>

<Story name="Variants" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Color variants</h2>
			<p class="text-muted m-0 max-w-2xl text-sm">
				The brand variant uses the application palette. The current-color variant adapts to its surrounding context.
			</p>
		</header>

		<div class="grid max-w-3xl gap-4 sm:grid-cols-2">
			<div class="border-border bg-surface grid min-h-32 place-items-center gap-3 rounded-2xl border-2 p-5">
				<LoadingDots variant="brand" size="lg" />
				<code class="text-muted text-xs font-bold">brand</code>
			</div>
			<div
				class="bg-secondary text-secondary-foreground grid min-h-32 place-items-center gap-3 rounded-2xl border-2 border-current p-5"
			>
				<LoadingDots variant="current" size="lg" />
				<code class="text-xs font-bold opacity-75">current</code>
			</div>
		</div>
	</section>
</Story>

<Story name="Status composition" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Status composition</h2>
			<p class="text-muted m-0 max-w-2xl text-sm">
				LoadingDots is visual-only. The surrounding status supplies the accessible and visible message.
			</p>
		</header>

		<div class="border-border bg-surface grid max-w-xl gap-6 rounded-2xl border-2 p-5 sm:p-6">
			<div class="text-foreground inline-flex items-center gap-3" role="status">
				<LoadingDots />
				<span class="text-sm font-bold">Synchronizing changes…</span>
			</div>

			<div class="flex flex-wrap gap-3">
				<Button disabled>
					<LoadingDots size="sm" variant="current" />
					Saving
				</Button>
				<Button variant="secondary" disabled>
					<LoadingDots size="sm" variant="current" />
					Connecting
				</Button>
			</div>
		</div>
	</section>
</Story>

<Story name="Customization" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Native attributes and styling</h2>
			<p class="text-muted m-0 max-w-2xl text-sm">
				Classes, data attributes, inherited color, and the animation-duration custom property compose without additional props.
			</p>
		</header>

		<div class="flex flex-wrap items-center gap-8">
			<LoadingDots
				variant="current"
				size="lg"
				class="text-primary rounded-full border-2 border-current p-3"
				data-example="custom-loading-dots"
				style="--loading-dots-duration: 2.2s"
			/>
			<LoadingDots variant="current" size="lg" class="text-secondary" style="--loading-dots-duration: 0.75s" />
		</div>
	</section>
</Story>
