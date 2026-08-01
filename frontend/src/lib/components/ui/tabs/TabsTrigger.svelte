<script lang="ts">
	import type { Snippet } from "svelte";
	import type { ClassValue, HTMLButtonAttributes } from "svelte/elements";
	import { getTabsContext } from "./tabs-context.svelte";

	type Props = {
		value: string;
		disabled?: boolean;
		children?: Snippet;
		class?: ClassValue;
	} & Omit<
		HTMLButtonAttributes,
		"aria-controls" | "aria-selected" | "children" | "class" | "data-state" | "disabled" | "id" | "role" | "tabindex"
	>;
	type ButtonMouseEvent = MouseEvent & { currentTarget: EventTarget & HTMLButtonElement };
	type ButtonFocusEvent = FocusEvent & { currentTarget: EventTarget & HTMLButtonElement };
	type ButtonKeyboardEvent = KeyboardEvent & { currentTarget: EventTarget & HTMLButtonElement };

	let {
		value,
		disabled = false,
		children,
		class: className,
		type = "button",
		onclick,
		onfocus,
		onkeydown,
		...restProps
	}: Props = $props();

	const tabs = getTabsContext();
	const isDisabled = $derived(tabs.isTriggerDisabled(disabled));
	const state = $derived(tabs.state(value));
	const triggerId = $derived(tabs.getTriggerId(value));
	const contentId = $derived(tabs.getContentId(value));

	function registerTrigger(element: HTMLButtonElement) {
		return tabs.registerTrigger({
			element,
			isDisabled: () => isDisabled
		});
	}

	function handleClick(event: ButtonMouseEvent): void {
		onclick?.(event);
		if (event.defaultPrevented || isDisabled) return;
		tabs.setValue(value);
	}

	function handleFocus(event: ButtonFocusEvent): void {
		onfocus?.(event);
		if (isDisabled || tabs.activationMode !== "automatic") return;
		tabs.setValue(value);
	}

	function handleKeydown(event: ButtonKeyboardEvent): void {
		onkeydown?.(event);
		if (event.defaultPrevented || isDisabled) return;

		if (event.key === "Enter" || event.key === " ") {
			event.preventDefault();
			tabs.setValue(value);
			return;
		}

		tabs.handleTriggerKeydown(event.currentTarget, event);
	}
</script>

<button
	{...restProps}
	{@attach registerTrigger}
	id={triggerId}
	{type}
	disabled={isDisabled}
	role="tab"
	aria-selected={state === "active"}
	aria-controls={contentId}
	tabindex={state === "active" ? 0 : -1}
	data-tabs-trigger
	data-state={state}
	data-value={value}
	data-orientation={tabs.orientation}
	data-disabled={isDisabled ? "" : undefined}
	onclick={handleClick}
	onfocus={handleFocus}
	onkeydown={handleKeydown}
	class={[
		"text-muted relative inline-flex min-h-11 shrink-0 cursor-pointer items-center justify-center gap-2 px-4 py-2.5 text-sm font-extrabold whitespace-nowrap",
		"transition-[color,background-color] duration-150 motion-reduce:transition-none",
		"after:absolute after:bg-transparent after:content-['']",
		"hover:bg-surface-muted hover:text-foreground focus-visible:z-10",
		"disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-45",
		"data-[state=active]:text-foreground data-[state=active]:after:bg-primary",
		"data-[orientation=horizontal]:after:right-[0.55rem] data-[orientation=horizontal]:after:-bottom-0.5 data-[orientation=horizontal]:after:left-[0.55rem] data-[orientation=horizontal]:after:h-1 data-[orientation=horizontal]:after:rounded-t-full data-[orientation=horizontal]:after:rounded-b-none",
		"data-[orientation=vertical]:w-full data-[orientation=vertical]:justify-start data-[orientation=vertical]:after:top-[0.55rem] data-[orientation=vertical]:after:-right-0.5 data-[orientation=vertical]:after:bottom-[0.55rem] data-[orientation=vertical]:after:w-[3px] data-[orientation=vertical]:after:rounded-l-full data-[orientation=vertical]:after:rounded-r-none",
		className
	]}
>
	{@render children?.()}
</button>
