<script lang="ts">
	import { untrack } from "svelte";
	import type { Snippet } from "svelte";
	import type { ClassValue, HTMLButtonAttributes } from "svelte/elements";
	import { getDropdownMenuContext } from "./dropdown-menu-context.svelte";

	type AttributeValue = string | number | boolean | null | undefined;
	type Props = {
		disabled?: boolean;
		closeOnSelect?: boolean;
		textValue?: string;
		onselect?: (event: Event) => void;
		children?: Snippet;
		class?: ClassValue;
		[key: `data-${string}`]: AttributeValue;
	} & Omit<
		HTMLButtonAttributes,
		"children" | "class" | "disabled" | "onclick" | "onfocus" | "onkeydown" | "onpointermove" | "role"
	>;

	const uid = $props.id();
	let {
		id: providedId = undefined,
		type = "button",
		disabled = false,
		closeOnSelect = true,
		textValue,
		onselect,
		children,
		class: className,
		...rest
	}: Props = $props();

	const menu = getDropdownMenuContext();
	let itemElement: HTMLButtonElement | undefined = $state(undefined);
	const itemId = $derived(providedId ?? `${uid}-dropdown-menu-item`);
	const isDisabled = $derived(menu.disabled || disabled);
	const active = $derived(menu.activeItemId === itemId && !isDisabled);

	const select = (event: Event) => {
		if (isDisabled) {
			event.preventDefault();
			return;
		}

		onselect?.(event);
		if (event.defaultPrevented) return;
		if (closeOnSelect) menu.closeMenu();
	};

	const handleClick = (event: MouseEvent) => select(event);

	$effect(() => {
		if (!itemElement) return;

		const id = itemId;
		const element = itemElement;

		untrack(() =>
			menu.registerItem({
				id,
				element,
				disabled: () => isDisabled,
				textValue: () => textValue ?? element.textContent?.trim() ?? "",
				select
			})
		);

		return () => untrack(() => menu.unregisterItem(id));
	});
</script>

<button
	{...rest}
	id={itemId}
	bind:this={itemElement}
	{type}
	role="menuitem"
	disabled={isDisabled}
	tabindex={active ? 0 : -1}
	data-dropdown-menu-item={true}
	data-active={active ? true : undefined}
	data-disabled={isDisabled ? true : undefined}
	onclick={handleClick}
	onfocus={() => menu.setActiveItem(itemId)}
	onpointermove={() => {
		if (!isDisabled) menu.setActiveItem(itemId);
	}}
	class={[
		"text-foreground flex min-h-11 w-full items-center gap-2 rounded-lg px-3 py-2 text-left text-sm font-extrabold outline-none",
		"transition-[background-color,color,transform] duration-150 motion-reduce:transition-none",
		"enabled:hover:bg-accent enabled:hover:text-accent-foreground enabled:cursor-pointer enabled:active:translate-x-px",
		"data-active:bg-accent data-active:text-accent-foreground disabled:cursor-not-allowed disabled:opacity-45",
		className
	]}
>
	{@render children?.()}
</button>
