import { redirect } from "@sveltejs/kit";
import type { LayoutServerLoad } from "./$types";

export const load: LayoutServerLoad = ({ locals }) => {
	if (!locals.session.authenticated) redirect(303, "/");

	return {
		session: locals.session
	};
};
