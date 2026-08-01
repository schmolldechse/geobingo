import { invalidate } from "$app/navigation";
import { apiUrl } from "$lib/api-url";
import type { AuthProvider, Session } from "$lib/generated/api";
import { createContext } from "svelte";

type AuthStateData = {
	session: Readonly<Session>;
	providers: readonly AuthProvider[];
	returnUrl: string;
};

type AuthStateSource = () => AuthStateData;

class AuthState {
	readonly #source: AuthStateSource;
	#loginDialogVisible: boolean = $state(false);

	public constructor(source: AuthStateSource) {
		this.#source = source;
	}

	public get session(): Readonly<Session> {
		return this.#source().session;
	}

	public get providers(): readonly AuthProvider[] {
		return this.#source().providers;
	}

	public get loginDialogVisible(): boolean {
		return this.#loginDialogVisible;
	}

	public set loginDialogVisible(value: boolean) {
		this.#loginDialogVisible = value;
	}

	public openLoginDialog = (): void => {
		this.loginDialogVisible = true;
	};

	public closeLoginDialog = (): void => {
		this.loginDialogVisible = false;
	};

	public getLoginUrl(providerKey: string): string {
		const { returnUrl } = this.#source();

		const url = apiUrl(`/api/auth/login/${encodeURIComponent(providerKey)}`);
		url.searchParams.set("returnUrl", returnUrl);
		return url.href;
	}

	public async logout(): Promise<void> {
		const response = await fetch(apiUrl("/api/auth/logout"), {
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
