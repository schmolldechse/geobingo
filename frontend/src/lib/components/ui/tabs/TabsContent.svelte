<script lang="ts">
	import type { Snippet } from "svelte";
	import type { ClassValue, HTMLAttributes } from "svelte/elements";
	import { getTabsContext } from "./tabs-context.svelte";

	type Props = {
		value: string;
		children?: Snippet;
		class?: ClassValue;
	} & Omit<
		HTMLAttributes<HTMLDivElement>,
		"aria-labelledby" | "children" | "class" | "data-state" | "hidden" | "id" | "role" | "tabindex"
	>;

	let { value, children, class: className, ...restProps }: Props = $props();

	const tabs = getTabsContext();
	const state = $derived(tabs.state(value));
	const triggerId = $derived(tabs.getTriggerId(value));
	const contentId = $derived(tabs.getContentId(value));
</script>

<div
	{...restProps}
	id={contentId}
	role="tabpanel"
	aria-labelledby={triggerId}
	hidden={state !== "active"}
	tabindex={0}
	data-tabs-content
	data-state={state}
	data-value={value}
	data-orientation={tabs.orientation}
	class={[
		"text-foreground min-w-0",
		"data-[orientation=horizontal]:pt-4",
		"data-[orientation=vertical]:pt-4 sm:data-[orientation=vertical]:pt-0",
		className
	]}
>
	{@render children?.()}
</div>
