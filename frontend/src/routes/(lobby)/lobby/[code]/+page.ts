import { apiUrl } from "$lib/api-url";
import type { GameModeSummary, ResolveLobbyResponse } from "$lib/generated/api";
import { isValidLobbyCode, normalizeLobbyCode } from "$lib/lobbies/lobby-code";
import { error } from "@sveltejs/kit";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch, params, parent }) => {
	await parent();

	const code = normalizeLobbyCode(params.code);
	if (!isValidLobbyCode(code)) error(404, "This lobby could not be found.");

	const gameModeCatalogRequest = fetch(apiUrl("/api/v1/game-modes"), {
		headers: { accept: "application/json" }
	}).catch(() => null);

	const [response, gameModeCatalogResponse] = await Promise.all([
		fetch(apiUrl(`/api/v1/lobbies/${code}`), {
			credentials: "include",
			headers: { accept: "application/json" }
		}),
		gameModeCatalogRequest
	]);

	if (response.status === 401) error(401, "Your session has expired.");
	if (response.status === 404) error(404, "This lobby could not be found.");
	if (!response.ok) error(503, "This lobby is temporarily unavailable.");

	const resolution = (await response.json()) as ResolveLobbyResponse;
	let availableGameModes: GameModeSummary[] = [resolution.selectedMode];
	let gameModeCatalogAvailable = false;

	if (gameModeCatalogResponse?.ok) {
		try {
			const catalog = (await gameModeCatalogResponse.json()) as GameModeSummary[];
			if (Array.isArray(catalog)) {
				availableGameModes = catalog;
				gameModeCatalogAvailable = true;
			}
		} catch {
			// The selected mode from the lobby resolution remains a safe fallback.
		}
	}

	return { code, availableGameModes, gameModeCatalogAvailable };
};
