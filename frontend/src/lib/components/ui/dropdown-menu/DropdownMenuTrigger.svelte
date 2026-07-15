<script module lang="ts">
	import type { Snippet } from "svelte";

	type DropdownMenuTriggerChildProps = {
		props: {
			id: string;
			"aria-controls": string;
			"aria-expanded": boolean;
			"aria-haspopup": "menu";
			"data-state": "open" | "closed";
			"data-disabled"?: true;
			disabled?: boolean;
			onclick: (event: MouseEvent) => void;
			onfocus: (event: FocusEvent) => void;
			onkeydown: (event: KeyboardEvent) => void;
		};
		open: boolean;
		disabled: boolean;
	};

	export type { DropdownMenuTriggerChildProps };
</script>

<script lang="ts">
	import type { ClassValue, HTMLButtonAttributes } from "svelte/elements";
	import { getDropdownMenuContext } from "./dropdown-menu-context.svelte";

	type AttributeValue = string | number | boolean | null | undefined;
	type Props = {
		disabled?: boolean;
		openOnFocus?: boolean;
		clickBehavior?: "toggle" | "open";
		child?: Snippet<[DropdownMenuTriggerChildProps]>;
		children?: Snippet;
		class?: ClassValue;
		[key: `data-${string}`]: AttributeValue;
	} & Omit<
		HTMLButtonAttributes,
		| "aria-controls"
		| "aria-expanded"
		| "aria-haspopup"
		| "children"
		| "class"
		| "data-state"
		| "disabled"
		| "onclick"
		| "onfocus"
		| "onkeydown"
	>;

	let {
		type = "button",
		disabled = false,
		openOnFocus = false,
		clickBehavior = "toggle",
		child,
		children,
		class: className,
		...rest
	}: Props = $props();

	const menu = getDropdownMenuContext();
	const isDisabled = $derived(menu.disabled || disabled);
	const registerTrigger = (node: HTMLElement) => {
		menu.triggerElement = node;

		return {
			destroy: () => {
				if (menu.triggerElement === node) menu.triggerElement = undefined;
			}
		};
	};

	const handleClick = (event: MouseEvent) => {
		if (isDisabled) {
			event.preventDefault();
			return;
		}

		if (clickBehavior === "open") menu.openMenu();
		else menu.toggleMenu();
	};

	const handleFocus = () => {
		if (isDisabled || !openOnFocus) return;
		menu.openMenu();
	};

	const handleKeydown = (event: KeyboardEvent) => menu.handleTriggerKeydown(event);

	const childProps = $derived({
		id: menu.triggerId,
		"aria-controls": menu.contentId,
		"aria-expanded": menu.open,
		"aria-haspopup": "menu" as const,
		"data-state": menu.state,
		"data-disabled": isDisabled ? (true as const) : undefined,
		disabled: isDisabled ? true : undefined,
		onclick: handleClick,
		onfocus: handleFocus,
		onkeydown: handleKeydown
	});
</script>

{#if child}
	<span
		use:registerTrigger
		data-dropdown-menu-trigger={true}
		data-state={menu.state}
		data-disabled={isDisabled ? true : undefined}
		class={["inline-block", className]}
	>
		{@render child({ props: childProps, open: menu.open, disabled: isDisabled })}
	</span>
{:else}
	<button
		{...rest}
		id={menu.triggerId}
		use:registerTrigger
		{type}
		disabled={isDisabled}
		aria-controls={menu.contentId}
		aria-expanded={menu.open}
		aria-haspopup="menu"
		data-dropdown-menu-trigger={true}
		data-state={menu.state}
		data-disabled={isDisabled ? true : undefined}
		onclick={handleClick}
		onfocus={handleFocus}
		onkeydown={handleKeydown}
		class={[
			"inline-flex min-h-11 items-center justify-center gap-2 rounded-xl border-2 border-foreground bg-surface px-4 text-sm font-extrabold text-foreground",
			"shadow-[3px_3px_0_var(--foreground)] transition-[transform,box-shadow,background-color,color] duration-150 motion-reduce:transition-none",
			"enabled:cursor-pointer enabled:hover:-translate-x-px enabled:hover:-translate-y-px enabled:hover:bg-accent enabled:hover:text-accent-foreground enabled:hover:shadow-[4px_4px_0_var(--foreground)]",
			"enabled:active:translate-x-0.5 enabled:active:translate-y-0.5 enabled:active:shadow-[1px_1px_0_var(--foreground)] data-[state=open]:bg-accent data-[state=open]:text-accent-foreground",
			"disabled:cursor-not-allowed disabled:opacity-45",
			className
		]}
	>
		{@render children?.()}
	</button>
{/if}
