<script module lang="ts">
	import type { Snippet } from "svelte";
	import type { ClassValue, HTMLAttributes } from "svelte/elements";
	import type { ToastAction, ToastPriority, ToastVariant } from "./toast-manager.svelte";

	type AttributeValue = string | number | boolean | null | undefined;

	type ToastProps = {
		open?: boolean;
		variant?: ToastVariant;
		priority?: ToastPriority;
		title: string | Snippet;
		description?: string | Snippet;
		action?: ToastAction;
		dismissible?: boolean;
		icon?: Snippet | false;
		children?: Snippet;
		onopenchange?: (open: boolean) => void;
		class?: ClassValue;
		[key: `data-${string}`]: AttributeValue;
	} & Omit<
		HTMLAttributes<HTMLDivElement>,
		"aria-describedby" | "aria-labelledby" | "aria-live" | "children" | "class" | "role" | "title"
	>;

	export type { ToastProps };
</script>

<script lang="ts">
	import Bell from "@lucide/svelte/icons/bell";
	import CheckCircle2 from "@lucide/svelte/icons/circle-check-big";
	import CircleAlert from "@lucide/svelte/icons/circle-alert";
	import Info from "@lucide/svelte/icons/info";
	import TriangleAlert from "@lucide/svelte/icons/triangle-alert";
	import X from "@lucide/svelte/icons/x";
	import { prefersReducedMotion } from "svelte/motion";
	import Button from "../Button.svelte";
	import { resolveToastLayout } from "./toast-layout";

	const uid = $props.id();
	let {
		id = `${uid}-toast`,
		open = $bindable(true),
		variant = "default",
		priority,
		title,
		description,
		action,
		dismissible = true,
		icon,
		children,
		onopenchange,
		ontransitionend,
		class: className,
		...restProps
	}: ToastProps = $props();

	let present = $state(open);

	const titleId = $derived(`${id}-title`);
	const descriptionId = $derived(description === undefined ? undefined : `${id}-description`);
	const managed = $derived(restProps["data-toast-managed"] !== undefined);
	const layout = $derived(resolveToastLayout({ description, action, children }));
	const resolvedPriority = $derived<ToastPriority>(priority ?? (variant === "error" ? "assertive" : "polite"));
	const resolvedRole = $derived(variant === "error" ? "alert" : "status");
	const DefaultIcon = $derived(
		variant === "success"
			? CheckCircle2
			: variant === "info"
				? Info
				: variant === "warning"
					? TriangleAlert
					: variant === "error"
						? CircleAlert
						: Bell
	);

	const requestClose = (): void => {
		if (!open) return;

		open = false;
		onopenchange?.(false);
	};

	const handleAction = async (event: MouseEvent): Promise<void> => {
		if (!action) return;

		try {
			await action.onclick(event);
		} finally {
			if (!event.defaultPrevented) requestClose();
		}
	};

	const handleTransitionEnd: NonNullable<ToastProps["ontransitionend"]> = (event) => {
		ontransitionend?.(event);

		if (event.target === event.currentTarget && event.propertyName === "opacity" && !open && !managed) {
			present = false;
		}
	};

	$effect(() => {
		if (open) {
			present = true;
			return;
		}

		if (!present || managed) return;

		if (prefersReducedMotion.current) {
			present = false;
			return;
		}

		const closeTimer = window.setTimeout(() => {
			present = false;
		}, 250);

		return () => window.clearTimeout(closeTimer);
	});
</script>

