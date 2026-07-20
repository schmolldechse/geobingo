import { getApiBaseUrl, readProviders } from "$lib/server/auth";
import type { LayoutServerLoad } from "./$types";

export const load: LayoutServerLoad = async ({ depends, locals, url }) => {
	depends("app:auth");

	return {
		session: locals.session,
		providers: await readProviders(),
		apiBaseUrl: getApiBaseUrl(),
		returnUrl: `${url.pathname}${url.search}`
	};
};
