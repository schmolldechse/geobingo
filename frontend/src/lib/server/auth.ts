import { env } from "$env/dynamic/public";
import type { AuthProvider, Session } from "$lib/generated/api";

const FALLBACK_API_URL = "http://localhost:5287/";

const anonymousSession = (): Session => ({
	authenticated: false,
	user: null
});

const getApiBaseUrl = (): string => {
	const configuredUrl = env.PUBLIC_API_URL?.trim();
	const baseUrl = configuredUrl && configuredUrl.length > 0 ? configuredUrl : FALLBACK_API_URL;
	return new URL(baseUrl.endsWith("/") ? baseUrl : `${baseUrl}/`).href;
};

const getApiUrl = (path: string): URL => new URL(path.replace(/^\/+/, ""), getApiBaseUrl());

const readSession = async (cookieHeader: string | null): Promise<Session> => {
	const headers = new Headers({ accept: "application/json" });
	if (cookieHeader) headers.set("cookie", cookieHeader);

	const response = await fetch(getApiUrl("/api/auth/session"), {
		headers,
		cache: "no-store"
	});
	if (!response.ok) return anonymousSession();

	const session = (await response.json()) as Session;
	if (session.authenticated && !session.user) return anonymousSession();
	return session;
};

const readProviders = async (): Promise<AuthProvider[]> => {
	const response = await fetch(getApiUrl("/api/auth/providers"), {
		headers: { accept: "application/json" },
		cache: "no-store"
	});
	if (!response.ok) return [];

	const providers = (await response.json()) as AuthProvider[];
	return Array.isArray(providers) ? providers : [];
};

export { anonymousSession, getApiBaseUrl, readSession, readProviders };
