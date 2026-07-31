import type { GoogleMapsLibraries } from "./google-maps-loader";
import {
	GoogleMapsError,
	type GoogleMapsBounds,
	type GoogleMapsCameraSnapshot,
	type GoogleMapsCameraUpdate,
	type GoogleMapsControlPosition,
	type GoogleMapsCoordinates,
	type GoogleMapsInitialCamera,
	type GoogleMapsMapConfiguration,
	type GoogleMapsMapOptions,
	type GoogleMapsMapType,
	type GoogleMapsPadding,
	type GoogleMapsStreetViewLookupOptions,
	type GoogleMapsStreetViewOptions,
	type GoogleMapsStreetViewPosition,
	type GoogleMapsStreetViewState,
	type GoogleMapsStreetViewTarget,
	type GoogleMapsView
} from "./google-maps-types";

export interface GoogleMapsRuntimeSink {
	onCameraChange: (camera: GoogleMapsCameraSnapshot) => void;
	onViewChange: (view: GoogleMapsView) => void;
	onStreetViewChange: (state: GoogleMapsStreetViewState, position?: GoogleMapsStreetViewPosition) => void;
}

const MAP_TYPES: GoogleMapsMapType[] = ["roadmap", "satellite", "hybrid", "terrain"];
const LOOKUP_TIMEOUT_MS = 12_000;

export class GoogleMapsRuntime {
	readonly #container: HTMLDivElement;
	readonly #event: typeof google.maps.event;
	readonly #streetViewStatus: typeof google.maps.StreetViewStatus;
	readonly #sink: GoogleMapsRuntimeSink;
	readonly #map: google.maps.Map;
	readonly #panorama: google.maps.StreetViewPanorama;
	readonly #listeners: google.maps.MapsEventListener[] = [];
	readonly #pendingLookupCancels = new Set<() => void>();

	#requestSerial = 0;
	#destroyed = false;

	constructor(
		container: HTMLDivElement,
		libraries: GoogleMapsLibraries,
		initialCamera: GoogleMapsInitialCamera,
		mapConfiguration: GoogleMapsMapConfiguration,
		mapOptions: GoogleMapsMapOptions,
		streetViewOptions: GoogleMapsStreetViewOptions,
		sink: GoogleMapsRuntimeSink
	) {
		assertCoordinates(initialCamera.center);
		assertFinite("zoom", initialCamera.zoom);

		this.#container = container;
		this.#event = libraries.core.event;
		this.#streetViewStatus = libraries.streetView.StreetViewStatus;
		this.#sink = sink;
		this.#map = new libraries.maps.Map(container, toGoogleMapOptions(initialCamera, mapConfiguration, mapOptions));
		this.#panorama = this.#map.getStreetView();
		this.#panorama.setOptions(toGoogleStreetViewOptions(streetViewOptions));

		this.#listen(this.#map, "idle", () => this.#syncCamera());
		for (const eventName of [
			"pano_changed",
			"position_changed",
			"pov_changed",
			"status_changed",
			"visible_changed",
			"zoom_changed"
		]) {
			this.#listen(this.#panorama, eventName, () => this.#syncStreetView());
		}

		this.#syncCamera();
		this.#syncStreetView();
	}

	setOptions(mapOptions: GoogleMapsMapOptions, streetViewOptions: GoogleMapsStreetViewOptions): void {
		this.#assertActive();
		this.#map.setOptions(toMutableGoogleMapOptions(mapOptions));
		this.#panorama.setOptions(toGoogleStreetViewOptions(streetViewOptions));
	}

