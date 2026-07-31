<script lang="ts">
	import MapPinned from "@lucide/svelte/icons/map-pinned";
	import RotateCcw from "@lucide/svelte/icons/rotate-ccw";
	import TriangleAlert from "@lucide/svelte/icons/triangle-alert";
	import { onMount, type Snippet } from "svelte";
	import type { ClassValue, HTMLAttributes } from "svelte/elements";
	import Button from "../Button.svelte";
	import LoadingDots from "../LoadingDots.svelte";
	import { createGoogleMapsController } from "./google-maps-controller.svelte";
	import { loadGoogleMaps } from "./google-maps-loader";
	import { GoogleMapsRuntime } from "./google-maps-runtime";
	import {
		GoogleMapsError,
		type GoogleMapsCameraSnapshot,
		type GoogleMapsController,
		type GoogleMapsInitialCamera,
		type GoogleMapsLoadApi,
		type GoogleMapsLoaderOptions,
		type GoogleMapsMapConfiguration,
		type GoogleMapsMapOptions,
		type GoogleMapsStateSnapshot,
		type GoogleMapsStreetViewLookupOptions,
		type GoogleMapsStreetViewOptions,
		type GoogleMapsStreetViewPosition,
		type GoogleMapsStreetViewState,
		type GoogleMapsStreetViewTarget,
		type GoogleMapsView
	} from "./google-maps-types";

	type Props = Omit<HTMLAttributes<HTMLDivElement>, "aria-label" | "children" | "class"> & {
		controller?: GoogleMapsController | null;
		loaderOptions?: GoogleMapsLoaderOptions;
		initialView?: GoogleMapsView;
		initialCamera?: GoogleMapsInitialCamera;
		initialStreetView?: GoogleMapsStreetViewTarget;
		initialStreetViewOptions?: GoogleMapsStreetViewLookupOptions;
		mapConfiguration?: GoogleMapsMapConfiguration;
		mapOptions?: GoogleMapsMapOptions;
		streetViewOptions?: GoogleMapsStreetViewOptions;
		loadingLabel?: string;
		errorLabel?: string;
		retryLabel?: string;
		showUnavailableMapAction?: boolean;
		overlay?: Snippet<[GoogleMapsController]>;
		loadApi?: GoogleMapsLoadApi;
		class?: ClassValue;
		"aria-label"?: string;
		onready?: (controller: GoogleMapsController) => void;
		onstatechange?: (state: GoogleMapsStateSnapshot) => void;
		onviewchange?: (view: GoogleMapsView) => void;
		oncamerachange?: (camera: GoogleMapsCameraSnapshot) => void;
		onstreetviewchange?: (state: GoogleMapsStreetViewState, position: GoogleMapsStreetViewPosition | null) => void;
		onerror?: (error: GoogleMapsError) => void;
	};

	let {
		controller = $bindable(null),
		loaderOptions = {},
		initialView = "map",
		initialCamera = { center: { latitude: 20, longitude: 0 }, zoom: 2 },
		initialStreetView,
		initialStreetViewOptions = {},
		mapConfiguration = {},
		mapOptions = {},
		streetViewOptions = {},
		loadingLabel = "Loading Google Maps…",
		errorLabel,
		retryLabel = "Try again",
		showUnavailableMapAction = true,
		overlay,
		loadApi,
		class: className,
		"aria-label": ariaLabel = "Interactive Google map",
		onready,
		onstatechange,
		onviewchange,
		oncamerachange,
		onstreetviewchange,
		onerror,
		...restProps
	}: Props = $props();

	type ReadyWaiter = {
		resolve: () => void;
		reject: (error: GoogleMapsError) => void;
	};

	let container: HTMLDivElement | null = $state(null);
	let runtime = $state.raw<GoogleMapsRuntime | null>(null);
	let resizeObserver: ResizeObserver | null = null;
	let destroyed = false;
	let initializing = false;
	let initializationSerial = 0;
	const readyWaiters = new Set<ReadyWaiter>();

	const controllerHandle = createGoogleMapsController(
		{
			whenReady: waitUntilReady,
			moveCamera: async (update) => (await requireRuntime()).moveCamera(update),
			panTo: async (coordinates) => (await requireRuntime()).panTo(coordinates),
			fitBounds: async (bounds, padding) => (await requireRuntime()).fitBounds(bounds, padding),
			showMap: async () => (await requireRuntime()).showMap(),
			showStreetView: async (target, options) => (await requireRuntime()).showStreetView(target, options),
			focus: async () => (await requireRuntime()).focus(),
			refreshSize: async () => (await requireRuntime()).refreshSize(),
			retry: retryInitialization
		},
		(state) => onstatechange?.(state)
	);
	const ownedController = controllerHandle.controller;

	const announcement = $derived.by(() => {
		if (ownedController.loadState === "loading") return loadingLabel;
		if (ownedController.loadState === "error") return errorLabel ?? ownedController.error?.message ?? "Google Maps failed.";
		if (ownedController.streetViewState === "unavailable") return "Street View imagery is unavailable here.";
		if (ownedController.view === "street-view") return "Street View is active.";
		if (ownedController.loadState === "ready") return "The map is ready.";
		return "";
	});

	function commit(patch: Parameters<typeof controllerHandle.update>[0]): GoogleMapsStateSnapshot {
		return controllerHandle.update(patch);
	}

	function waitUntilReady(): Promise<void> {
		if (destroyed) return Promise.reject(destroyedError());
		if (runtime && ownedController.loadState === "ready") return Promise.resolve();
		if (ownedController.error) return Promise.reject(ownedController.error);

		return new Promise<void>((resolve, reject) => {
			readyWaiters.add({ resolve, reject });
		});
	}

	async function requireRuntime(): Promise<GoogleMapsRuntime> {
		await waitUntilReady();
		if (runtime) return runtime;
		throw destroyed ? destroyedError() : new GoogleMapsError("initialization-failed", "Google Maps is not ready.");
	}

	function resolveReadyWaiters(): void {
		for (const waiter of readyWaiters) waiter.resolve();
		readyWaiters.clear();
	}

	function rejectReadyWaiters(error: GoogleMapsError): void {
		for (const waiter of readyWaiters) waiter.reject(error);
		readyWaiters.clear();
	}

	function handleViewChange(view: GoogleMapsView): void {
		if (ownedController.view === view) return;
		commit({ view });
		onviewchange?.(view);
	}

	function handleCameraChange(camera: GoogleMapsCameraSnapshot): void {
		commit({ camera });
		oncamerachange?.(camera);
	}

	function handleStreetViewChange(streetViewState: GoogleMapsStreetViewState, position?: GoogleMapsStreetViewPosition): void {
		const patch = position ? { streetViewState, streetViewPosition: position } : { streetViewState };
		commit(patch);
		onstreetviewchange?.(streetViewState, position ?? ownedController.getStreetViewPosition());
	}

	async function initialize(): Promise<void> {
		if (destroyed || initializing || !container) return;
		initializing = true;
		const serial = ++initializationSerial;
		runtime?.destroy();
		runtime = null;
		commit({ loadState: "loading", error: null, view: "map", streetViewState: "hidden" });

		try {
			const libraries = await loadGoogleMaps(loaderOptions, loadApi);
			if (destroyed || serial !== initializationSerial || !container) return;

			const nextRuntime = new GoogleMapsRuntime(
				container,
				libraries,
				initialCamera,
				mapConfiguration,
				mapOptions,
				streetViewOptions,
				{
					onCameraChange: handleCameraChange,
					onViewChange: handleViewChange,
					onStreetViewChange: handleStreetViewChange
				}
			);
			runtime = nextRuntime;

			if (initialView === "street-view") {
				try {
					await nextRuntime.showStreetView(initialStreetView ?? initialCamera.center, initialStreetViewOptions);
				} catch (error) {
					if (!(error instanceof GoogleMapsError) || error.code !== "street-view-unavailable") throw error;
				}
			}

			if (destroyed || serial !== initializationSerial) {
				nextRuntime.destroy();
				return;
			}

			commit({ loadState: "ready", error: null });
			resolveReadyWaiters();
			onready?.(ownedController);
		} catch (error) {
			if (destroyed || serial !== initializationSerial) return;
			const mapsError = toGoogleMapsError(error);
			commit({ loadState: "error", error: mapsError });
			rejectReadyWaiters(mapsError);
			onerror?.(mapsError);
		} finally {
			if (serial === initializationSerial) initializing = false;
		}
	}

	async function retryInitialization(): Promise<void> {
		if (destroyed) throw destroyedError();
		if (ownedController.loadState === "loading") return waitUntilReady();
		await initialize();
		if (ownedController.error) throw ownedController.error;
	}

	function destroyedError(): GoogleMapsError {
		return new GoogleMapsError("destroyed", "This Google Maps controller is no longer connected.", {
			recoverable: false
		});
	}

	function toGoogleMapsError(error: unknown): GoogleMapsError {
		return error instanceof GoogleMapsError
			? error
			: new GoogleMapsError("initialization-failed", "Google Maps could not be initialized.", { cause: error });
	}

	function runUiCommand(command: Promise<unknown>): void {
		void command.catch(() => undefined);
	}

	$effect(() => {
		if (!runtime) return;
		runtime.setOptions(mapOptions, streetViewOptions);
	});

	onMount(() => {
		controller = ownedController;
		resizeObserver = new ResizeObserver(() => runtime?.refreshSize());
		if (container) resizeObserver.observe(container);
		void initialize();

		return () => {
			destroyed = true;
			initializationSerial += 1;
			resizeObserver?.disconnect();
			resizeObserver = null;
			runtime?.destroy();
			runtime = null;
			const error = destroyedError();
			rejectReadyWaiters(error);
			if (controller === ownedController) controller = null;
		};
	});
