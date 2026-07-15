<script module lang="ts">
	type SeparatorOrientation = "horizontal" | "vertical";

	export { type SeparatorOrientation };
</script>

<script lang="ts">
	import type { ClassValue, HTMLAttributes } from "svelte/elements";

	type Props = {
		orientation: SeparatorOrientation;
		class?: ClassValue;
	} & Omit<HTMLAttributes<HTMLDivElement>, "role" | "aria-orientation">;
	let { orientation = "horizontal", class: className, ...restProps }: Props = $props();
</script>

<div
	{...restProps}
	aria-hidden="true"
	role="separator"
	aria-orientation={orientation}
	class={["bg-border", orientation === "horizontal" && "h-0.5 w-full", orientation === "vertical" && "h-full w-0.5", className]}
></div>
