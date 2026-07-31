<script module lang="ts">
	import MapIcon from "@lucide/svelte/icons/map";
	import ScanEye from "@lucide/svelte/icons/scan-eye";
	import { defineMeta } from "@storybook/addon-svelte-csf";

	import Button from "$lib/components/ui/Button.svelte";
	import GoogleMaps, {
		GoogleMapsError,
		type GoogleMapsGestureHandling,
		type GoogleMapsMapType,
		type GoogleMapsView
	} from "$lib/components/ui/google-maps";
	import GoogleMapsControllerExample from "./GoogleMapsControllerExample.svelte";
	import GoogleMapsResponsiveExample from "./GoogleMapsResponsiveExample.svelte";

	type PlaygroundArgs = {
		latitude: number;
		longitude: number;
		zoom: number;
		initialView: GoogleMapsView;
		mapType: GoogleMapsMapType;
		gestureHandling: GoogleMapsGestureHandling;
		disableDefaultUi: boolean;
		fullscreenControl: boolean;
		mapTypeControl: boolean;
		streetViewControl: boolean;
		zoomControl: boolean;
		showRoadLabels: boolean;
		clickToGo: boolean;
	};

	const mapTypes: GoogleMapsMapType[] = ["roadmap", "satellite", "hybrid", "terrain"];
	const gestureModes: GoogleMapsGestureHandling[] = ["auto", "cooperative", "greedy", "none"];

	const pendingLoader = () => new Promise<void>(() => undefined);
	const failedLoader = async () => {
		throw new Error("Storybook simulated a Google Maps network failure.");
	};
	const missingConfigurationLoader = async () => {
		throw new GoogleMapsError(
			"missing-api-key",
			"Google Maps is not configured. Set PUBLIC_GOOGLE_MAPS_API_KEY for this environment."
		);
	};

	const { Story } = defineMeta({
		title: "UI/GoogleMaps",
		component: GoogleMaps,
		render: playground,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen",
			docs: {
				description: {
					component:
						"A responsive Google Maps and Street View surface. The component reads PUBLIC_GOOGLE_MAPS_API_KEY internally and exposes a Google-independent controller."
				}
			}
		},
		args: {
			latitude: 52.52,
			longitude: 13.405,
			zoom: 12,
			initialView: "map",
			mapType: "roadmap",
			gestureHandling: "cooperative",
			disableDefaultUi: false,
			fullscreenControl: true,
			mapTypeControl: true,
			streetViewControl: true,
			zoomControl: true,
			showRoadLabels: true,
			clickToGo: true
		},
		argTypes: {
			latitude: { control: { type: "number", min: -90, max: 90, step: 0.001 } },
			longitude: { control: { type: "number", min: -180, max: 180, step: 0.001 } },
			zoom: { control: { type: "number", min: 0, max: 22, step: 1 } },
			initialView: { control: "inline-radio", options: ["map", "street-view"] },
			mapType: { control: "select", options: mapTypes },
			gestureHandling: { control: "select", options: gestureModes },
			disableDefaultUi: { control: "boolean" },
			fullscreenControl: { control: "boolean" },
			mapTypeControl: { control: "boolean" },
			streetViewControl: { control: "boolean" },
			zoomControl: { control: "boolean" },
			showRoadLabels: { control: "boolean" },
			clickToGo: { control: "boolean" }
		}
	});
</script>

