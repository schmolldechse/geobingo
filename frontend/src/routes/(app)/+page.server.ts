import { apiUrl } from "$lib/api-url";
import { isValidLobbyCode, normalizeLobbyCode } from "$lib/lobbies/lobby-code";
import { fail, redirect } from "@sveltejs/kit";
import type { Actions } from "./$types";
import type { CreateLobbyResponse, ResolveLobbyCodeResponse } from "@/lib/generated/api";

const authRequired = () =>
	fail(401, {
		message: "Melde dich an, um eine Lobby zu erstellen oder ihr beizutreten.",
		authRequired: true
	});

export const actions: Actions = {
	createLobby: async ({ locals }) => {
		if (!locals.session.authenticated) return authRequired();

		const response = await fetch(apiUrl("/api/v1/lobbies"), {
			method: "POST",
			credentials: "include",
			headers: { accept: "application/json" }
		});
		if (!response.ok)
			return fail(response.status, {
				message: "Lobby could not be created. ",
				details: response.statusText
			});

		const data = (await response.json()) as CreateLobbyResponse;
		redirect(303, `/lobby/${data.code}`);
	},

	resolveLobby: async ({ locals, request }) => {
		if (!locals.session.authenticated) return authRequired();

		const formData = await request.formData();
		const code = normalizeLobbyCode(String(formData.get("code") ?? ""));
		if (!isValidLobbyCode(code))
			return fail(400, {
				code,
				fieldErrors: {
					code: ["Der Lobby-Code besteht aus genau acht Zeichen (A–Z und 0–9)."]
				}
			});

		const response = await fetch(apiUrl(`/api/v1/lobbies/${code}`), {
			method: "GET",
			credentials: "include",
			headers: { accept: "application/json" }
		});
		if (!response.ok)
			return fail(response.status, {
				message: "Lobby could not be resolved.",
				details: response.statusText
			});

		const data = (await response.json()) as ResolveLobbyCodeResponse;
		if (!data.joinable && !data.reconnectable) {
			return fail(409, {
				code,
				message: "Diese Lobby nimmt aktuell keine weiteren Spieler auf."
			});
		}

		redirect(303, `/lobby/${data.code}`);
	}
};
