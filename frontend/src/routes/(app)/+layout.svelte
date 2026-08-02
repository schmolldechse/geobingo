<script lang="ts">
	import { env } from "$env/dynamic/public";
	import { AuthState, setAuthState } from "$lib/auth/session.svelte";
	import AccountControl from "$lib/components/AccountControl.svelte";
	import BrandMark from "$lib/components/BrandMark.svelte";
	import ThemeSwitcher from "$lib/components/ThemeSwitcher.svelte";
	import type { LayoutProps } from "./$types";

	let { data, children }: LayoutProps = $props();

	const authState = new AuthState(() => ({
		session: data.session,
		providers: data.providers,
		returnUrl: data.returnUrl
	}));
	setAuthState(authState);

	const appVersion = env.PUBLIC_APP_VERSION?.trim() || "development";
</script>

<div class="bg-background text-foreground flex min-h-screen flex-col">
	<header class="border-border bg-background/95 sticky top-0 z-40 border-b-2 backdrop-blur">
		<nav
			class="mx-auto flex min-h-16 w-full max-w-7xl items-center justify-between gap-3 px-4 py-2 sm:px-6"
			aria-label="Main navigation"
		>
			<a
				href="/"
				class="text-foreground inline-flex min-w-0 items-center rounded-lg no-underline"
				aria-label="GeoBingo Homepage"
			>
				<BrandMark />
			</a>

			<div class="flex shrink-0 items-center gap-2">
				<ThemeSwitcher />
				<AccountControl />
			</div>
		</nav>
	</header>

	<main id="main-content" class="mx-auto w-full max-w-7xl flex-1 px-4 py-8 sm:px-6 sm:py-12">
		{@render children()}
	</main>

	<footer class="border-border bg-surface text-foreground border-t">
		<div
			class="mx-auto flex w-full max-w-7xl flex-col gap-4 px-4 py-7 text-xs md:flex-row md:items-center md:justify-between md:px-6 md:py-8"
		>
			<p class="m-0 opacity-70">
				© 2026 GeoBingo {appVersion}
			</p>
			<a href="https://github.com/schmolldechse/geobingo" class="text-foreground w-fit font-bold no-underline hover:underline">
				GitHub
			</a>
		</div>
	</footer>
</div>
