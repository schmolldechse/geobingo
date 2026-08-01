<script lang="ts">
	import { enhance } from "$app/forms";
	import ArrowRight from "@lucide/svelte/icons/arrow-right";
	import Laptop from "@lucide/svelte/icons/laptop";
	import LockKeyhole from "@lucide/svelte/icons/lock-keyhole";
	import Plus from "@lucide/svelte/icons/plus";
	import Users from "@lucide/svelte/icons/users";
	import { getAuthState } from "$lib/auth/session.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Input from "$lib/components/ui/Input.svelte";
	import Separator from "$lib/components/ui/Separator.svelte";
	import { toast } from "$lib/components/ui/toast";
	import type { SubmitFunction } from "@sveltejs/kit";

	type LobbyAction = "create" | "resolve";

	const auth = getAuthState();

	let code: string = $state("");
	let pendingAction = $state<LobbyAction | null>(null);

	const actionErrorCopy: Record<LobbyAction, { title: string; fallback: string }> = {
		create: {
			title: "Lobby creation failed",
			fallback: "The lobby could not be created. Please try again."
		},
		resolve: {
			title: "Could not join lobby",
			fallback: "The lobby could not be found. Check the code and try again."
		}
	};

	const getActionErrorMessage = (data: unknown, fallback: string): string => {
		if (!data || typeof data !== "object") return fallback;

		const { fieldErrors, message } = data as Record<string, unknown>;

		if (fieldErrors && typeof fieldErrors === "object") {
			for (const errors of Object.values(fieldErrors)) {
				if (!Array.isArray(errors)) continue;

				const fieldError = errors.find((error): error is string => typeof error === "string" && error.trim().length > 0);
				if (fieldError) return fieldError;
			}
		}

		return typeof message === "string" && message.trim().length > 0 ? message : fallback;
	};

	const enhanceAction =
		(action: LobbyAction): SubmitFunction =>
		() => {
			pendingAction = action;
			return async ({ result, update }) => {
				try {
					if (result.type === "failure") {
						const { title, fallback } = actionErrorCopy[action];
						toast.error(title, {
							description: getActionErrorMessage(result.data, fallback)
						});
					}

					await update();
				} finally {
					pendingAction = null;
				}
			};
		};
	const createSubmit = enhanceAction("create");
	const resolveSubmit = enhanceAction("resolve");
</script>

<svelte:head>
	<title>GeoBingo</title>
	<meta name="description" content="Multiplayer Street View adventures" />
</svelte:head>