</script>

<div
	{...restProps}
	class={[
		"border-border bg-surface text-foreground relative isolate min-h-50 w-full min-w-0 overflow-hidden rounded-2xl border-2",
		className
	]}
	data-google-maps
	data-load-state={ownedController.loadState}
	data-view={ownedController.view}
	data-street-view-state={ownedController.streetViewState}
	data-error-code={ownedController.error?.code}
>
	<div bind:this={container} class="absolute inset-0" tabindex="-1" aria-label={ariaLabel}></div>

	{#if ownedController.loadState === "idle" || ownedController.loadState === "loading"}
		<div class="bg-surface absolute inset-0 z-20 grid place-items-center p-6 text-center" role="status">
			<div class="grid justify-items-center gap-3">
				<LoadingDots size="lg" />
				<p class="m-0 text-sm font-extrabold">{loadingLabel}</p>
			</div>
		</div>
	{:else if ownedController.loadState === "error"}
		<div class="bg-surface absolute inset-0 z-20 grid place-items-center p-6 text-center" role="alert">
			<div class="grid max-w-md justify-items-center gap-3">
				<TriangleAlert class="text-primary" size={30} aria-hidden="true" />
				<p class="m-0 text-sm font-extrabold">{errorLabel ?? ownedController.error?.message ?? "Google Maps failed."}</p>
				{#if ownedController.error?.recoverable}
					<Button variant="outline" size="sm" onclick={() => runUiCommand(ownedController.retry())}>
						<RotateCcw size={16} aria-hidden="true" />
						{retryLabel}
					</Button>
				{/if}
			</div>
		</div>
	{:else if ownedController.streetViewState === "unavailable"}
		<div
			class="border-foreground bg-surface absolute top-3 left-1/2 z-20 flex max-w-[calc(100%-1.5rem)] -translate-x-1/2 items-center gap-2 rounded-xl border-2 px-3 py-2 shadow-[var(--shadow-paper)]"
			role="status"
		>
			<MapPinned class="text-primary shrink-0" size={17} aria-hidden="true" />
			<span class="text-xs font-extrabold">Street View is unavailable here.</span>
			{#if showUnavailableMapAction}
				<Button variant="ghost" size="sm" onclick={() => runUiCommand(ownedController.showMap())}>Back to map</Button>
			{/if}
		</div>
	{/if}

	{#if overlay}
		<div class="pointer-events-none absolute inset-x-3 top-3 bottom-12 z-10" data-google-maps-overlay>
			{@render overlay(ownedController)}
		</div>
	{/if}

	<p class="sr-only" aria-live="polite">{announcement}</p>
</div>
