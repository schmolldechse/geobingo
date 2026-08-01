<script lang="ts">
	import type { Snippet } from "svelte";
	import type { ClassValue, HTMLAttributes } from "svelte/elements";

	type Props = {
		children?: Snippet;
		class?: ClassValue;
	} & Omit<HTMLAttributes<HTMLDivElement>, "children" | "class">;

	let { children, class: className, ...rest }: Props = $props();
</script>

<div {...rest} class={["text-muted px-3 pt-2 pb-1 text-xs font-extrabold tracking-[0.12em] uppercase", className]}>
	{@render children?.()}
</div>
