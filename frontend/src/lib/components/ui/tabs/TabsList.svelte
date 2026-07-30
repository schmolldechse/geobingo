<script lang="ts">
	import type { Snippet } from "svelte";
	import type { ClassValue, HTMLAttributes } from "svelte/elements";
	import { getTabsContext } from "./tabs-context.svelte";

	type Props = {
		children?: Snippet;
		class?: ClassValue;
	} & Omit<HTMLAttributes<HTMLDivElement>, "aria-orientation" | "children" | "class" | "role">;

	let { children, class: className, ...restProps }: Props = $props();

	const tabs = getTabsContext();
</script>

<div
	{...restProps}
	role="tablist"
	aria-orientation={tabs.orientation}
	data-tabs-list
	data-orientation={tabs.orientation}
	data-disabled={tabs.disabled ? "" : undefined}
	class={[
		"relative flex min-w-0 gap-1",
		"data-[orientation=horizontal]:border-border data-[orientation=horizontal]:w-full data-[orientation=horizontal]:items-end data-[orientation=horizontal]:overflow-x-auto data-[orientation=horizontal]:overscroll-x-contain data-[orientation=horizontal]:border-b-2 data-[orientation=horizontal]:px-[0.2rem]",
		"data-[orientation=vertical]:border-border data-[orientation=vertical]:w-full data-[orientation=vertical]:flex-col data-[orientation=vertical]:border-r-2 data-[orientation=vertical]:py-[0.2rem]",
		"sm:data-[orientation=vertical]:w-auto",
		className
	]}
>
	{@render children?.()}
</div>

<style>
	[data-tabs-list][data-orientation="horizontal"] {
		scrollbar-color: var(--border) transparent;
		scrollbar-width: thin;
	}
</style>
