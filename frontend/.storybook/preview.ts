import type { Preview } from "@storybook/sveltekit";

import "../src/app.css";

const preview: Preview = {
	initialGlobals: {
		theme: "light"
	},
	globalTypes: {
		theme: {
			description: "GeoBingo Theme",
			toolbar: {
				icon: "paintbrush",
				items: [
					{ value: "light", title: "Light" },
					{ value: "dark", title: "Dark" }
				]
			}
		}
	},
	decorators: [
		(Story, context) => {
			document.documentElement.dataset.theme = context.globals.theme === "dark" ? "dark" : "light";
			return Story();
		}
	],
	parameters: {
		controls: {
			matchers: {
				color: /(background|color)$/i,
				date: /Date$/i
			}
		}
	}
};

export default preview;
