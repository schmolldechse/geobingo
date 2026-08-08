<script lang="ts">
	import ChevronLeft from "@lucide/svelte/icons/chevron-left";
	import ChevronRight from "@lucide/svelte/icons/chevron-right";
	import type { Snippet } from "svelte";
	import type { ClassValue, HTMLAttributes } from "svelte/elements";
	import { getTabsContext } from "./tabs-context.svelte";

	type ScrollDirection = -1 | 1;

	type Props = {
		children?: Snippet;
		class?: ClassValue;
	} & Omit<HTMLAttributes<HTMLDivElement>, "aria-orientation" | "children" | "class" | "role">;
	let { children, class: className, ...restProps }: Props = $props();

	const tabs = getTabsContext();

	const scrollButtonClasses =
		"border-foreground bg-surface/95 text-secondary absolute top-1/2 z-20 grid size-9 -translate-y-1/2 place-items-center rounded-full border-2 shadow-[2px_2px_0_var(--foreground)] backdrop-blur-sm transition-[background-color,color,box-shadow] duration-150 motion-reduce:transition-none hover:bg-secondary hover:text-secondary-foreground active:shadow-none";

	let viewport: HTMLDivElement | null = $state(null);
	let canScrollLeft: boolean = $state(false);
	let canScrollRight: boolean = $state(false);

	const updateScrollControls = (element: HTMLDivElement): void => {
		const maxScrollLeft = Math.max(0, element.scrollWidth - element.clientWidth);
		const currentScrollLeft = Math.max(0, element.scrollLeft);

		canScrollLeft = currentScrollLeft > 1;
		canScrollRight = currentScrollLeft < maxScrollLeft - 1;
	};

	const observeScrollViewport = (element: HTMLDivElement): (() => void) => {
		viewport = element;

		const update = () => updateScrollControls(element);
		const resizeObserver = new ResizeObserver(update);
		const frame = requestAnimationFrame(update);
		const list = element.querySelector<HTMLElement>("[data-tabs-list]");

		element.addEventListener("scroll", update, { passive: true });
		resizeObserver.observe(element);
		if (list) resizeObserver.observe(list);

		return () => {
			cancelAnimationFrame(frame);
			resizeObserver.disconnect();
			element.removeEventListener("scroll", update);
			if (viewport === element) viewport = null;
		};
	};

	const scrollTabs = (direction: ScrollDirection): void => {
		if (!viewport) return;

		const distance = Math.max(viewport.clientWidth * 0.75, 160);
		const behavior = window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "auto" : "smooth";

		viewport.scrollBy({ left: direction * distance, behavior });
	};
</script>

<div
	data-tabs-list-container
	data-orientation={tabs.orientation}
	data-disabled={tabs.disabled ? "" : undefined}
	data-can-scroll-left={canScrollLeft ? "" : undefined}
	data-can-scroll-right={canScrollRight ? "" : undefined}
	class={[
		"relative min-w-0",
		"data-[orientation=horizontal]:w-full",
		"data-[orientation=vertical]:w-full sm:data-[orientation=vertical]:w-auto"
	]}
>
	<div
		{@attach observeScrollViewport}
		data-tabs-list-viewport
		data-orientation={tabs.orientation}
		class={[
			"min-w-0",
			"data-[orientation=horizontal]:w-full data-[orientation=horizontal]:overflow-x-auto data-[orientation=horizontal]:overflow-y-hidden data-[orientation=horizontal]:overscroll-x-contain",
			"data-[orientation=vertical]:w-full sm:data-[orientation=vertical]:w-auto"
		]}
	>
		<div
			{...restProps}
			role="tablist"
			aria-orientation={tabs.orientation}
			data-tabs-list
			data-orientation={tabs.orientation}
			data-disabled={tabs.disabled ? "" : undefined}
			class={[
				"relative flex min-w-0 gap-1",
				"data-[orientation=horizontal]:border-border data-[orientation=horizontal]:w-max data-[orientation=horizontal]:min-w-full data-[orientation=horizontal]:items-end data-[orientation=horizontal]:border-b-2 data-[orientation=horizontal]:px-[0.2rem]",
				"data-[orientation=vertical]:border-border data-[orientation=vertical]:w-full data-[orientation=vertical]:flex-col data-[orientation=vertical]:border-r-2",
				"sm:data-[orientation=vertical]:w-auto",
				className
			]}
		>
			{@render children?.()}
		</div>
	</div>

	{#if tabs.orientation === "horizontal" && canScrollLeft}
		<button
			type="button"
			aria-label="Scroll tabs left"
			data-tabs-scroll-button
			data-direction="left"
			class={[scrollButtonClasses, "left-1", "cursor-pointer"]}
			onclick={() => scrollTabs(-1)}
		>
			<ChevronLeft aria-hidden="true" size={20} strokeWidth={3} />
		</button>
	{/if}

	{#if tabs.orientation === "horizontal" && canScrollRight}
		<button
			type="button"
			aria-label="Scroll tabs right"
			data-tabs-scroll-button
			data-direction="right"
			class={[scrollButtonClasses, "right-1", "cursor-pointer"]}
			onclick={() => scrollTabs(1)}
		>
			<ChevronRight aria-hidden="true" size={20} strokeWidth={3} />
		</button>
	{/if}
</div>

<style>
	[data-tabs-list-viewport][data-orientation="horizontal"] {
		scrollbar-width: none;
	}

	[data-tabs-list-viewport][data-orientation="horizontal"]::-webkit-scrollbar {
		display: none;
	}
</style>