<div class="relative left-1/2 -my-8 w-dvw -translate-x-1/2 overflow-hidden sm:-my-12">
	<section class="border-border bg-background relative overflow-hidden border-b-2" aria-labelledby="homepage-title">
		<div
			class="border-secondary/30 pointer-events-none absolute -top-40 -left-36 size-[26rem] rotate-[-17deg] border-2"
			style="border-radius: 48% 52% 67% 33% / 35% 37% 63% 65%"
			aria-hidden="true"
		></div>
		<div
			class="border-accent/20 pointer-events-none absolute -right-44 -bottom-60 size-[32rem] rounded-full border-[4.5rem]"
			aria-hidden="true"
		></div>

		<div
			class="relative z-10 mx-auto grid min-h-[calc(100dvh-4rem)] max-w-7xl items-center gap-12 px-4 py-16 sm:px-6 sm:py-20 lg:grid-cols-[minmax(0,1.08fr)_minmax(22.5rem,0.72fr)] lg:gap-20 lg:py-24"
		>
			<div class="relative max-w-3xl min-w-0">
				<p class="text-primary mb-5 inline-flex items-center gap-2 text-xs font-black tracking-[0.17em] uppercase">
					<span class="bg-primary h-0.75 w-6" aria-hidden="true"></span>
					Multiplayer Street View adventures
				</p>

				<h1
					id="homepage-title"
					class="m-0 max-w-3xl text-[clamp(3rem,7vw,5.5rem)] leading-[0.92] font-black tracking-[-0.07em]"
				>
					Your next game night is
					<span class="text-secondary relative inline-block">
						somewhere on Earth.
						<svg
							aria-hidden="true"
							class="text-primary absolute inset-x-0 -bottom-2 h-3 w-full"
							viewBox="0 0 220 9"
							preserveAspectRatio="none"
						>
							<path d="M2 6C48 2 127 2 218 5" fill="none" stroke="currentColor" stroke-width="4" stroke-linecap="round" />
						</svg>
					</span>
				</h1>

				<p class="text-muted mt-8 max-w-xl text-base leading-relaxed font-medium sm:text-xl">
					Explore unexpected places, set the goals, and discover how your friends see the world.
				</p>

				<ul
					class="text-muted mt-8 flex list-none flex-wrap gap-x-5 gap-y-3 p-0 text-xs font-bold sm:text-sm"
					aria-label="Game details"
				>
					<li class="inline-flex items-center gap-2">
						<Users class="text-secondary" size={18} aria-hidden="true" />
						2–32 players
					</li>
					<li class="inline-flex items-center gap-2">
						<LockKeyhole class="text-secondary" size={18} aria-hidden="true" />
						Private real-time lobby
					</li>
					<li class="inline-flex items-center gap-2">
						<Laptop class="text-secondary" size={18} aria-hidden="true" />
						Browser-based
					</li>
				</ul>
			</div>

			<div class="relative">
				<div
					class="bg-accent border-foreground pointer-events-none absolute -top-4 -right-3 h-20 w-32 rotate-6 rounded-2xl border-2"
					aria-hidden="true"
				></div>

				<section
					class="border-foreground bg-surface relative rounded-[1.4rem] border-2 p-5 shadow-[7px_7px_0_var(--foreground)] sm:p-8"
					aria-label="Lobby quick start"
				>
					<div class="absolute top-5 right-5 grid gap-2" aria-hidden="true">
						<span class="bg-accent size-2 rounded-full"></span>
						<span class="bg-primary size-2 rounded-full"></span>
						<span class="bg-secondary size-2 rounded-full"></span>
					</div>

					<div class="pr-9">
						<p class="text-primary m-0 text-xs font-black tracking-[0.15em] uppercase">Ready for a new adventure?</p>
						<h2 class="mt-2 mb-0 text-3xl leading-none font-black tracking-[-0.04em]">Find your lobby</h2>
						<p class="text-muted mt-3 mb-0 text-sm leading-relaxed">Enter the eight-character code shared by your host.</p>
					</div>

					<form
						method="POST"
						action="?/resolveLobby"
						use:enhance={resolveSubmit}
						class="mt-5"
						aria-busy={pendingAction === "resolve"}
					>
						<label for="lobby-code" class="mb-2 block text-xs font-extrabold">Lobby code</label>
						<div class="grid gap-3 sm:grid-cols-[minmax(0,1fr)_auto]">
							<Input
								id="lobby-code"
								name="code"
								inputmode="text"
								autocomplete="off"
								maxlength={8}
								placeholder="MAPS2FUN"
								bind:value={code}
								class="min-h-13 min-w-0 px-3 text-center text-lg font-black tracking-[0.15em] uppercase"
							/>

							{#if auth.session.authenticated}
								<Button type="submit" size="lg" class="w-full sm:w-auto" disabled={pendingAction !== null}>
									{pendingAction === "resolve" ? "Finding lobby …" : "Join lobby"}
									<ArrowRight size={19} aria-hidden="true" />
								</Button>
							{:else}
								<Button type="button" size="lg" class="w-full sm:w-auto" onclick={auth.openLoginDialog}>
									Join lobby <ArrowRight size={19} aria-hidden="true" />
								</Button>
							{/if}
						</div>
					</form>

					<div class="my-6 flex items-center gap-3">
						<Separator orientation="horizontal" class="flex-1" />
						<span class="text-muted text-xs font-black tracking-[0.12em] uppercase">or start fresh</span>
						<Separator orientation="horizontal" class="flex-1" />
					</div>

					<form method="POST" action="?/createLobby" use:enhance={createSubmit} aria-busy={pendingAction === "create"}>
						{#if auth.session.authenticated}
							<Button type="submit" variant="accent" size="lg" class="w-full" disabled={pendingAction !== null}>
								<Plus size={20} aria-hidden="true" />
								{pendingAction === "create" ? "Creating lobby …" : "Create a new lobby"}
							</Button>
						{:else}
							<Button type="button" variant="accent" size="lg" class="w-full" onclick={auth.openLoginDialog}>
								<Plus size={20} aria-hidden="true" /> Create a new lobby
							</Button>
						{/if}
					</form>
				</section>
			</div>
		</div>
	</section>

	<section class="bg-primary text-primary-foreground relative overflow-hidden" aria-labelledby="closing-cta-title">
		<div
			class="pointer-events-none absolute -top-32 -left-16 size-80 rounded-full border-3 border-current opacity-15"
			aria-hidden="true"
		></div>
		<div
			class="pointer-events-none absolute -right-28 -bottom-52 size-[30rem] rounded-full border-3 border-current opacity-15"
			aria-hidden="true"
		></div>

		<div
			class="relative z-10 mx-auto flex max-w-7xl flex-col items-start justify-between gap-8 px-4 py-16 sm:px-6 sm:py-20 lg:flex-row lg:items-center"
		>
			<div>
				<h2 id="closing-cta-title" class="m-0 max-w-3xl text-4xl leading-[0.96] font-black tracking-[-0.06em] sm:text-6xl">
					Where will your group end up tonight?
				</h2>
				<p class="mt-5 mb-0 max-w-xl text-sm leading-relaxed opacity-85 sm:text-base">
					Create a lobby, invite your friends, and let GeoBingo pick the destination.
				</p>
			</div>

			{#if auth.session.authenticated}
				<form
					method="POST"
					action="?/createLobby"
					use:enhance={createSubmit}
					aria-busy={pendingAction === "create"}
					class="w-full lg:w-auto"
				>
					<Button type="submit" variant="accent" size="lg" class="w-full lg:w-auto" disabled={pendingAction !== null}>
						{pendingAction === "create" ? "Creating lobby …" : "Create a lobby"}
						<ArrowRight size={20} aria-hidden="true" />
					</Button>
				</form>
			{:else}
				<Button type="button" variant="accent" size="lg" class="w-full lg:w-auto" onclick={auth.openLoginDialog}>
					Create a lobby <ArrowRight size={20} aria-hidden="true" />
				</Button>
			{/if}
		</div>
	</section>
</div>
