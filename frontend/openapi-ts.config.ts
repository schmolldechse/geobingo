import { defineConfig } from "@hey-api/openapi-ts";

export default defineConfig({
	input: "http://localhost:5287/openapi/v1.json",
	output: "src/lib/generated/api",
	plugins: [
		{
			name: "@hey-api/typescript",
			enums: false
		}
	]
});
