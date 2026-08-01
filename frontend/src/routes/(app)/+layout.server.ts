import { readProviders } from "$lib/auth/auth.server";
import type { LayoutServerLoad } from "./$types";

export const load: LayoutServerLoad = async ({ depends, locals, url }) => {
	depends("app:auth");

	return {
		session: locals.session,
		providers: await readProviders(),
		returnUrl: `${url.pathname}${url.search}`
	};
};
