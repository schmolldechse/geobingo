import type { GoogleMapsStreetViewPosition } from "$lib/components/ui/google-maps";
import type { StreetViewPosition } from "$lib/generated/realtime/GeoBingo.Contracts.GameModes.CaptureChallenge";

const POSITION_EPSILON = 0.000001;

export function toCaptureStreetViewPosition(position: GoogleMapsStreetViewPosition | null): StreetViewPosition | null {
	if (!position) return null;

	const panoramaId = position.panoramaId.trim();
	if (!panoramaId || panoramaId.length > 512) return null;
	if (!isFiniteRange(position.latitude, -90, 90)) return null;
	if (!isFiniteRange(position.longitude, -180, 180)) return null;
	if (!isFiniteRange(position.pitch, -90, 90)) return null;
	if (!Number.isFinite(position.heading) || !Number.isFinite(position.zoom)) return null;

	return {
		panoramaId,
		latitude: position.latitude,
		longitude: position.longitude,
		heading: normalizeHeading(position.heading),
		pitch: position.pitch,
		zoom: Math.min(5, Math.max(0, position.zoom))
	};
}

export function sameCaptureStreetViewPosition(left: StreetViewPosition | null, right: StreetViewPosition | null): boolean {
	if (!left || !right) return left === right;

	return (
		left.panoramaId === right.panoramaId &&
		nearlyEqual(left.latitude, right.latitude) &&
		nearlyEqual(left.longitude, right.longitude) &&
		circularHeadingDistance(left.heading, right.heading) <= POSITION_EPSILON &&
		nearlyEqual(left.pitch, right.pitch) &&
		nearlyEqual(left.zoom, right.zoom)
	);
}

function isFiniteRange(value: number, minimum: number, maximum: number): boolean {
	return Number.isFinite(value) && value >= minimum && value <= maximum;
}

function normalizeHeading(heading: number): number {
	return ((heading % 360) + 360) % 360;
}

function circularHeadingDistance(left: number, right: number): number {
	const difference = Math.abs(normalizeHeading(left) - normalizeHeading(right));
	return Math.min(difference, 360 - difference);
}

function nearlyEqual(left: number, right: number): boolean {
	return Math.abs(left - right) <= POSITION_EPSILON;
}
