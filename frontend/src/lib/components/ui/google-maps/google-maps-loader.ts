import { importLibrary, setOptions } from "@googlemaps/js-api-loader";
import { GoogleMapsError, type GoogleMapsLoadApi, type GoogleMapsLoaderOptions } from "./google-maps-types";

export interface GoogleMapsLibraries {
	maps: google.maps.MapsLibrary;
	streetView: google.maps.StreetViewLibrary;
	core: google.maps.CoreLibrary;
}

let configuredSignature: string | null = null;
let librariesPromise: Promise<GoogleMapsLibraries> | null = null;

function normalizedOptions(options: GoogleMapsLoaderOptions): GoogleMapsLoaderOptions {
	return {
		version: options.version?.trim() || "weekly",
		language: options.language?.trim() || undefined,
		region: options.region?.trim() || undefined,
		authReferrerPolicy: options.authReferrerPolicy,
		mapIds: options.mapIds
			?.map((mapId) => mapId.trim())
			.filter(Boolean)
			.sort()
	};
}

function configurationSignature(apiKey: string, options: GoogleMapsLoaderOptions): string {
	return JSON.stringify({ apiKey, ...normalizedOptions(options) });
}

async function importLibraries(): Promise<GoogleMapsLibraries> {
	const [maps, streetView, core] = await Promise.all([
		importLibrary("maps"),
		importLibrary("streetView"),
		importLibrary("core")
	]);

	return { maps, streetView, core };
}

export async function loadGoogleMaps(
	options: GoogleMapsLoaderOptions = {},
	loadApi?: GoogleMapsLoadApi
): Promise<GoogleMapsLibraries> {
	if (typeof window === "undefined") {
		throw new GoogleMapsError("initialization-failed", "Google Maps can only be loaded in a browser.", {
			recoverable: false
		});
	}

	if (loadApi) {
		try {
			await loadApi();
		} catch (error) {
			if (error instanceof GoogleMapsError) throw error;
			throw new GoogleMapsError("load-failed", "Google Maps could not be loaded.", { cause: error });
		}
		if (!window.google?.maps?.importLibrary) {
			throw new GoogleMapsError(
				"initialization-failed",
				"The custom Google Maps loader completed without installing the Maps API."
			);
		}
		return importLibraries();
	}

	const apiKey = await readApiKey();
	if (!apiKey) {
		throw new GoogleMapsError(
			"missing-api-key",
			"Google Maps is not configured. Set PUBLIC_GOOGLE_MAPS_API_KEY for this environment."
		);
	}

	const normalized = normalizedOptions(options);
	const signature = configurationSignature(apiKey, normalized);
	if (configuredSignature && configuredSignature !== signature) {
		throw new GoogleMapsError("configuration-conflict", "Google Maps was already loaded with different global options.", {
			recoverable: false
		});
	}

	if (!configuredSignature) {
		setOptions({
			key: apiKey,
			v: normalized.version,
			...(normalized.language ? { language: normalized.language } : {}),
			...(normalized.region ? { region: normalized.region } : {}),
			...(normalized.authReferrerPolicy ? { authReferrerPolicy: normalized.authReferrerPolicy } : {}),
			...(normalized.mapIds?.length ? { mapIds: normalized.mapIds } : {})
		});
		configuredSignature = signature;
	}

	if (!librariesPromise) {
		librariesPromise = importLibraries().catch((error: unknown) => {
			librariesPromise = null;
			throw new GoogleMapsError("load-failed", "Google Maps could not be loaded.", { cause: error });
		});
	}

	return librariesPromise;
}

async function readApiKey(): Promise<string | undefined> {
	try {
		const publicEnv = await import("$env/static/public");
		const apiKey: unknown = Reflect.get(publicEnv, "PUBLIC_GOOGLE_MAPS_API_KEY");
		return typeof apiKey === "string" ? apiKey.trim() || undefined : undefined;
	} catch {
		return undefined;
	}
}
