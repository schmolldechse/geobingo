<script module lang="ts">
	type BadgeTone = "neutral" | "primary" | "secondary" | "accent";

	export type { BadgeTone };
</script>

<script lang="ts">
	import type { Snippet } from "svelte";
	import type { ClassValue, HTMLAttributes } from "svelte/elements";

	type BadgeContent = { text: string; children?: never } | { text?: never; children: Snippet };
	type Props = BadgeContent &
		Omit<HTMLAttributes<HTMLSpanElement>, "children" | "class"> & {
			tone?: BadgeTone;
			class?: ClassValue;
		};

	let { tone = "neutral", text, children, class: className, ...restProps }: Props = $props();

	const toneClasses: Record<BadgeTone, string> = {
		neutral: "bg-surface-muted text-foreground",
		primary: "bg-primary text-primary-foreground",
		secondary: "bg-secondary text-secondary-foreground",
		accent: "bg-accent text-accent-foreground"
	};
</script>

<span
	{...restProps}
	data-badge-root
	data-tone={tone}
	class={[
		"inline-flex max-w-full items-center gap-1 rounded-full px-2 py-1 text-[0.62rem] leading-none font-black",
		toneClasses[tone],
		className
	]}
>
	{#if children}
		{@render children()}
	{:else}
		{text}
	{/if}
</span>
