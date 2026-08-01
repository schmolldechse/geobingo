<script lang="ts">
	import type { ClassValue, HTMLAttributes } from "svelte/elements";

	type Props = {
		class?: ClassValue;
	} & Omit<HTMLAttributes<HTMLDivElement>, "aria-hidden" | "class">;

	let { class: className, ...restProps }: Props = $props();
</script>

<div
	{...restProps}
	aria-hidden="true"
	data-skeleton
	class={[
		"border-border/70 bg-surface-muted relative overflow-hidden border shadow-[inset_0_1px_0_color-mix(in_srgb,var(--surface)_70%,transparent)]",
		"motion-safe:animate-pulse motion-reduce:animate-none",
		className
	]}
></div>