	moveCamera(update: GoogleMapsCameraUpdate): void {
		this.#assertActive();
		if (Object.keys(update).length === 0) {
			throw new GoogleMapsError("invalid-command", "A camera update must contain at least one value.");
		}

		if (update.center) assertCoordinates(update.center);
		if (update.zoom !== undefined) assertFinite("zoom", update.zoom);
		if (update.heading !== undefined) assertFinite("heading", update.heading);
		if (update.tilt !== undefined) assertFinite("tilt", update.tilt);

		this.#map.moveCamera({
			center: update.center ? toGoogleCoordinates(update.center) : undefined,
			zoom: update.zoom,
			heading: update.heading === undefined ? undefined : normalizeHeading(update.heading),
			tilt: update.tilt
		});
	}

	panTo(coordinates: GoogleMapsCoordinates): void {
		this.#assertActive();
		assertCoordinates(coordinates);
		this.#map.panTo(toGoogleCoordinates(coordinates));
	}

	fitBounds(bounds: GoogleMapsBounds, padding?: number | GoogleMapsPadding): void {
		this.#assertActive();
		assertBounds(bounds);
		if (typeof padding === "number") assertNonNegative("padding", padding);
		if (padding && typeof padding !== "number") {
			for (const [side, value] of Object.entries(padding)) assertNonNegative(`${side} padding`, value);
		}
		this.#map.fitBounds(toGoogleBounds(bounds), padding);
	}

	showMap(): void {
		this.#assertActive();
		this.#requestSerial += 1;
		this.#cancelPendingLookups();
		this.#panorama.setVisible(false);
		this.#sink.onViewChange("map");
		this.#sink.onStreetViewChange("hidden");
		this.#syncCamera();
	}

	async showStreetView(
		target: GoogleMapsStreetViewTarget,
		options: GoogleMapsStreetViewLookupOptions = {}
	): Promise<GoogleMapsStreetViewPosition> {
		this.#assertActive();
		validateStreetViewTarget(target);

		const serial = ++this.#requestSerial;
		this.#cancelPendingLookups();
		this.#sink.onStreetViewChange("loading");

		let resolved = false;
		if ("panoramaId" in target && target.panoramaId) {
			resolved = await this.#lookup(() => this.#panorama.setPano(target.panoramaId), target.panoramaId);
		}

		this.#assertCurrentRequest(serial);
		const hasCoordinates = target.latitude !== undefined && target.longitude !== undefined;
		const shouldTryCoordinates =
			hasCoordinates && (!("panoramaId" in target) || !target.panoramaId || options.fallbackToCoordinates !== false);
		if (!resolved && shouldTryCoordinates) {
			resolved = await this.#lookup(() => this.#panorama.setPosition({ lat: target.latitude!, lng: target.longitude! }));
		}

		this.#assertCurrentRequest(serial);
		if (!resolved) {
			this.#panorama.setVisible(false);
			this.#sink.onViewChange("map");
			this.#sink.onStreetViewChange("unavailable");
			throw new GoogleMapsError("street-view-unavailable", "Street View imagery is unavailable at this location.");
		}

		const currentPov = this.#panorama.getPov();
		if (target.heading !== undefined || target.pitch !== undefined) {
			this.#panorama.setPov({
				heading: target.heading === undefined ? currentPov.heading : normalizeHeading(target.heading),
				pitch: target.pitch ?? currentPov.pitch
			});
		}
		if (target.zoom !== undefined) this.#panorama.setZoom(target.zoom);

		this.#panorama.setVisible(true);
		this.#sink.onViewChange("street-view");
		const position = this.#readStreetViewPosition();
		if (!position) {
			this.#sink.onStreetViewChange("loading");
			throw new GoogleMapsError("street-view-unavailable", "Street View did not provide a complete panorama position.");
		}

		this.#sink.onStreetViewChange("ready", position);
		return { ...position };
	}

	focus(): void {
		this.#assertActive();
		if (this.#panorama.getVisible()) {
			this.#panorama.focus();
			return;
		}
		this.#container.focus({ preventScroll: true });
	}

	refreshSize(): void {
		this.#assertActive();
		this.#event.trigger(this.#map, "resize");
		this.#event.trigger(this.#panorama, "resize");
		this.#syncCamera();
		this.#syncStreetView();
	}

	destroy(): void {
		if (this.#destroyed) return;
		this.#destroyed = true;
		this.#requestSerial += 1;
		this.#cancelPendingLookups();
		for (const listener of this.#listeners.splice(0)) listener.remove();
	}

	#listen(target: google.maps.MVCObject, eventName: string, listener: () => void): void {
		this.#listeners.push(target.addListener(eventName, listener));
	}

	#syncCamera(): void {
		if (this.#destroyed) return;
		const center = this.#map.getCenter();
		const zoom = this.#map.getZoom();
		if (!center || !Number.isFinite(zoom)) return;

		const bounds = this.#map.getBounds();
		const northEast = bounds?.getNorthEast();
		const southWest = bounds?.getSouthWest();
		const mapTypeId = this.#map.getMapTypeId();

		this.#sink.onCameraChange({
			center: { latitude: center.lat(), longitude: center.lng() },
			zoom: zoom!,
			heading: this.#map.getHeading(),
			tilt: this.#map.getTilt(),
			bounds:
				northEast && southWest
					? {
							north: northEast.lat(),
							east: northEast.lng(),
							south: southWest.lat(),
							west: southWest.lng()
						}
					: undefined,
			mapType: MAP_TYPES.includes(mapTypeId as GoogleMapsMapType) ? (mapTypeId as GoogleMapsMapType) : "roadmap"
		});
	}

	#syncStreetView(): void {
		if (this.#destroyed) return;
		if (!this.#panorama.getVisible()) {
			this.#sink.onViewChange("map");
			this.#sink.onStreetViewChange("hidden");
			return;
		}

		this.#sink.onViewChange("street-view");
		const status = this.#panorama.getStatus();
		if (status && status !== this.#streetViewStatus.OK) {
			this.#sink.onStreetViewChange(status === this.#streetViewStatus.ZERO_RESULTS ? "unavailable" : "loading");
			return;
		}

		const position = this.#readStreetViewPosition();
		this.#sink.onStreetViewChange(position ? "ready" : "loading", position ?? undefined);
	}

	#readStreetViewPosition(): GoogleMapsStreetViewPosition | null {
		const panoramaId = this.#panorama.getPano().trim();
		const position = this.#panorama.getPosition();
		const pov = this.#panorama.getPov();
		const zoom = this.#panorama.getZoom();
		if (
			!panoramaId ||
			!position ||
			!Number.isFinite(position.lat()) ||
			!Number.isFinite(position.lng()) ||
			!Number.isFinite(pov.heading) ||
			!Number.isFinite(pov.pitch) ||
			!Number.isFinite(zoom)
		) {
			return null;
		}

		return {
			panoramaId,
			latitude: position.lat(),
			longitude: position.lng(),
			heading: normalizeHeading(pov.heading),
			pitch: pov.pitch,
			zoom
		};
	}

	#lookup(apply: () => void, panoramaId?: string): Promise<boolean> {
		if (panoramaId && this.#panorama.getPano() === panoramaId && this.#panorama.getStatus() === this.#streetViewStatus.OK) {
			return Promise.resolve(true);
		}

		return new Promise((resolve) => {
			let settled = false;
			let listener: google.maps.MapsEventListener | null = null;
			let timeout: number | null = null;

			const finish = (result: boolean) => {
				if (settled) return;
				settled = true;
				listener?.remove();
				if (timeout !== null) window.clearTimeout(timeout);
				this.#pendingLookupCancels.delete(cancel);
				resolve(result);
			};
			const cancel = () => finish(false);

			listener = this.#panorama.addListener("status_changed", () => {
				finish(this.#panorama.getStatus() === this.#streetViewStatus.OK);
			});
			timeout = window.setTimeout(() => finish(false), LOOKUP_TIMEOUT_MS);
			this.#pendingLookupCancels.add(cancel);
			apply();
		});
	}

	#cancelPendingLookups(): void {
		for (const cancel of [...this.#pendingLookupCancels]) cancel();
	}

	#assertCurrentRequest(serial: number): void {
		this.#assertActive();
		if (serial !== this.#requestSerial) {
			throw new GoogleMapsError("superseded", "A newer Google Maps command replaced this request.");
		}
	}

	#assertActive(): void {
		if (this.#destroyed) {
			throw new GoogleMapsError("destroyed", "This Google Maps controller is no longer connected.", {
				recoverable: false
			});
		}
	}
}

