<script lang="ts">
	import X from "@lucide/svelte/icons/x";
	import type { Snippet } from "svelte";
	import type { ClassValue, HTMLDialogAttributes } from "svelte/elements";
	import Button from "./Button.svelte";

	type Props = Omit<HTMLDialogAttributes, "children" | "open" | "onclose" | "title"> & {
		isVisible?: boolean;
		title?: string | Snippet;
		children: Snippet;
		actions?: Snippet;
		showHeader?: boolean;
		showActions?: boolean;
		showCloseButton?: boolean;
		isModal?: boolean;
		clickOutsideToClose?: boolean;
		class?: ClassValue;
		onclose?: () => void;
	};
	let {
		isVisible = $bindable(false),
		title,
		children,
		actions,
		showHeader = true,
		showActions = true,
		showCloseButton = true,
		isModal = true,
		clickOutsideToClose = true,
		class: className,
		onclose,
		...restProps
	}: Props = $props();

	const id = $props.id();
	const titleId = `${id}-title`;

	let dialog: HTMLDialogElement | null = $state(null);

	let hasEmittedClose = $state(false);
	let suppressNextNativeClose = $state(false);

	const hasVisibleTitle = $derived(showHeader && Boolean(title));
	const hasVisibleActions = $derived(showActions && Boolean(actions));
	const ariaLabel = $derived(hasVisibleTitle ? undefined : (restProps["aria-label"] ?? "Dialog"));

	$effect(() => {
		if (!dialog) return;

		if (isVisible) {
			hasEmittedClose = false;
			suppressNextNativeClose = false;

			if (!dialog.open) {
				if (isModal) dialog.showModal();
				else dialog.show();
			}

			return;
		}

		if (dialog.open) {
			suppressNextNativeClose = true;
			dialog.close();
		}
	});

	const emitClose = () => {
		if (hasEmittedClose) return;

		hasEmittedClose = true;
		onclose?.();
	};

	const requestClose = () => {
		if (!isVisible && !dialog?.open) return;

		emitClose();
		isVisible = false;

		if (dialog?.open) {
			suppressNextNativeClose = true;
			dialog.close();
		}
	};

	const handleNativeClose = () => {
		if (suppressNextNativeClose) {
			suppressNextNativeClose = false;
			return;
		}

		if (!isVisible) return;

		emitClose();
		isVisible = false;
	};

	const handleCancel = (event: Event) => {
		event.preventDefault();
		requestClose();
	};

	const pointerIsInsideDialog = (event: PointerEvent) => {
		if (!dialog) return false;

		const rect = dialog.getBoundingClientRect();
		return (
			event.clientX >= rect.left && event.clientX <= rect.right && event.clientY >= rect.top && event.clientY <= rect.bottom
		);
	};

	const handleDialogPointerDown = (event: PointerEvent) => {
		if (!dialog || !isVisible || !isModal || !clickOutsideToClose) return;
		if (event.target === dialog && !pointerIsInsideDialog(event)) requestClose();
	};

	const handleWindowPointerDown = (event: PointerEvent) => {
		if (!dialog || !isVisible || isModal || !clickOutsideToClose) return;
		if (!(event.target instanceof Node)) return;
		if (dialog.contains(event.target)) return;

		requestClose();
	};
</script>

<svelte:window onpointerdown={handleWindowPointerDown} />

<dialog
	{...restProps}
	bind:this={dialog}
	onclose={handleNativeClose}
	oncancel={handleCancel}
	onpointerdown={handleDialogPointerDown}
	class={[
		"border-foreground bg-surface text-foreground m-0 max-h-[calc(100dvh-2rem)] w-[min(calc(100%-2rem),32rem)] overflow-hidden rounded-2xl border-2 p-0 shadow-[var(--shadow-paper-raised)]",
		className
	]}
	aria-labelledby={hasVisibleTitle ? titleId : undefined}
	aria-label={ariaLabel}
	data-dialog
	data-state={isVisible ? "open" : "closed"}
>
	<div class="flex max-h-[calc(100dvh-2rem)] flex-col gap-y-4 overflow-y-auto p-4 sm:p-6">
		{#if showHeader}
			<div class="flex items-start justify-between gap-x-4">
				{#if typeof title === "string"}
					<h2 id={titleId} class="text-foreground m-0 text-xl font-black tracking-tight">{title}</h2>
				{:else if title}
					<div id={titleId}>
						{@render title()}
					</div>
				{/if}

				{#if showCloseButton}
					<Button size="sm" variant="ghost" aria-label="Close dialog" onclick={requestClose} class="-mt-1 -mr-2 p-1!">
						<X aria-hidden="true" size={18} />
					</Button>
				{/if}
			</div>
		{/if}

		{@render children()}

		{#if hasVisibleActions}
			<footer class="mt-2 flex flex-col-reverse gap-2 sm:flex-row sm:justify-end sm:gap-3">
				{@render actions?.()}
			</footer>
		{/if}
	</div>
</dialog>
