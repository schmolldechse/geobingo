import tailwindcss from "@tailwindcss/vite";
import adapter from "svelte-adapter-bun";
import { sveltekit } from "@sveltejs/kit/vite";
import { defineConfig } from "vite";

export default defineConfig({
	plugins: [
		tailwindcss(),
		sveltekit({
			compilerOptions: {
				// Force runes mode for the project, except for libraries. Can be removed in svelte 6.
				runes: ({ filename }) => (filename.split(/[/\\]/).includes("node_modules") ? undefined : true),
				experimental: { async: true }
			},

			adapter: adapter(),
			experimental: { remoteFunctions: true },

			alias: {
				"@": "src",
				"@lib": "src/lib",
				"@components": "src/lib/components"
			}
		})
	]
});
