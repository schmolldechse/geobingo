<script lang="ts">
	import type { Snippet } from "svelte";
	import type { ClassValue, HTMLAttributes } from "svelte/elements";
	import { setTabsContext, TabsContext, type TabsActivationMode, type TabsOrientation } from "./tabs-context.svelte";

	type Props = {
		id?: string;
		value: string;
		orientation?: TabsOrientation;
		activationMode?: TabsActivationMode;
		loop?: boolean;
		disabled?: boolean;
		onvaluechange?: (value: string) => void;
		children?: Snippet;
		class?: ClassValue;
	} & Omit<HTMLAttributes<HTMLDivElement>, "children" | "class" | "id">;

	const uid = $props.id();
	let {
		id = `${uid}-tabs`,
		value = $bindable(),
		orientation = "horizontal",
		activationMode = "automatic",
		loop = true,
		disabled = false,
		onvaluechange,
		children,
		class: className,
		...restProps
	}: Props = $props();

	const tabs = new TabsContext({
		get value() {
			return value;
		},
		set value(nextValue: string) {
			value = nextValue;
		},
		get orientation() {
			return orientation;
		},
		get activationMode() {
			return activationMode;
		},
		get loop() {
			return loop;
		},
		get disabled() {
			return disabled;
		},
		get baseId() {
			return id;
		},
		onvaluechange: (nextValue) => onvaluechange?.(nextValue)
	});
	setTabsContext(tabs);
</script>

<div
	{...restProps}
	{id}
	data-tabs-root
	data-orientation={orientation}
	data-disabled={disabled ? "" : undefined}
	class={[
		"w-full min-w-0",
		"data-[orientation=vertical]:grid data-[orientation=vertical]:gap-4",
		"sm:data-[orientation=vertical]:grid-cols-[minmax(10rem,max-content)_minmax(0,1fr)] sm:data-[orientation=vertical]:items-start",
		className
	]}
>
	{@render children?.()}
</div>
