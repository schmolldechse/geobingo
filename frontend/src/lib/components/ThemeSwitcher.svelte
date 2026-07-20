<script lang="ts">
	import { onMount } from "svelte";
	import Moon from "@lucide/svelte/icons/moon";
	import Sun from "@lucide/svelte/icons/sun";
	import { cubicOut } from "svelte/easing";
	import { scale } from "svelte/transition";
	import Button from "@/lib/components/ui/Button.svelte";

	type Theme = "light" | "dark";

	const STORAGE_KEY = "theme";
	const SYSTEM_QUERY = "(prefers-color-scheme: dark)";
	const REDUCED_MOTION_QUERY = "(prefers-reduced-motion: reduce)";
	const THEME_TRANSITION_DURATION = 240;

	let theme = $state<Theme>("light");
	let mounted: boolean = $state(false);
	let reducedMotion: boolean = $state(false);

	let followsSystemTheme: boolean = $state(true);
	let themeTransitionTimer: number | undefined;

	const isDark: boolean = $derived(theme === "dark");
	const resolvedTitle: string = $derived(isDark ? "Activate Light Theme" : "Activate Dark Theme");

	const iconTransition = $derived({
		duration: reducedMotion ? 0 : 140,
		start: 0.8,
		easing: cubicOut
	});

	const getSystemTheme = (): Theme => (window.matchMedia(SYSTEM_QUERY).matches ? "dark" : "light");

	const getStoredTheme = (): Theme | null => {
		const storedTheme = localStorage.getItem(STORAGE_KEY);
		return storedTheme === "light" || storedTheme === "dark" ? storedTheme : null;
	};

	const storeTheme = (theme: Theme): void => localStorage.setItem(STORAGE_KEY, theme);

	const beginTransition = (): void => {
		if (reducedMotion) return;

		const root = document.documentElement;
		if (themeTransitionTimer !== undefined) window.clearTimeout(themeTransitionTimer);

		// Activates the global color transition from app.css temporarily.
		root.dataset.themeTransitioning = "true";

		/*
		 * Forces a style recalculation so that the browser
		 * already knows about the transition before `data-theme` is changed.
		 */
		void root.offsetWidth;

		themeTransitionTimer = window.setTimeout(() => {
			delete root.dataset.themeTransitioning;
			themeTransitionTimer = undefined;
		}, THEME_TRANSITION_DURATION);
	};

	function applyTheme(
		nextTheme: Theme,
		options: {
			persist?: boolean;
			animate?: boolean;
		} = {}
	): void {
		const { persist = true, animate = true } = options;

		if (nextTheme === theme) return;
		if (animate) beginTransition();

		document.documentElement.dataset.theme = nextTheme;
		theme = nextTheme;

		if (persist) {
			followsSystemTheme = false;
			storeTheme(nextTheme);
		}
	}

	const toggleTheme = (): void => applyTheme(isDark ? "light" : "dark");

	onMount(() => {
		const systemThemeQuery = window.matchMedia(SYSTEM_QUERY);
		const reducedMotionQuery = window.matchMedia(REDUCED_MOTION_QUERY);

		const storedTheme = getStoredTheme();
		const documentTheme = document.documentElement.dataset.theme as Theme | undefined;

		followsSystemTheme = storedTheme === null;
		reducedMotion = reducedMotionQuery.matches;

		/*
		 * Synchronizing the `theme`-state, as the inline script in
		 * app.html has already set the `data-theme` attribute on the document before hydration.
		 */
		theme = documentTheme === "dark" ? "dark" : "light";
		mounted = true;

		const handleSystemThemeChange = (event: MediaQueryListEvent): void => {
			if (!followsSystemTheme) return;

			applyTheme(event.matches ? "dark" : "light", {
				persist: false
			});
		};

		const handleReducedMotionChange = (event: MediaQueryListEvent): void => {
			reducedMotion = event.matches;
		};

		const handleStorageChange = (event: StorageEvent): void => {
			if (event.key !== STORAGE_KEY) return;

			if (event.newValue === "light" || event.newValue === "dark") {
				followsSystemTheme = false;

				applyTheme(event.newValue, {
					persist: false
				});

				return;
			}

			/*
			 * If the saved value was deleted in another tab,
			 * the application reverts to the system's behavior.
			 */
			followsSystemTheme = true;

			applyTheme(getSystemTheme(), {
				persist: false
			});
		};

		systemThemeQuery.addEventListener("change", handleSystemThemeChange);
		reducedMotionQuery.addEventListener("change", handleReducedMotionChange);
		window.addEventListener("storage", handleStorageChange);

		return () => {
			systemThemeQuery.removeEventListener("change", handleSystemThemeChange);
			reducedMotionQuery.removeEventListener("change", handleReducedMotionChange);
			window.removeEventListener("storage", handleStorageChange);

			if (themeTransitionTimer !== undefined) window.clearTimeout(themeTransitionTimer);

			delete document.documentElement.dataset.themeTransitioning;
		};
	});
</script>

<Button
	variant="outline"
	size="icon"
	title={resolvedTitle}
	aria-label={resolvedTitle}
	aria-pressed={isDark}
	onclick={toggleTheme}
>
	<span class="relative inline-grid size-5 place-items-center" aria-hidden="true">
		{#if mounted}
			{#key theme}
				<span class="absolute inset-0 grid place-items-center" transition:scale={iconTransition}>
					{#if isDark}
						<Sun size={20} />
					{:else}
						<Moon size={20} />
					{/if}
				</span>
			{/key}
		{:else}
			<!-- SSR-Fallback: show the correct icon based on data-theme without JavaScript state. -->
			<Moon size={20} class="absolute dark:hidden" />
			<Sun size={20} class="absolute hidden dark:block" />
		{/if}
	</span>
</Button>
