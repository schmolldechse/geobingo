import { apiUrl } from "$lib/api-url";
import type { AuthProvider, Session } from "$lib/generated/api";

const anonymousSession = (): Session => ({
	authenticated: false,
	user: null
});

const readSession = async (cookieHeader: string | null): Promise<Session> => {
	const headers = new Headers({ accept: "application/json" });
	if (cookieHeader) headers.set("cookie", cookieHeader);

	const response = await fetch(apiUrl("/api/auth/session"), {
		headers,
		cache: "no-store"
	});
	if (!response.ok) return anonymousSession();

	const session = (await response.json()) as Session;
	if (session.authenticated && !session.user) return anonymousSession();
	return session;
};

const readProviders = async (): Promise<AuthProvider[]> => {
	const response = await fetch(apiUrl("/api/auth/providers"), {
		headers: { accept: "application/json" },
		cache: "no-store"
	});
	if (!response.ok) return [];

	const providers = (await response.json()) as AuthProvider[];
	return Array.isArray(providers) ? providers : [];
};

export { anonymousSession, readSession, readProviders };
