# GeoBingo frontend

The frontend is the SvelteKit client for GeoBingo. It renders the public entry page and lobby experience, communicates with the API over REST and SignalR, and embeds Google Maps and Street View for gameplay.

Its main dependencies are Svelte 5, SvelteKit, Bun, Tailwind CSS, SignalR, the Google Maps JavaScript API, Valibot, and Storybook.

## Local development

The API must be running before lobby, authentication, or contract-generation workflows can work.

Complete the shared [Google Cloud setup](../README.md#google-cloud-setup) first. Use the Maps API key created for that project below.

From `frontend`, copy the example environment file:

```bash
cp .env.example .env
```

Set the generated API key in `.env`:

```dotenv
PUBLIC_GOOGLE_MAPS_API_KEY=your-google-maps-api-key
```

Then install the dependencies and start the development server:

```bash
bun install
bun -bun run dev
```

The development server uses Vite's default address, `http://localhost:5173`.

## Storybook

Storybook is the catalog for the reusable UI components in `src/lib/components/ui`. Every reusable component must have stories that demonstrate all supported public variants, states, interactions, and meaningful compositions.

Place story files in `src/stories`. A standalone component uses `src/stories/Component.stories.svelte`. When a component family contains multiple child components, or its stories require examples, fixtures, or other supporting files, group the story and all related files under `src/stories/<component-name>/`. Follow the existing `google-maps` and `tabs` directories as references.

Build the static Storybook to verify the complete catalog:

```bash
bun --bun run build-storybook
```

Start Storybook locally:

```bash
bun --bun run storybook
```

## Generated clients

### REST contracts

Start the API at `http://localhost:5287`, then run:

```bash
bun --bun run generate:rest
```

The generator reads `http://localhost:5287/openapi/v1.json` and writes the TypeScript client to `src/lib/generated/api`.

### SignalR contracts

Restore the repository's local .NET tools once:

```bash
cd ../backend
dotnet tool restore
cd ../frontend
```

Then generate the realtime contracts:

```bash
bun --bun run generate:realtime
```

The command reads the annotated interfaces in `GeoBingo.Contracts` and writes TypeScript definitions to `src/lib/generated/realtime`.
