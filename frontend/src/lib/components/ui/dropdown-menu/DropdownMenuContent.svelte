<script lang="ts">
	import type { Snippet } from "svelte";
	import type { Attachment } from "svelte/attachments";
	import type { ClassValue, HTMLAttributes } from "svelte/elements";
	import { getDropdownMenuContext, type DropdownMenuAlign } from "./dropdown-menu-context.svelte";
	import {
		getDropdownMenuPosition,
		type DropdownMenuPosition,
		type DropdownMenuResolvedSide,
		type DropdownMenuSide
	} from "./dropdown-menu-position";

	type AttributeValue = string | number | boolean | null | undefined;
	type Props = {
		align?: DropdownMenuAlign;
		side?: DropdownMenuSide;
		sideOffset?: number;
		collisionPadding?: number;
		matchTriggerWidth?: boolean;
		children?: Snippet;
		class?: ClassValue;
		[key: `data-${string}`]: AttributeValue;
	} & Omit<HTMLAttributes<HTMLDivElement>, "children" | "class" | "onkeydown">;

	let {
		align = "start",
		side = "auto",
		sideOffset = 6,
		collisionPadding = 16,
		matchTriggerWidth = false,
		children,
		class: className,
		style,
		...rest
	}: Props = $props();

	const menu = getDropdownMenuContext();
	let position = $state<DropdownMenuPosition | null>(null);
	let resolvedSide = $state<DropdownMenuResolvedSide>("bottom");
	const maximumHeightInRem = 24;

	const updatePosition = (node: HTMLDivElement) => {
		const trigger = menu.triggerElement;
		if (!trigger) return;

		const triggerRect = trigger.getBoundingClientRect();
		const contentRect = node.getBoundingClientRect();
		const rootFontSize = Number.parseFloat(getComputedStyle(document.documentElement).fontSize) || 16;
		const nextPosition = getDropdownMenuPosition({
			triggerRect,
			contentSize: contentRect,
			viewportSize: {
				width: document.documentElement.clientWidth,
				height: document.documentElement.clientHeight
			},
			align,
			side,
			sideOffset,
			collisionPadding,
			maximumHeight: maximumHeightInRem * rootFontSize
		});

		position = nextPosition;
		resolvedSide = nextPosition.side;
	};

	const registerContent: Attachment<HTMLDivElement> = (node) => {
		position = null;
		resolvedSide = "bottom";
		menu.contentElement = node;
		const portalAnchor = document.createComment("dropdown-menu-portal");
		node.before(portalAnchor);
		document.body.append(node);
		updatePosition(node);

		let scheduledFrame: number | undefined;
		const schedulePositionUpdate = () => {
			if (scheduledFrame !== undefined) return;
			scheduledFrame = requestAnimationFrame(() => {
				scheduledFrame = undefined;
				updatePosition(node);
			});
		};
		const trigger = menu.triggerElement;
		const resizeObserver = new ResizeObserver((entries) => {
			if (trigger && entries.some((entry) => entry.target === trigger)) menu.updateTriggerWidth();
			schedulePositionUpdate();
		});

		resizeObserver.observe(node);
		if (trigger) resizeObserver.observe(trigger);
		window.addEventListener("resize", schedulePositionUpdate);
		window.addEventListener("scroll", schedulePositionUpdate, true);
		queueMicrotask(schedulePositionUpdate);

		return () => {
			if (scheduledFrame !== undefined) cancelAnimationFrame(scheduledFrame);
			resizeObserver.disconnect();
			window.removeEventListener("resize", schedulePositionUpdate);
			window.removeEventListener("scroll", schedulePositionUpdate, true);
			if (menu.contentElement === node) menu.contentElement = undefined;
			node.remove();
			portalAnchor.remove();
		};
	};
	const widthStyle = $derived(matchTriggerWidth && menu.triggerWidth > 0 ? `width: ${menu.triggerWidth}px` : undefined);
	const contentStyle = $derived(
		[
			position ? `top: ${position.top}px` : "top: 0",
			position ? `left: ${position.left}px` : "left: 0",
			position ? `max-height: min(24rem, ${position.availableHeight}px)` : undefined,
			position ? undefined : "visibility: hidden",
			widthStyle,
			style
		]
			.filter(Boolean)
			.join("; ")
	);
</script>

{#if menu.open}
	<div
		{...rest}
		id={menu.contentId}
		{@attach registerContent}
		role="menu"
		tabindex="-1"
		aria-labelledby={menu.triggerId}
		data-dropdown-menu-content={true}
		data-state={menu.state}
		data-side={resolvedSide}
		onkeydown={menu.handleContentKeydown}
		style={contentStyle}
		class={[
			"border-foreground bg-surface text-foreground fixed z-50 max-w-[calc(100vw-2rem)] min-w-52 overflow-y-auto rounded-xl border-2 p-1.5 shadow-[var(--shadow-paper-raised)] outline-none",
			"transition-[opacity,transform] duration-150 motion-reduce:transition-none",
			className
		]}
	>
		{@render children?.()}
	</div>
{/if}
