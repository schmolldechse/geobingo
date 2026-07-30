<script module lang="ts">
	import type { ClassValue, HTMLAttributes } from "svelte/elements";
	import type { ToastPosition } from "./toast-manager.svelte";

	type AttributeValue = string | number | boolean | null | undefined;

	type ToasterProps = {
		position?: ToastPosition;
		duration?: number;
		visibleToasts?: number;
		label?: string;
		hotkey?: string | false;
		class?: ClassValue;
		[key: `data-${string}`]: AttributeValue;
	} & Omit<
		HTMLAttributes<HTMLElement>,
		| "aria-keyshortcuts"
		| "aria-label"
		| "children"
		| "class"
		| "onfocusin"
		| "onfocusout"
		| "onpointerenter"
		| "onpointerleave"
		| "role"
		| "tabindex"
	>;

	export type { ToasterProps };
</script>

<script lang="ts">
	import { onMount } from "svelte";
	import ToastRoot from "./Toast.svelte";
	import { toastManager, type ToastHost } from "./toast-manager.svelte";

	let {
		position = "bottom-right",
		duration = 5000,
		visibleToasts = 3,
		label = "Benachrichtigungen",
		hotkey = "F8",
		class: className,
		...restProps
	}: ToasterProps = $props();

	let viewport: HTMLElement | null = $state(null);
	let hostActive = $state(false);
	let reducedMotion = $state(false);
	let toastHost: ToastHost | undefined;
	let pointerPaused = false;
	let focusPaused = false;
	let documentPaused = false;
	let focusReturnElement: HTMLElement | null = null;

	const normalizedDuration = $derived(Math.max(0, Number.isFinite(duration) ? duration : 5000));
	const normalizedVisibleToasts = $derived(Math.max(1, Math.floor(Number.isFinite(visibleToasts) ? visibleToasts : 1)));
	const renderedToasts = $derived(hostActive ? toastManager.visibleToasts : []);
	const openToasts = $derived(renderedToasts.filter((toast) => toast.open));

	const positionClasses: Record<ToastPosition, string> = {
		"top-left": "top-[max(1rem,env(safe-area-inset-top))] left-[max(1rem,env(safe-area-inset-left))]",
		"top-center": "top-[max(1rem,env(safe-area-inset-top))] left-1/2 -translate-x-1/2",
		"top-right": "top-[max(1rem,env(safe-area-inset-top))] right-[max(1rem,env(safe-area-inset-right))]",
		"bottom-left": "bottom-[max(1rem,env(safe-area-inset-bottom))] left-[max(1rem,env(safe-area-inset-left))]",
		"bottom-center": "bottom-[max(1rem,env(safe-area-inset-bottom))] left-1/2 -translate-x-1/2",
		"bottom-right": "right-[max(1rem,env(safe-area-inset-right))] bottom-[max(1rem,env(safe-area-inset-bottom))]"
	};

	const syncPausedState = (): void => {
		toastHost?.setPaused(pointerPaused || focusPaused || documentPaused);
	};

	const handlePointerEnter = (): void => {
		pointerPaused = true;
		syncPausedState();
	};

	const handlePointerLeave = (): void => {
		pointerPaused = false;
		syncPausedState();
	};

	const handleFocusIn = (): void => {
		focusPaused = true;
		syncPausedState();
	};

	const handleFocusOut = (event: FocusEvent): void => {
		if (viewport && event.relatedTarget instanceof Node && viewport.contains(event.relatedTarget)) return;

		focusPaused = false;
		syncPausedState();
	};

	const focusViewport = (): void => {
		if (!viewport || openToasts.length === 0) return;

		if (
			document.activeElement instanceof HTMLElement &&
			document.activeElement !== document.body &&
			!viewport.contains(document.activeElement)
		) {
			focusReturnElement = document.activeElement;
		}

		viewport.focus({ preventScroll: true });
	};

	const restorePreviousFocus = (): void => {
		const target = focusReturnElement;
		focusReturnElement = null;

		if (!target?.isConnected || target.closest("[inert]") || target.matches(":disabled, [aria-disabled='true']")) {
			viewport?.blur();
			return;
		}

		target.focus({ preventScroll: true });
	};

	const handleToastClose = (id: string): void => {
		const activeElement = document.activeElement;
		const focusedRoot = activeElement instanceof Element ? activeElement.closest<HTMLElement>("[data-toast-root]") : null;
		const shouldRepairFocus = focusedRoot?.dataset.toastId === id || activeElement === viewport;
		if (!shouldRepairFocus) return;

		queueMicrotask(() => {
			const remainingVisible = toastManager.visibleToasts.some((toast) => toast.open);
			if (remainingVisible) {
				viewport?.focus({ preventScroll: true });
				return;
			}

			restorePreviousFocus();
		});
	};

	const handleWindowKeydown = (event: KeyboardEvent): void => {
		if (event.defaultPrevented) return;

		if (hotkey && event.key === hotkey) {
			if (openToasts.length === 0) return;

			event.preventDefault();
			focusViewport();
			return;
		}

		if (event.key !== "Escape" || !viewport || !viewport.contains(document.activeElement)) return;

		const activeElement = document.activeElement;
		const focusedRoot = activeElement instanceof Element ? activeElement.closest<HTMLElement>("[data-toast-root]") : null;
		const focusedId = focusedRoot?.dataset.toastId;
		const newestVisibleId = openToasts.at(-1)?.id;
		const targetId = focusedId ?? newestVisibleId;
		if (!targetId) return;

		event.preventDefault();
		toastManager.dismiss(targetId);
	};

	$effect(() => {
		if (!hostActive || !toastHost) return;

		toastHost.configure({
			duration: normalizedDuration,
			visibleToasts: normalizedVisibleToasts,
			exitDuration: reducedMotion ? 0 : 150
		});
	});

	onMount(() => {
		const reducedMotionQuery = window.matchMedia("(prefers-reduced-motion: reduce)");
		reducedMotion = reducedMotionQuery.matches;
		documentPaused = document.hidden;

		toastHost = toastManager.connect(
			{
				duration: normalizedDuration,
				visibleToasts: normalizedVisibleToasts,
				exitDuration: reducedMotion ? 0 : 150
			},
			{
				paused: documentPaused,
				onClose: handleToastClose
			}
		);
		hostActive = toastHost.active;

		const handleVisibilityChange = (): void => {
			documentPaused = document.hidden;

			if (!documentPaused) {
				pointerPaused = viewport?.matches(":hover") ?? false;
				focusPaused = viewport?.contains(document.activeElement) ?? false;
			}

			syncPausedState();
		};

		const handleReducedMotionChange = (event: MediaQueryListEvent): void => {
			reducedMotion = event.matches;
		};

		const handleDocumentFocusIn = (event: FocusEvent): void => {
			const target = event.target;
			if (!(target instanceof HTMLElement) || !viewport || viewport.contains(target) || target === document.body) return;

			focusReturnElement = target;
		};

		window.addEventListener("keydown", handleWindowKeydown);
		document.addEventListener("visibilitychange", handleVisibilityChange);
		document.addEventListener("focusin", handleDocumentFocusIn);
		reducedMotionQuery.addEventListener("change", handleReducedMotionChange);

		return () => {
			hostActive = false;
			window.removeEventListener("keydown", handleWindowKeydown);
			document.removeEventListener("visibilitychange", handleVisibilityChange);
			document.removeEventListener("focusin", handleDocumentFocusIn);
			reducedMotionQuery.removeEventListener("change", handleReducedMotionChange);
			toastHost?.disconnect();
			toastHost = undefined;
		};
	});
