<script lang="ts">
	import { Toaster } from "$lib/components/ui/toast";
	import type { LayoutProps } from "./$types";
	import "../app.css";

	let { children }: LayoutProps = $props();
</script>

{@render children()}
<Toaster />
