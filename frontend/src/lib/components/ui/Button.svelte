<script module lang="ts">
	type ButtonVariant = "primary" | "secondary" | "accent" | "outline" | "ghost" | "destructive";
	type ButtonSize = "sm" | "md" | "lg" | "icon";

	export type { ButtonVariant, ButtonSize };
</script>

<script lang="ts">
	import type { Snippet } from "svelte";
	import type { ClassValue, HTMLAnchorAttributes, HTMLButtonAttributes } from "svelte/elements";

	const id = $props.id();

	type Props = {
		ref?: HTMLButtonElement | HTMLAnchorElement | null;
		variant?: ButtonVariant;
		size?: ButtonSize;
		href?: string;
		disabled?: boolean;
		children?: Snippet;
		class?: ClassValue;
		type?: HTMLButtonAttributes["type"];
		onclick?: (event: MouseEvent) => void;
		target?: string;
		rel?: string;
	} & Omit<HTMLButtonAttributes, "children" | "disabled" | "onclick" | "type">;
	let {
		variant = "primary",
		size = "md",
		href,
		disabled = false,
		ref = $bindable(null),
		children,
		class: className,
		type = "button",
		onclick,
		tabindex,
		...restProps
	}: Props = $props();

	const baseClasses = [
		"inline-flex cursor-pointer items-center justify-center gap-2 rounded-xl border-2 font-extrabold no-underline",
		"transition-[transform,box-shadow,opacity,background-color,color,border-color] duration-150 motion-reduce:transition-none",
		"hover:-translate-x-px hover:-translate-y-px active:translate-x-0.5 active:translate-y-0.5",
		"disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-45",
		"data-[disabled]:pointer-events-none data-[disabled]:cursor-not-allowed data-[disabled]:opacity-45"
	];

	const raisedClasses = [
		"shadow-[3px_3px_0_var(--foreground)]",
		"hover:shadow-[4px_4px_0_var(--foreground)] active:shadow-[1px_1px_0_var(--foreground)]"
	];

	const variantClasses: Record<ButtonVariant, string> = {
		primary: "border-foreground bg-primary text-primary-foreground",
		secondary: "border-foreground bg-secondary text-secondary-foreground",
		accent: "border-foreground bg-accent text-accent-foreground",
		outline: "border-foreground bg-surface text-foreground",
		ghost: "border-transparent bg-transparent text-secondary hover:bg-surface-muted",
		destructive: "border-primary bg-transparent text-primary hover:bg-primary hover:text-primary-foreground"
	};

	const sizeClasses: Record<ButtonSize, string> = {
		sm: "min-h-9 px-3 text-xs",
		md: "min-h-11 px-4 text-sm",
		lg: "min-h-13 px-5 text-base",
		icon: "size-11 shrink-0 p-0"
	};

	const anchorProps = $derived(restProps as unknown as HTMLAnchorAttributes);
	const buttonProps = $derived(restProps as HTMLButtonAttributes);
</script>

{#if href !== undefined}
	<a
		{...anchorProps}
		bind:this={ref}
		{id}
		class={[baseClasses, variant !== "ghost" && raisedClasses, variantClasses[variant], sizeClasses[size], className]}
		href={disabled ? undefined : href}
		target={anchorProps.target}
		rel={anchorProps.rel}
		tabindex={disabled ? -1 : tabindex}
		aria-disabled={disabled ? "true" : undefined}
		data-button-root
		data-variant={variant}
		data-size={size}
		data-disabled={disabled ? "" : undefined}
		onclick={(event: MouseEvent) => {
			if (disabled) {
				event.preventDefault();
				event.stopImmediatePropagation();
				return;
			}

			onclick?.(event);
		}}
	>
		{@render children?.()}
	</a>
{:else}
	<button
		{...buttonProps}
		bind:this={ref}
		{id}
		class={[baseClasses, variant !== "ghost" && raisedClasses, variantClasses[variant], sizeClasses[size], className]}
		{type}
		{disabled}
		{tabindex}
		data-button-root
		data-variant={variant}
		data-size={size}
		data-disabled={disabled ? "" : undefined}
		{onclick}
	>
		{@render children?.()}
	</button>
{/if}
