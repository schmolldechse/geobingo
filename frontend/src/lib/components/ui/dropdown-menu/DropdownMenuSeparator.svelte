<script lang="ts">
	import type { ClassValue, HTMLAttributes } from "svelte/elements";
	import Separator from "../Separator.svelte";

	type Props = {
		class?: ClassValue;
	} & Omit<HTMLAttributes<HTMLDivElement>, "role">;
	let { class: className, ...restProps }: Props = $props();
</script>

<Separator {...restProps} orientation="horizontal" class={["my-1.5", className]} data-dropdown-menu-separator />