function toGoogleMapOptions(
	initialCamera: GoogleMapsInitialCamera,
	configuration: GoogleMapsMapConfiguration,
	options: GoogleMapsMapOptions
): google.maps.MapOptions {
	const colorSchemes = {
		light: "LIGHT",
		dark: "DARK",
		"follow-system": "FOLLOW_SYSTEM"
	} as const;
	const renderingTypes = { raster: "RASTER", vector: "VECTOR" } as const;

	return withoutUndefined({
		...toMutableGoogleMapOptions(options),
		center: toGoogleCoordinates(initialCamera.center),
		zoom: initialCamera.zoom,
		heading: initialCamera.heading,
		tilt: initialCamera.tilt,
		mapId: configuration.mapId,
		colorScheme: configuration.colorScheme ? colorSchemes[configuration.colorScheme] : undefined,
		renderingType: configuration.renderingType ? renderingTypes[configuration.renderingType] : undefined,
		controlSize: configuration.controlSize,
		backgroundColor: configuration.backgroundColor
	});
}

function toMutableGoogleMapOptions(options: GoogleMapsMapOptions): google.maps.MapOptions {
	return withoutUndefined({
		mapTypeId: options.mapType,
		minZoom: options.minZoom,
		maxZoom: options.maxZoom,
		restriction: options.restriction
			? withoutUndefined({
					latLngBounds: toGoogleBounds(options.restriction.bounds),
					strictBounds: options.restriction.strict
				})
			: undefined,
		gestureHandling: options.gestureHandling,
		keyboardShortcuts: options.keyboardShortcuts,
		clickableIcons: options.clickableIcons,
		disableDefaultUI: options.disableDefaultUi,
		cameraControl: options.controls?.camera,
		cameraControlOptions: toGoogleControlOptions(options.controls?.positions?.camera),
		fullscreenControl: options.controls?.fullscreen,
		fullscreenControlOptions: toGoogleControlOptions(options.controls?.positions?.fullscreen),
		mapTypeControl: options.controls?.mapType,
		mapTypeControlOptions: toGoogleControlOptions(options.controls?.positions?.mapType),
		rotateControl: options.controls?.rotate,
		rotateControlOptions: toGoogleControlOptions(options.controls?.positions?.rotate),
		scaleControl: options.controls?.scale,
		streetViewControl: options.controls?.streetView,
		streetViewControlOptions: toGoogleControlOptions(options.controls?.positions?.streetView),
		zoomControl: options.controls?.zoom,
		zoomControlOptions: toGoogleControlOptions(options.controls?.positions?.zoom)
	});
}

