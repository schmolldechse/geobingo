<script lang="ts">
	import type { LayoutProps } from "./$types";

	let { children }: LayoutProps = $props();
</script>

<div class="bg-background text-foreground min-h-dvh">
	{@render children()}
</div>
