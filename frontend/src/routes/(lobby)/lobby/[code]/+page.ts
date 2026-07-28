import { isValidLobbyCode, normalizeLobbyCode } from "$lib/lobbies/lobby-code";
import { redirect } from "@sveltejs/kit";
import type { PageLoad } from "./$types";

export const load: PageLoad = ({ params }) => {
	const code = normalizeLobbyCode(params.code);
	if (!isValidLobbyCode(code)) redirect(303, "/");

	return { code };
};
