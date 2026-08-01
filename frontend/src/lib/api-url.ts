import { env } from "$env/dynamic/public";

function apiBaseUrl(): URL {
	const configuredApiUrl = env.PUBLIC_API_URL?.trim();
	if (!configuredApiUrl) throw new Error("PUBLIC_API_URL must be configured.");

	try {
		return new URL(configuredApiUrl);
	} catch {
		throw new Error("PUBLIC_API_URL must be a valid absolute URL.");
	}
}

const apiUrl = (path: string): URL => new URL(path, apiBaseUrl());

export { apiUrl };