function toGoogleControlOptions(
	position: GoogleMapsControlPosition | undefined
): { position: google.maps.ControlPosition } | undefined {
	if (!position) return undefined;

	const positions: Record<GoogleMapsControlPosition, google.maps.ControlPosition> = {
		"top-left": google.maps.ControlPosition.TOP_LEFT,
		"top-center": google.maps.ControlPosition.TOP_CENTER,
		"top-right": google.maps.ControlPosition.TOP_RIGHT,
		"left-top": google.maps.ControlPosition.LEFT_TOP,
		"left-center": google.maps.ControlPosition.LEFT_CENTER,
		"left-bottom": google.maps.ControlPosition.LEFT_BOTTOM,
		"right-top": google.maps.ControlPosition.RIGHT_TOP,
		"right-center": google.maps.ControlPosition.RIGHT_CENTER,
		"right-bottom": google.maps.ControlPosition.RIGHT_BOTTOM,
		"bottom-left": google.maps.ControlPosition.BOTTOM_LEFT,
		"bottom-center": google.maps.ControlPosition.BOTTOM_CENTER,
		"bottom-right": google.maps.ControlPosition.BOTTOM_RIGHT
	};

	return { position: positions[position] };
}

function toGoogleStreetViewOptions(options: GoogleMapsStreetViewOptions): google.maps.StreetViewPanoramaOptions {
	return withoutUndefined({
		clickToGo: options.clickToGo,
		scrollwheel: options.scrollwheel,
		showRoadLabels: options.showRoadLabels,
		motionTracking: options.motionTracking,
		disableDefaultUI: options.disableDefaultUi,
		addressControl: options.controls?.address,
		enableCloseButton: options.controls?.close ?? true,
		fullscreenControl: options.controls?.fullscreen,
		imageDateControl: options.controls?.imageDate,
		linksControl: options.controls?.links,
		motionTrackingControl: options.controls?.motionTracking,
		panControl: options.controls?.pan,
		zoomControl: options.controls?.zoom
	});
}

