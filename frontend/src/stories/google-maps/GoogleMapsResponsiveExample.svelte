<script lang="ts">
	import Button from "$lib/components/ui/Button.svelte";
	import GoogleMaps from "$lib/components/ui/google-maps";

	let compactFrame = $state(false);
</script>

<section class="grid min-h-dvh content-start gap-5 p-4 sm:p-6">
	<header class="flex flex-wrap items-center justify-between gap-3">
		<div>
			<h2 class="text-foreground m-0 text-xl font-black">One responsive instance</h2>
			<p class="text-muted mt-1 mb-0 text-sm">Changing the frame exercises the internal ResizeObserver.</p>
		</div>
		<div class="flex gap-2">
			<Button size="sm" variant={compactFrame ? "outline" : "accent"} onclick={() => (compactFrame = false)}>Desktop</Button>
			<Button size="sm" variant={compactFrame ? "accent" : "outline"} onclick={() => (compactFrame = true)}>Mobile</Button>
		</div>
	</header>

	<div
		class={[
			"border-border bg-background mx-auto w-full rounded-[1.75rem] border-2 p-2 transition-[max-width] duration-200 motion-reduce:transition-none",
			compactFrame ? "max-w-[390px]" : "max-w-6xl"
		]}
	>
		<GoogleMaps
			initialCamera={{ center: { latitude: 35.6762, longitude: 139.6503 }, zoom: 11 }}
			mapOptions={{ gestureHandling: "cooperative" }}
			class={compactFrame ? "h-[42rem]" : "h-[36rem]"}
		/>
	</div>
</section>
