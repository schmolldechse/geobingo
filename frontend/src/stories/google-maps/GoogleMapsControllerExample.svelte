<script lang="ts">
	import Crosshair from "@lucide/svelte/icons/crosshair";
	import Expand from "@lucide/svelte/icons/expand";
	import LocateFixed from "@lucide/svelte/icons/locate-fixed";
	import MapIcon from "@lucide/svelte/icons/map";
	import ScanEye from "@lucide/svelte/icons/scan-eye";
	import Button from "$lib/components/ui/Button.svelte";
	import GoogleMaps, { type GoogleMapsController } from "$lib/components/ui/google-maps";

	let controller = $state<GoogleMapsController | null>(null as GoogleMapsController | null);
	let commandMessage = $state("Choose a controller action.");
	let capturedSnapshot = $state("No snapshot captured yet.");

	async function runCommand(label: string, command: (() => Promise<unknown>) | undefined): Promise<void> {
		if (!command) {
			commandMessage = "The map is not ready yet.";
			return;
		}

		try {
			await command();
			commandMessage = label;
		} catch (error) {
			commandMessage = error instanceof Error ? error.message : "The Google Maps command failed.";
		}
	}

	function captureCurrentSnapshot(): void {
		capturedSnapshot = JSON.stringify(controller?.getCurrentViewSnapshot() ?? null, null, 2);
		commandMessage = "Captured the current view snapshot.";
	}
</script>

<section class="grid min-h-dvh gap-4 p-4 lg:grid-cols-[minmax(0,1fr)_22rem] lg:p-6">
	<GoogleMaps
		bind:controller
		initialCamera={{ center: { latitude: 52.52, longitude: 13.405 }, zoom: 11 }}
		mapOptions={{ gestureHandling: "greedy" }}
		class="min-h-[32rem] lg:h-[calc(100dvh-3rem)]"
	/>

	<aside class="border-border bg-surface grid content-start gap-4 rounded-2xl border-2 p-4 shadow-[var(--shadow-paper)]">
		<div>
			<h2 class="text-foreground m-0 text-lg font-black">Controller</h2>
			<p class="text-muted mt-1 mb-0 text-xs">{commandMessage}</p>
		</div>

		<dl class="grid grid-cols-[auto_1fr] gap-x-3 gap-y-1 text-xs">
			<dt class="text-muted font-bold">Load</dt>
			<dd class="m-0 font-mono">{controller?.loadState ?? "unbound"}</dd>
			<dt class="text-muted font-bold">View</dt>
			<dd class="m-0 font-mono">{controller?.view ?? "—"}</dd>
			<dt class="text-muted font-bold">Street View</dt>
			<dd class="m-0 font-mono">{controller?.streetViewState ?? "—"}</dd>
		</dl>

		<div class="grid grid-cols-2 gap-2">
			<Button
				variant="outline"
				size="sm"
				disabled={!controller?.ready}
				onclick={() =>
					void runCommand("Centered the map on Berlin.", () =>
						controller!.moveCamera({ center: { latitude: 52.52, longitude: 13.405 }, zoom: 13 })
					)}
			>
				<LocateFixed size={16} aria-hidden="true" />Berlin
			</Button>
			<Button
				variant="outline"
				size="sm"
				disabled={!controller?.ready}
				onclick={() =>
					void runCommand("Fitted the map to Germany.", () =>
						controller!.fitBounds({ north: 55.1, east: 15.1, south: 47.2, west: 5.8 }, 28)
					)}
			>
				<Expand size={16} aria-hidden="true" />Bounds
			</Button>
			<Button
				variant="secondary"
				size="sm"
				disabled={!controller?.ready}
				onclick={() =>
					void runCommand("Opened Street View near Brandenburg Gate.", () =>
						controller!.showStreetView({
							latitude: 52.5163,
							longitude: 13.3777,
							heading: 90,
							pitch: 0,
							zoom: 1
						})
					)}
			>
				<ScanEye size={16} aria-hidden="true" />Street View
			</Button>
			<Button
				variant="outline"
				size="sm"
				disabled={!controller?.ready}
				onclick={() => void runCommand("Returned to the map.", () => controller!.showMap())}
			>
				<MapIcon size={16} aria-hidden="true" />Map
			</Button>
			<Button
				variant="outline"
				size="sm"
				disabled={!controller?.ready}
				onclick={() => void runCommand("Focused the active surface.", () => controller!.focus())}
			>
				<Crosshair size={16} aria-hidden="true" />Focus
			</Button>
			<Button
				variant="outline"
				size="sm"
				disabled={!controller?.ready}
				onclick={() => void runCommand("Refreshed the map size.", () => controller!.refreshSize())}
			>
				<Expand size={16} aria-hidden="true" />Resize
			</Button>
		</div>

		<Button variant="accent" size="sm" disabled={!controller?.ready} onclick={captureCurrentSnapshot}>
			Capture current snapshot
		</Button>
		<pre
			class="bg-surface-muted text-foreground m-0 max-h-64 overflow-auto rounded-xl p-3 text-[0.65rem] leading-5">{capturedSnapshot}</pre>
	</aside>
</section>