</script>

<section
	{...restProps}
	bind:this={viewport}
	aria-label={label}
	aria-keyshortcuts={hotkey || undefined}
	tabindex="-1"
	data-toast-viewport
	data-position={position}
	data-state={openToasts.length > 0 ? "open" : "closed"}
	class={[
		"pointer-events-none fixed z-50 flex w-[calc(100vw-max(1rem,env(safe-area-inset-left))-max(1rem,env(safe-area-inset-right)))] max-w-[26rem] flex-col gap-3 rounded-2xl focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-current",
		positionClasses[position],
		className
	]}
	onpointerenter={handlePointerEnter}
	onpointerleave={handlePointerLeave}
	onfocusin={handleFocusIn}
	onfocusout={handleFocusOut}
>
	{#each renderedToasts as item (item.id)}
		<ToastRoot
			id={item.id}
			open={item.open}
			title={item.title}
			description={item.description}
			variant={item.variant}
			priority={item.priority}
			action={item.action}
			dismissible={item.dismissible}
			data-toast-managed=""
			onopenchange={(open) => {
				if (!open) toastManager.dismiss(item.id);
			}}
			ontransitionend={(event) => {
				if (event.target === event.currentTarget && event.propertyName === "opacity" && !item.open) {
					toastManager.finalize(item.id);
				}
			}}
		/>
	{/each}
</section>
