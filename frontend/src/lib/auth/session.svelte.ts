import { invalidate } from "$app/navigation";
import type { AuthProvider, Session } from "$lib/generated/api";
import { createContext } from "svelte";

type AuthStateData = {
	session: Readonly<Session>;
	providers: readonly AuthProvider[];
	apiBaseUrl: string;
	returnUrl: string;
};

type AuthStateSource = () => AuthStateData;

class AuthState {
	readonly #source: AuthStateSource;

	public constructor(source: AuthStateSource) {
		this.#source = source;
	}

	public get session(): Readonly<Session> {
		return this.#source().session;
	}

	public get providers(): readonly AuthProvider[] {
		return this.#source().providers;
	}

	public getLoginUrl(providerKey: string): string {
		const { apiBaseUrl, returnUrl } = this.#source();
		const url = new URL(`/api/auth/login/${encodeURIComponent(providerKey)}`, apiBaseUrl);
		url.searchParams.set("returnUrl", returnUrl);
		return url.href;
	}

	public async logout(): Promise<void> {
		const response = await fetch(new URL("/api/auth/logout", this.#source().apiBaseUrl), {
			method: "POST",
			credentials: "include",
			headers: { accept: "application/json" }
		});

		if (!response.ok) throw new Error("Logout request failed.");
		await invalidate("app:auth");
	}
}

const [getAuthState, setAuthState] = createContext<AuthState>();

export { type AuthStateData, AuthState, getAuthState, setAuthState };
