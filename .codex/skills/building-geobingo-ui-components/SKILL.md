---
name: building-geobingo-ui-components
description: Use when creating, refactoring, or reviewing reusable Svelte 5 UI components in GeoBingo's frontend/src/lib/components/ui.
---

# Building GeoBingo UI Components

## Overview

Build semantic primitives with explicit APIs and predictable composition. Apply these rules even to tiny components.

## Workflow

1. Inspect existing primitives, tokens, and Storybook patterns before adding an abstraction.
2. Choose the smallest valid file structure.
3. Define semantics and API, then model source state once and derive the rest.
4. Add accessibility, state hooks, styling, icons, and a documentation-only story.
5. Run `bun run check` and `bun run build-storybook` from `frontend`.

## Component Contract

| Concern | Requirement |
|---|---|
| Simple unit | Place `PascalCase.svelte` directly in `frontend/src/lib/components/ui/`. |
| Compound unit | Use `ui/<component-name>/` for child components, Context, helpers, or barrel exports. Keep component files PascalCase. |
| Context | Put Context logic in `*-context.svelte.ts`; use a module-local, uniquely described `Symbol(...)` and typed set/get helpers. |
| Native API | Start with semantic HTML. Type from `svelte/elements`; forward applicable native attributes, events, `class`, ARIA, and consumer `data-*`. |
| Composition | Reuse existing primitives and Svelte snippets. Expose a bindable DOM ref only when needed. |
| Buttons | Default reusable action buttons to `type="button"`; require explicit submit behavior. |
| State | Keep source state in `$state` or bindings and consequences in `$derived`; prefer handlers and lifecycle callbacks over synchronization. |
| Effects | Never derive or mirror state with `$effect`. Use it only for unavoidable imperative integration; guard it and return cleanup. |
| Browser APIs | Access browser globals only in safe event or lifecycle paths. Preserve SSR/hydration markup. |
| State hooks | Add stable component/state `data-*` attributes when useful for styling or inspection. They never replace native semantics or ARIA. |
| Accessibility | Add ARIA only for real names, states, or relationships. Avoid redundant roles; name icon-only controls and hide decorative icons. |
| Icons | Import only direct Lucide modules, for example `import Camera from "@lucide/svelte/icons/camera";`. Never import from `@lucide/svelte`'s barrel. |
| Styling | Use semantic tokens, both themes, and reduced motion. Separate consumer layout from semantic variants. |
| Storybook | Co-locate `PascalCase.stories.svelte`. Document variants, sizes, meaningful states, composition, and theme behavior. Do not add `play` functions or test dependencies. |

## Context Pattern

```ts
import { getContext, setContext } from "svelte";

const CONTEXT_KEY = Symbol("GeoBingoAccordion");

export function setAccordionContext(value: AccordionContext): void {
	setContext(CONTEXT_KEY, value);
}

export function getAccordionContext(): AccordionContext {
	const value = getContext<AccordionContext>(CONTEXT_KEY);
	if (!value) throw new Error("Accordion child must be inside AccordionRoot.");
	return value;
}
```

## Common Mistakes

- Omitting native prop forwarding or Storybook because a component is tiny.
- Creating a folder, helper, Context, or barrel before the component actually needs one.
- Mirroring derived values through chained effects instead of `$derived`.
- Treating `data-state` as accessibility semantics or importing Lucide's central barrel.

## Completion Check

Confirm structure, PascalCase, native semantics, prop forwarding, reactivity, direct icon imports, SSR safety, state attributes, ARIA, semantic tokens, themes, reduced motion, and the co-located documentation-only story before delivery.
