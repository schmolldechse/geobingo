export type GoogleMapsLoadState = "idle" | "loading" | "ready" | "error";
export type GoogleMapsView = "map" | "street-view";
export type GoogleMapsStreetViewState = "hidden" | "loading" | "ready" | "unavailable";
export type GoogleMapsMapType = "roadmap" | "satellite" | "hybrid" | "terrain";
export type GoogleMapsGestureHandling = "auto" | "cooperative" | "greedy" | "none";
export type GoogleMapsColorScheme = "light" | "dark" | "follow-system";
export type GoogleMapsRenderingType = "raster" | "vector";
export type GoogleMapsControlPosition =
	| "top-left"
	| "top-center"
	| "top-right"
	| "left-top"
	| "left-center"
	| "left-bottom"
	| "right-top"
	| "right-center"
	| "right-bottom"
	| "bottom-left"
	| "bottom-center"
	| "bottom-right";

export type GoogleMapsErrorCode =
	| "missing-api-key"
	| "configuration-conflict"
	| "load-failed"
	| "initialization-failed"
	| "invalid-command"
	| "street-view-unavailable"
	| "superseded"
	| "destroyed";

export class GoogleMapsError extends Error {
	readonly code: GoogleMapsErrorCode;
	readonly recoverable: boolean;

	constructor(code: GoogleMapsErrorCode, message: string, options: { recoverable?: boolean; cause?: unknown } = {}) {
		super(message, options.cause === undefined ? undefined : { cause: options.cause });
		this.name = "GoogleMapsError";
		this.code = code;
		this.recoverable = options.recoverable ?? true;
	}
}

export interface GoogleMapsCoordinates {
	latitude: number;
	longitude: number;
}

export interface GoogleMapsBounds {
	north: number;
	east: number;
	south: number;
	west: number;
}

export interface GoogleMapsPadding {
	top: number;
	right: number;
	bottom: number;
	left: number;
}

export interface GoogleMapsInitialCamera {
	center: GoogleMapsCoordinates;
	zoom: number;
	heading?: number;
	tilt?: number;
}

export interface GoogleMapsCameraUpdate {
	center?: GoogleMapsCoordinates;
	zoom?: number;
	heading?: number;
	tilt?: number;
}

export interface GoogleMapsCameraSnapshot extends GoogleMapsInitialCamera {
	bounds?: GoogleMapsBounds;
	mapType: GoogleMapsMapType;
}

export interface GoogleMapsStreetViewPosition extends GoogleMapsCoordinates {
	panoramaId: string;
	heading: number;
	pitch: number;
	zoom: number;
}

export type GoogleMapsStreetViewTarget =
	| (Partial<Omit<GoogleMapsStreetViewPosition, "panoramaId">> & {
			panoramaId: string;
	  })
	| (GoogleMapsCoordinates & {
			panoramaId?: never;
			heading?: number;
			pitch?: number;
			zoom?: number;
	  });

export interface GoogleMapsStreetViewLookupOptions {
	fallbackToCoordinates?: boolean;
}

export type GoogleMapsViewSnapshot =
	{ view: "map"; camera: GoogleMapsCameraSnapshot } | { view: "street-view"; position: GoogleMapsStreetViewPosition };

export interface GoogleMapsStateSnapshot {
	loadState: GoogleMapsLoadState;
	view: GoogleMapsView;
	streetViewState: GoogleMapsStreetViewState;
	camera: GoogleMapsCameraSnapshot | null;
	streetViewPosition: GoogleMapsStreetViewPosition | null;
	error: GoogleMapsError | null;
	ready: boolean;
}

export interface GoogleMapsLoaderOptions {
	version?: string;
	language?: string;
	region?: string;
	authReferrerPolicy?: "origin";
	mapIds?: string[];
}

export interface GoogleMapsMapConfiguration {
	mapId?: string;
	colorScheme?: GoogleMapsColorScheme;
	renderingType?: GoogleMapsRenderingType;
	controlSize?: number;
	backgroundColor?: string;
}

export interface GoogleMapsMapControls {
	camera?: boolean;
	fullscreen?: boolean;
	mapType?: boolean;
	rotate?: boolean;
	scale?: boolean;
	streetView?: boolean;
	zoom?: boolean;
	positions?: {
		camera?: GoogleMapsControlPosition;
		fullscreen?: GoogleMapsControlPosition;
		mapType?: GoogleMapsControlPosition;
		rotate?: GoogleMapsControlPosition;
		streetView?: GoogleMapsControlPosition;
		zoom?: GoogleMapsControlPosition;
	};
}

export interface GoogleMapsMapOptions {
	mapType?: GoogleMapsMapType;
	minZoom?: number;
	maxZoom?: number;
	restriction?: {
		bounds: GoogleMapsBounds;
		strict?: boolean;
	};
	gestureHandling?: GoogleMapsGestureHandling;
	keyboardShortcuts?: boolean;
	clickableIcons?: boolean;
	disableDefaultUi?: boolean;
	controls?: GoogleMapsMapControls;
}

export interface GoogleMapsStreetViewControls {
	address?: boolean;
	close?: boolean;
	fullscreen?: boolean;
	imageDate?: boolean;
	links?: boolean;
	motionTracking?: boolean;
	pan?: boolean;
	zoom?: boolean;
}

export interface GoogleMapsStreetViewOptions {
	clickToGo?: boolean;
	scrollwheel?: boolean;
	showRoadLabels?: boolean;
	motionTracking?: boolean;
	disableDefaultUi?: boolean;
	controls?: GoogleMapsStreetViewControls;
}

export type GoogleMapsLoadApi = () => Promise<void>;

export interface GoogleMapsController {
	readonly loadState: GoogleMapsLoadState;
	readonly view: GoogleMapsView;
	readonly streetViewState: GoogleMapsStreetViewState;
	readonly camera: GoogleMapsCameraSnapshot | null;
	readonly streetViewPosition: GoogleMapsStreetViewPosition | null;
	readonly error: GoogleMapsError | null;
	readonly ready: boolean;

	whenReady(): Promise<void>;
	getCamera(): GoogleMapsCameraSnapshot | null;
	getStreetViewPosition(): GoogleMapsStreetViewPosition | null;
	getCurrentViewSnapshot(): GoogleMapsViewSnapshot | null;
	moveCamera(update: GoogleMapsCameraUpdate): Promise<void>;
	panTo(coordinates: GoogleMapsCoordinates): Promise<void>;
	fitBounds(bounds: GoogleMapsBounds, padding?: number | GoogleMapsPadding): Promise<void>;
	showMap(): Promise<void>;
	showStreetView(
		target: GoogleMapsStreetViewTarget,
		options?: GoogleMapsStreetViewLookupOptions
	): Promise<GoogleMapsStreetViewPosition>;
	focus(): Promise<void>;
	refreshSize(): Promise<void>;
	retry(): Promise<void>;
}