function withoutUndefined<T extends object>(options: T): T {
	return Object.fromEntries(Object.entries(options).filter(([, value]) => value !== undefined)) as T;
}

function toGoogleCoordinates(coordinates: GoogleMapsCoordinates): google.maps.LatLngLiteral {
	return { lat: coordinates.latitude, lng: coordinates.longitude };
}

function toGoogleBounds(bounds: GoogleMapsBounds): google.maps.LatLngBoundsLiteral {
	return { north: bounds.north, east: bounds.east, south: bounds.south, west: bounds.west };
}

function validateStreetViewTarget(target: GoogleMapsStreetViewTarget): void {
	if ("panoramaId" in target && target.panoramaId !== undefined && !target.panoramaId.trim()) {
		throw new GoogleMapsError("invalid-command", "The panorama identifier cannot be empty.");
	}

	const hasLatitude = target.latitude !== undefined;
	const hasLongitude = target.longitude !== undefined;
	if (hasLatitude !== hasLongitude) {
		throw new GoogleMapsError("invalid-command", "Latitude and longitude must be provided together.");
	}
	if (hasLatitude && hasLongitude) assertCoordinates({ latitude: target.latitude!, longitude: target.longitude! });
	if (!("panoramaId" in target) && !hasLatitude) {
		throw new GoogleMapsError("invalid-command", "A panorama identifier or coordinates are required.");
	}
	if (target.heading !== undefined) assertFinite("heading", target.heading);
	if (target.pitch !== undefined) {
		assertFinite("pitch", target.pitch);
		if (target.pitch < -90 || target.pitch > 90) {
			throw new GoogleMapsError("invalid-command", "Pitch must be between -90 and 90 degrees.");
		}
	}
	if (target.zoom !== undefined) assertNonNegative("zoom", target.zoom);
}

function assertCoordinates(coordinates: GoogleMapsCoordinates): void {
	assertFinite("latitude", coordinates.latitude);
	assertFinite("longitude", coordinates.longitude);
	if (coordinates.latitude < -90 || coordinates.latitude > 90) {
		throw new GoogleMapsError("invalid-command", "Latitude must be between -90 and 90 degrees.");
	}
	if (coordinates.longitude < -180 || coordinates.longitude > 180) {
		throw new GoogleMapsError("invalid-command", "Longitude must be between -180 and 180 degrees.");
	}
}

function assertBounds(bounds: GoogleMapsBounds): void {
	assertCoordinates({ latitude: bounds.north, longitude: bounds.east });
	assertCoordinates({ latitude: bounds.south, longitude: bounds.west });
	if (bounds.north < bounds.south) {
		throw new GoogleMapsError("invalid-command", "North must be greater than or equal to south.");
	}
}

function assertNonNegative(name: string, value: number): void {
	assertFinite(name, value);
	if (value < 0) throw new GoogleMapsError("invalid-command", `${name} cannot be negative.`);
}

function assertFinite(name: string, value: number): void {
	if (!Number.isFinite(value)) {
		throw new GoogleMapsError("invalid-command", `${name} must be a finite number.`);
	}
}

function normalizeHeading(heading: number): number {
	return ((heading % 360) + 360) % 360;
}