{#if present}
	<div
		{...restProps}
		{id}
		role={resolvedRole}
		aria-live={resolvedPriority}
		aria-labelledby={titleId}
		aria-describedby={descriptionId}
		data-toast-root
		data-toast-id={id}
		data-state={open ? "open" : "closed"}
		data-variant={variant}
		data-priority={resolvedPriority}
		data-layout={layout}
		ontransitionend={handleTransitionEnd}
		class={[
			"toast-root pointer-events-auto relative grid w-full grid-cols-[auto_minmax(0,1fr)_auto] overflow-hidden rounded-2xl border-2 shadow-[var(--shadow-paper-raised)]",
			layout === "compact" && "min-h-[3.125rem] items-center gap-x-2.5 px-2 py-1.5",
			layout !== "compact" && "items-start gap-x-3 p-4",
			className
		]}
	>
		{#if icon !== false}
			<span
				class={[
					"toast-icon inline-flex shrink-0 items-center justify-center",
					layout === "compact" && "size-[1.875rem] rounded-[0.5625rem]",
					layout !== "compact" && "mt-0.5 size-9 rounded-xl"
				]}
				aria-hidden="true"
			>
				{#if icon}
					{@render icon()}
				{:else}
					<DefaultIcon size={19} strokeWidth={2.5} />
				{/if}
			</span>
		{/if}

		<div
			class={[
				"min-w-0",
				layout === "compact" && "overflow-hidden",
				icon === false && dismissible && "col-span-2 col-start-1",
				icon === false && !dismissible && "col-span-3 col-start-1"
			]}
		>
			{#if typeof title === "string"}
				<p id={titleId} class={["m-0 text-sm leading-5 font-black", layout === "compact" && "truncate"]}>{title}</p>
			{:else}
				<div id={titleId} class={["text-sm leading-5 font-black", layout === "compact" && "truncate"]}>
					{@render title()}
				</div>
			{/if}

			{#if description !== undefined}
				{#if typeof description === "string"}
					<p id={descriptionId} class="mt-1 mb-0 text-sm leading-5">{description}</p>
				{:else}
					<div id={descriptionId} class="mt-1 text-sm leading-5">
						{@render description()}
					</div>
				{/if}
			{/if}

			{#if children}
				<div class="mt-2">
					{@render children()}
				</div>
			{/if}

			{#if action}
				<Button
					size="sm"
					variant="outline"
					class="toast-action mt-3 w-fit shadow-none! hover:shadow-none! active:shadow-none!"
					onclick={handleAction}
				>
					{action.label}
				</Button>
			{/if}
		</div>

		{#if dismissible}
			<Button
				size="icon"
				variant="ghost"
				aria-label="Benachrichtigung schließen"
				class={[
					"border-transparent! p-0! text-current! shadow-none! hover:bg-current/10! hover:shadow-none! focus-visible:outline-current! active:shadow-none!",
					layout === "compact" && "size-8! min-h-8!",
					layout !== "compact" && "-mt-1 -mr-1 size-8 min-h-8"
				]}
				onclick={requestClose}
			>
				<X aria-hidden="true" size={17} strokeWidth={2.5} />
			</Button>
		{/if}
	</div>
{/if}

<style>
	.toast-root {
		border-color: var(--foreground);
		background: var(--surface);
		color: var(--foreground);
		transition:
			opacity 150ms cubic-bezier(0.33, 1, 0.68, 1),
			transform 150ms cubic-bezier(0.33, 1, 0.68, 1);
	}

	.toast-root[data-variant="success"] {
		background: var(--secondary);
		color: var(--secondary-foreground);
	}

	.toast-root[data-variant="info"] {
		border-color: var(--secondary);
	}

	.toast-root[data-variant="warning"] {
		background: var(--accent);
		color: var(--accent-foreground);
	}

	.toast-root[data-variant="error"] {
		background: var(--primary);
		color: var(--primary-foreground);
	}

	.toast-icon {
		background: var(--surface-muted);
		color: var(--foreground);
	}

	.toast-root[data-variant="success"] .toast-icon {
		background: color-mix(in srgb, var(--secondary-foreground) 15%, transparent);
		color: var(--secondary-foreground);
	}

	.toast-root[data-variant="info"] .toast-icon {
		background: color-mix(in srgb, var(--secondary) 15%, transparent);
		color: var(--secondary);
	}

	.toast-root[data-variant="warning"] .toast-icon {
		background: color-mix(in srgb, var(--accent-foreground) 15%, transparent);
		color: var(--accent-foreground);
	}

	.toast-root[data-variant="error"] .toast-icon {
		background: color-mix(in srgb, var(--primary-foreground) 15%, transparent);
		color: var(--primary-foreground);
	}

	.toast-root[data-state="closed"] {
		pointer-events: none;
		opacity: 0;
		transform: translateY(0.75rem);
	}

	:global(.toast-action:focus-visible) {
		outline: none;
		box-shadow:
			0 0 0 2px var(--surface),
			0 0 0 4px var(--foreground) !important;
	}

	@media (prefers-reduced-motion: reduce) {
		.toast-root {
			transition-duration: 0ms;
		}
	}

	@media (forced-colors: active) {
		:global(.toast-action:focus-visible) {
			outline: 2px solid CanvasText;
			outline-offset: 2px;
		}
	}
</style>
