import type {
	GoogleMapsBounds,
	GoogleMapsCameraSnapshot,
	GoogleMapsCameraUpdate,
	GoogleMapsController,
	GoogleMapsCoordinates,
	GoogleMapsError,
	GoogleMapsLoadState,
	GoogleMapsPadding,
	GoogleMapsStateSnapshot,
	GoogleMapsStreetViewPosition,
	GoogleMapsStreetViewState,
	GoogleMapsStreetViewTarget,
	GoogleMapsView,
	GoogleMapsViewSnapshot
} from "./google-maps-types";

type GoogleMapsControllerCommands = {
	whenReady: () => Promise<void>;
	moveCamera: (update: GoogleMapsCameraUpdate) => Promise<void>;
	panTo: (coordinates: GoogleMapsCoordinates) => Promise<void>;
	fitBounds: (bounds: GoogleMapsBounds, padding?: number | GoogleMapsPadding) => Promise<void>;
	showMap: () => Promise<void>;
	showStreetView: (
		target: GoogleMapsStreetViewTarget,
		options?: { fallbackToCoordinates?: boolean }
	) => Promise<GoogleMapsStreetViewPosition>;
	focus: () => Promise<void>;
	refreshSize: () => Promise<void>;
	retry: () => Promise<void>;
};

type GoogleMapsControllerStatePatch = Partial<{
	loadState: GoogleMapsLoadState;
	view: GoogleMapsView;
	streetViewState: GoogleMapsStreetViewState;
	camera: GoogleMapsCameraSnapshot | null;
	streetViewPosition: GoogleMapsStreetViewPosition | null;
	error: GoogleMapsError | null;
}>;

class GoogleMapsControllerImplementation implements GoogleMapsController {
	loadState = $state<GoogleMapsLoadState>("idle");
	view = $state<GoogleMapsView>("map");
	streetViewState = $state<GoogleMapsStreetViewState>("hidden");
	camera = $state<GoogleMapsCameraSnapshot | null>(null);
	streetViewPosition = $state<GoogleMapsStreetViewPosition | null>(null);
	error = $state<GoogleMapsError | null>(null);

	readonly #commands: GoogleMapsControllerCommands;

	constructor(commands: GoogleMapsControllerCommands) {
		this.#commands = commands;
	}

	get ready(): boolean {
		return this.loadState === "ready";
	}

	whenReady(): Promise<void> {
		return this.#commands.whenReady();
	}

	getCamera(): GoogleMapsCameraSnapshot | null {
		return cloneCamera(this.camera);
	}

	getStreetViewPosition(): GoogleMapsStreetViewPosition | null {
		return this.streetViewPosition ? { ...this.streetViewPosition } : null;
	}

	getCurrentViewSnapshot(): GoogleMapsViewSnapshot | null {
		if (this.view === "map") {
			const camera = this.getCamera();
			return camera ? { view: "map", camera } : null;
		}

		const position = this.getStreetViewPosition();
		return this.streetViewState === "ready" && position ? { view: "street-view", position } : null;
	}

	moveCamera(update: GoogleMapsCameraUpdate): Promise<void> {
		return this.#commands.moveCamera(update);
	}

	panTo(coordinates: GoogleMapsCoordinates): Promise<void> {
		return this.#commands.panTo(coordinates);
	}

	fitBounds(bounds: GoogleMapsBounds, padding?: number | GoogleMapsPadding): Promise<void> {
		return this.#commands.fitBounds(bounds, padding);
	}

	showMap(): Promise<void> {
		return this.#commands.showMap();
	}

	showStreetView(
		target: GoogleMapsStreetViewTarget,
		options?: { fallbackToCoordinates?: boolean }
	): Promise<GoogleMapsStreetViewPosition> {
		return this.#commands.showStreetView(target, options);
	}

	focus(): Promise<void> {
		return this.#commands.focus();
	}

	refreshSize(): Promise<void> {
		return this.#commands.refreshSize();
	}

	retry(): Promise<void> {
		return this.#commands.retry();
	}

	update(patch: GoogleMapsControllerStatePatch): void {
		if (patch.loadState !== undefined) this.loadState = patch.loadState;
		if (patch.view !== undefined) this.view = patch.view;
		if (patch.streetViewState !== undefined) this.streetViewState = patch.streetViewState;
		if (patch.camera !== undefined) this.camera = cloneCamera(patch.camera);
		if (patch.streetViewPosition !== undefined) {
			this.streetViewPosition = patch.streetViewPosition ? { ...patch.streetViewPosition } : null;
		}
		if (patch.error !== undefined) this.error = patch.error;
	}

	snapshot(): GoogleMapsStateSnapshot {
		return {
			loadState: this.loadState,
			view: this.view,
			streetViewState: this.streetViewState,
			camera: cloneCamera(this.camera),
			streetViewPosition: this.streetViewPosition ? { ...this.streetViewPosition } : null,
			error: this.error,
			ready: this.ready
		};
	}
}

function cloneCamera(camera: GoogleMapsCameraSnapshot | null): GoogleMapsCameraSnapshot | null {
	return camera
		? {
				...camera,
				center: { ...camera.center },
				bounds: camera.bounds ? { ...camera.bounds } : undefined
			}
		: null;
}

export type GoogleMapsControllerHandle = {
	controller: GoogleMapsControllerImplementation;
	update: (patch: GoogleMapsControllerStatePatch) => GoogleMapsStateSnapshot;
	snapshot: () => GoogleMapsStateSnapshot;
};

export function createGoogleMapsController(
	commands: GoogleMapsControllerCommands,
	onchange: (state: GoogleMapsStateSnapshot) => void
): GoogleMapsControllerHandle {
	const controller = new GoogleMapsControllerImplementation(commands);

	return {
		controller,
		update(patch) {
			controller.update(patch);
			const state = controller.snapshot();
			onchange(state);
			return state;
		},
		snapshot: () => controller.snapshot()
	};
}