{#snippet playground(args: PlaygroundArgs)}
	<section class="grid min-h-dvh gap-4 p-4 sm:p-6">
		<header class="grid content-start gap-1">
			<h1 class="text-foreground m-0 text-2xl font-black">Google Maps playground</h1>
			<p class="text-muted m-0 max-w-3xl text-sm">
				Use Pegman to choose where Street View begins. A configured PUBLIC_GOOGLE_MAPS_API_KEY enables the live map.
			</p>
		</header>
		<GoogleMaps
			initialView={args.initialView}
			initialCamera={{
				center: { latitude: args.latitude, longitude: args.longitude },
				zoom: args.zoom
			}}
			initialStreetView={{ latitude: args.latitude, longitude: args.longitude, heading: 0, pitch: 0, zoom: 1 }}
			mapOptions={{
				mapType: args.mapType,
				gestureHandling: args.gestureHandling,
				disableDefaultUi: args.disableDefaultUi,
				controls: {
					fullscreen: args.fullscreenControl,
					mapType: args.mapTypeControl,
					streetView: args.streetViewControl,
					zoom: args.zoomControl
				}
			}}
			streetViewOptions={{
				showRoadLabels: args.showRoadLabels,
				clickToGo: args.clickToGo,
				controls: { address: true, close: true, fullscreen: true, links: true, pan: true, zoom: true }
			}}
			class="h-full min-h-[32rem]"
		/>
	</section>
{/snippet}

<Story name="Playground" />

<Story
	name="External controller"
	parameters={{
		docs: { description: { story: "Every external command uses the bound, Google-independent controller." } }
	}}
	asChild
>
	<GoogleMapsControllerExample />
</Story>

<Story name="Map and Street View options" asChild>
	<section class="grid gap-5 p-4 lg:grid-cols-2 lg:p-6">
		<div class="grid content-start gap-3">
			<div>
				<h2 class="text-foreground m-0 text-lg font-black">Full controls</h2>
				<p class="text-muted mt-1 mb-0 text-sm">Greedy gestures and every relevant map control.</p>
			</div>
			<GoogleMaps
				initialCamera={{ center: { latitude: 52.52, longitude: 13.405 }, zoom: 11 }}
				mapOptions={{
					gestureHandling: "greedy",
					controls: {
						camera: true,
						fullscreen: true,
						mapType: true,
						rotate: true,
						scale: true,
						streetView: true,
						zoom: true,
						positions: { streetView: "left-center", zoom: "left-center" }
					}
				}}
				streetViewOptions={{
					clickToGo: true,
					motionTracking: true,
					showRoadLabels: true,
					controls: {
						address: true,
						close: true,
						fullscreen: true,
						imageDate: true,
						links: true,
						motionTracking: true,
						pan: true,
						zoom: true
					}
				}}
				class="h-[32rem]"
			/>
		</div>

		<div class="grid content-start gap-3">
			<div>
				<h2 class="text-foreground m-0 text-lg font-black">Minimal interaction</h2>
				<p class="text-muted mt-1 mb-0 text-sm">No default UI, keyboard shortcuts, POI clicks, or gestures.</p>
			</div>
			<GoogleMaps
				initialCamera={{ center: { latitude: 40.7128, longitude: -74.006 }, zoom: 11 }}
				mapOptions={{
					mapType: "terrain",
					gestureHandling: "none",
					keyboardShortcuts: false,
					clickableIcons: false,
					disableDefaultUi: true
				}}
				streetViewOptions={{ disableDefaultUi: true, clickToGo: false, scrollwheel: false, showRoadLabels: false }}
				class="h-[32rem]"
			/>
		</div>
	</section>
</Story>

<Story
	name="Fixed panorama policy"
	parameters={{
		docs: {
			description: {
				story:
					"A fixed panorama lookup never substitutes nearby coordinate imagery. If the exact panorama is unavailable, the consumer keeps the map action hidden and presents its own unavailable state."
			}
		}
	}}
	asChild
>
	<section class="grid min-h-dvh place-items-center p-4 sm:p-6">
		<GoogleMaps
			initialView="street-view"
			initialCamera={{ center: { latitude: 52.52, longitude: 13.405 }, zoom: 18 }}
			initialStreetView={{
				panoramaId: "storybook-fixed-panorama",
				latitude: 52.52,
				longitude: 13.405,
				heading: 0,
				pitch: 0,
				zoom: 1
			}}
			initialStreetViewOptions={{ fallbackToCoordinates: false }}
			showUnavailableMapAction={false}
			class="h-[32rem] w-full max-w-5xl"
		/>
	</section>
</Story>

<Story name="Custom overlay" asChild>
	<section class="grid min-h-dvh p-4 sm:p-6">
		<GoogleMaps initialCamera={{ center: { latitude: 48.8566, longitude: 2.3522 }, zoom: 12 }} class="h-full min-h-[34rem]">
			{#snippet overlay(controller)}
				<div class="flex items-start justify-between gap-3">
					<div
						class="border-foreground bg-surface pointer-events-auto rounded-xl border-2 px-3 py-2 shadow-[var(--shadow-paper)]"
					>
						<p class="text-muted m-0 text-[0.65rem] font-black tracking-widest uppercase">Current view</p>
						<p class="m-0 mt-0.5 text-sm font-black">{controller.view === "map" ? "Map" : "Street View"}</p>
					</div>
					<div class="pointer-events-auto flex gap-2">
						<Button size="sm" variant="outline" disabled={!controller.ready} onclick={() => void controller.showMap()}>
							<MapIcon size={16} aria-hidden="true" />Map
						</Button>
						<Button
							size="sm"
							variant="secondary"
							disabled={!controller.ready}
							onclick={() =>
								void controller
									.showStreetView({ latitude: 48.8584, longitude: 2.2945, heading: 35, pitch: 0, zoom: 1 })
									.catch(() => undefined)}
						>
							<ScanEye size={16} aria-hidden="true" />Street View
						</Button>
					</div>
				</div>
			{/snippet}
		</GoogleMaps>
	</section>
</Story>

<Story name="Responsive surface" asChild>
	<GoogleMapsResponsiveExample />
</Story>

<Story
	name="Loading"
	parameters={{ docs: { description: { story: "A deterministic pending loader keeps the GeoBingo LoadingDots visible." } } }}
	asChild
>
	<section class="grid min-h-dvh place-items-center p-4 sm:p-6">
		<GoogleMaps loadApi={pendingLoader} class="h-[28rem] w-full max-w-4xl" />
	</section>
</Story>

<Story name="Missing configuration" asChild>
	<section class="grid min-h-dvh place-items-center p-4 sm:p-6">
		<GoogleMaps loadApi={missingConfigurationLoader} class="h-[28rem] w-full max-w-4xl" />
	</section>
</Story>

<Story name="Load failure" asChild>
	<section class="grid min-h-dvh place-items-center p-4 sm:p-6">
		<GoogleMaps loadApi={failedLoader} class="h-[28rem] w-full max-w-4xl" />
	</section>
</Story>
