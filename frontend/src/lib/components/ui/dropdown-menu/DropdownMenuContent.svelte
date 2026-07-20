<script lang="ts">
	import type { Snippet } from "svelte";
	import type { ClassValue, HTMLAttributes } from "svelte/elements";
	import { getDropdownMenuContext, type DropdownMenuAlign } from "./dropdown-menu-context.svelte";

	type AttributeValue = string | number | boolean | null | undefined;
	type Props = {
		align?: DropdownMenuAlign;
		sideOffset?: number;
		matchTriggerWidth?: boolean;
		children?: Snippet;
		class?: ClassValue;
		[key: `data-${string}`]: AttributeValue;
	} & Omit<HTMLAttributes<HTMLDivElement>, "children" | "class" | "onkeydown">;

	let {
		align = "start",
		sideOffset = 6,
		matchTriggerWidth = false,
		children,
		class: className,
		style,
		...rest
	}: Props = $props();

	const menu = getDropdownMenuContext();
	const registerContent = (node: HTMLDivElement) => {
		menu.contentElement = node;

		return {
			destroy: () => {
				if (menu.contentElement === node) menu.contentElement = undefined;
			}
		};
	};
	const widthStyle = $derived(
		matchTriggerWidth ? `width: ${menu.triggerWidth > 0 ? `${menu.triggerWidth}px` : "100%"}` : undefined
	);
	const contentStyle = $derived([`top: calc(100% + ${sideOffset}px)`, widthStyle, style].filter(Boolean).join("; "));
</script>

{#if menu.open}
	<div
		{...rest}
		id={menu.contentId}
		use:registerContent
		role="menu"
		tabindex="-1"
		aria-labelledby={menu.triggerId}
		data-dropdown-menu-content={true}
		data-state={menu.state}
		onkeydown={menu.handleContentKeydown}
		style={contentStyle}
		class={[
			"border-foreground bg-surface text-foreground absolute z-50 max-h-[min(24rem,calc(100dvh-2rem))] max-w-[calc(100vw-2rem)] min-w-52 overflow-y-auto rounded-xl border-2 p-1.5 shadow-[var(--shadow-paper-raised)] outline-none",
			"transition-[opacity,transform] duration-150 motion-reduce:transition-none",
			align === "start" && "left-0",
			align === "center" && "left-1/2 -translate-x-1/2",
			align === "end" && "right-0",
			className
		]}
	>
		{@render children?.()}
	</div>
{/if}
