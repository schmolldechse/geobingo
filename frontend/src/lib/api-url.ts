import { env } from "$env/dynamic/public";

const configuredApiUrl = env.PUBLIC_API_URL?.trim();
if (!configuredApiUrl) throw new Error("PUBLIC_API_URL must be configured.");

let apiBaseUrl: URL;
try {
	apiBaseUrl = new URL(configuredApiUrl);
} catch {
	throw new Error("PUBLIC_API_URL must be a valid absolute URL.");
}

const apiUrl = (path: string): URL => new URL(path, apiBaseUrl);

export { apiUrl };
