<script module lang="ts">
	import { defineMeta } from "@storybook/addon-svelte-csf";
	import type { ComponentProps } from "svelte";

	import Input, { type InputValue } from "../lib/components/ui/Input.svelte";

	type PlaygroundArgs = Omit<ComponentProps<typeof Input>, "value"> & {
		label: string;
		value: InputValue;
	};

	const inputTypes = ["text", "search", "number", "range"] as const;
	let immediateValue = $state<InputValue>(35);
	let immediateChange = $state<InputValue>(35);
	let debouncedValue = $state<InputValue>(35);
	let debouncedChange = $state<InputValue>(35);

	const { Story } = defineMeta({
		title: "UI/Input",
		component: Input,
		render: playground,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen"
		},
		args: {
			label: "Display name",
			type: "text",
			value: "Alex Morgan",
			placeholder: "Enter a display name",
			min: 0,
			max: 100,
			step: 1,
			debounceTime: 0,
			disabled: false,
			readonly: false
		},
		argTypes: {
			label: { control: "text", description: "Visible label for the story example" },
			type: { control: "select", options: inputTypes },
			value: { control: "text", description: "Bindable input value" },
			placeholder: { control: "text" },
			min: { control: "number", description: "Native minimum for number and range inputs" },
			max: { control: "number", description: "Native maximum for number and range inputs" },
			step: { control: "number", description: "Native step size for number and range inputs" },
			rangeMinLabel: { control: "text", description: "Optional visible label for the range minimum" },
			rangeMaxLabel: { control: "text", description: "Optional visible label for the range maximum" },
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

{#snippet valueReadout(boundValue: InputValue, callbackValue: InputValue)}
	<div class="border-border bg-background grid gap-1 rounded-xl border p-3 text-xs">
		<span class="text-muted">Bound value: <code class="text-foreground font-bold">{boundValue}</code></span>
		<span class="text-muted">Callback value: <code class="text-foreground font-bold">{callbackValue}</code></span>
	</div>
{/snippet}

{#snippet playground({ label, ...args }: PlaygroundArgs)}
	<div class="grid max-w-md gap-2 p-6 sm:p-8">
		<label for="input-playground" class="text-foreground text-sm font-extrabold">{label}</label>
		<Input id="input-playground" {...args} />
	</div>
{/snippet}

<Story name="Playground" />

<Story name="Input Types" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Input types</h2>
			<p class="text-muted m-0 text-sm">The four supported native input behaviors.</p>
		</header>

		<div class="border-border bg-surface grid max-w-5xl gap-5 rounded-2xl border-2 p-5 md:grid-cols-2 xl:grid-cols-4">
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-type-text", "Text", 'type="text"')}
				<Input id="input-type-text" type="text" value="Sample title" />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-type-search", "Search", 'type="search"')}
				<Input id="input-type-search" type="search" value="" placeholder="Search records" />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-type-number", "Number", 'type="number"')}
				<Input id="input-type-number" type="number" value={12} min={0} max={50} />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-type-range", "Range", 'type="range"')}
				<Input id="input-type-range" type="range" value={40} min={0} max={100} step={5} />
			</div>
		</div>
	</section>
</Story>

<Story name="Content States" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Content states</h2>
			<p class="text-muted m-0 text-sm">Empty, placeholder, and filled text inputs.</p>
		</header>

		<div class="border-border bg-surface grid max-w-3xl gap-5 rounded-2xl border-2 p-5 md:grid-cols-3">
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-empty", "Empty")}
				<Input id="input-empty" value="" aria-label="Empty input" />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-placeholder", "Placeholder")}
				<Input id="input-placeholder" value="" placeholder="Enter a value" />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-filled", "Filled")}
				<Input id="input-filled" value="Saved value" />
			</div>
		</div>
	</section>
</Story>

<Story name="Interaction States" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Interaction states</h2>
			<p class="text-muted m-0 text-sm">Default, focused, disabled, and read-only text inputs.</p>
		</header>

		<div class="border-border bg-surface grid max-w-4xl gap-5 rounded-2xl border-2 p-5 sm:grid-cols-2">
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-default", "Default")}
				<Input id="input-default" value="Editable value" />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-focus", "Focus", "Focused automatically when this story opens")}
				<Input id="input-focus" value="Focused value" autofocus />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-disabled", "Disabled")}
				<Input id="input-disabled" value="Unavailable value" disabled />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-readonly", "Read only")}
				<Input id="input-readonly" value="Fixed value" readonly />
			</div>
		</div>
	</section>
</Story>

<Story name="Form Composition" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Form composition</h2>
			<p class="text-muted m-0 text-sm">Native attributes, help text, and an explicit error state.</p>
		</header>

		<form
			class="border-border bg-surface grid max-w-lg gap-6 rounded-2xl border-2 p-5"
			onsubmit={(event) => event.preventDefault()}
		>
			<div class="grid gap-2">
				<label for="input-composed-name" class="text-foreground text-sm font-extrabold">Display name</label>
				<Input
					id="input-composed-name"
					name="displayName"
					value="Alex Morgan"
					autocomplete="name"
					maxlength={40}
					required
					aria-describedby="input-composed-name-help"
				/>
				<p id="input-composed-name-help" class="text-muted m-0 text-xs leading-5">Use the name you want other people to see.</p>
			</div>

			<div class="grid gap-2">
				<label for="input-composed-code" class="text-foreground text-sm font-extrabold">Invite code</label>
				<Input
					id="input-composed-code"
					name="inviteCode"
					value="ABC12"
					pattern="[A-Z0-9]{6}"
					aria-invalid="true"
					aria-describedby="input-composed-code-error"
					class="border-primary focus:border-primary"
				/>
				<p id="input-composed-code-error" class="text-primary m-0 text-xs leading-5 font-bold">
					The invite code must contain six characters.
				</p>
			</div>
		</form>
	</section>
</Story>

<Story name="Numeric Constraints" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Numeric constraints</h2>
			<p class="text-muted m-0 text-sm">Native minimum, maximum, and step behavior for number inputs.</p>
		</header>

		<div class="border-border bg-surface grid max-w-2xl gap-5 rounded-2xl border-2 p-5 sm:grid-cols-2">
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-integer", "Integer", "Values from 1 to 10")}
				<Input id="input-integer" type="number" value={3} min={1} max={10} step={1} />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-decimal", "Decimal", "Half-step increments")}
				<Input id="input-decimal" type="number" value={1.5} min={0} max={5} step={0.5} />
			</div>
		</div>
	</section>
</Story>

<Story name="Range Inputs" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Range inputs</h2>
			<p class="text-muted m-0 text-sm">Default limits, custom endpoint labels, step sizes, and disabled behavior.</p>
		</header>

		<div class="border-border bg-surface grid max-w-5xl gap-6 rounded-2xl border-2 p-5 md:grid-cols-3">
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-range-default", "Native defaults", "Uses the HTML limits 0 and 100")}
				<Input id="input-range-default" type="range" />
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-range-custom", "Custom labels", "Step size: 2")}
				<Input
					id="input-range-custom"
					type="range"
					value={16}
					min={2}
					max={32}
					step={2}
					rangeMinLabel="2"
					rangeMaxLabel="32 players"
				/>
			</div>
			<div class="grid content-start gap-2">
				{@render fieldLabel("input-range-disabled", "Disabled")}
				<Input
					id="input-range-disabled"
					type="range"
					value={3}
					min={1}
					max={5}
					step={1}
					rangeMinLabel="Low"
					rangeMaxLabel="High"
					disabled
				/>
			</div>
		</div>
	</section>
</Story>

<Story name="Value Updates" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Value updates</h2>
			<p class="text-muted m-0 text-sm">The bound value updates immediately while the callback can be debounced.</p>
		</header>

		<div class="grid max-w-4xl gap-5 sm:grid-cols-2">
			<div class="border-border bg-surface grid content-start gap-3 rounded-2xl border-2 p-5">
				{@render fieldLabel("input-update-immediate", "Immediate callback", "debounceTime: 0 ms")}
				<Input
					id="input-update-immediate"
					type="range"
					bind:value={immediateValue}
					min={0}
					max={100}
					step={5}
					onchange={(value) => (immediateChange = value)}
				/>
				{@render valueReadout(immediateValue, immediateChange)}
			</div>

			<div class="border-border bg-surface grid content-start gap-3 rounded-2xl border-2 p-5">
				{@render fieldLabel("input-update-debounced", "Debounced callback", "debounceTime: 700 ms")}
				<Input
					id="input-update-debounced"
					type="range"
					bind:value={debouncedValue}
					min={0}
					max={100}
					step={5}
					debounceTime={700}
					onchange={(value) => (debouncedChange = value)}
				/>
				{@render valueReadout(debouncedValue, debouncedChange)}
			</div>
		</div>
	</section>
</Story>
