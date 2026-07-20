<script lang="ts">
	import ArrowRight from "@lucide/svelte/icons/arrow-right";
	import ChevronDown from "@lucide/svelte/icons/chevron-down";
	import LogIn from "@lucide/svelte/icons/log-in";
	import LogOut from "@lucide/svelte/icons/log-out";
	import { getAuthState } from "$lib/auth/session.svelte";
	import * as DropdownMenu from "$lib/components/ui/dropdown-menu";
	import Button from "@/lib/components/ui/Button.svelte";
	import Dialog from "@/lib/components/ui/Dialog.svelte";

	const auth = getAuthState();

	let logoutPending = $state(false);
	let logoutError = $state<string | null>(null);
	let loginDialogVisible = $state(false);

	const initials = $derived.by(() => {
		const parts = auth.session.user?.displayName.trim().split(/\s+/).filter(Boolean) ?? [];
		if (parts.length === 0) return "?";

		const firstInitial = Array.from(parts[0])[0] ?? "";
		const lastInitial = parts.length > 1 ? (Array.from(parts.at(-1) ?? "")[0] ?? "") : "";
		return `${firstInitial}${lastInitial}`.toLocaleUpperCase("de-DE");
	});

	async function logout(): Promise<void> {
		if (logoutPending) return;

		logoutPending = true;
		logoutError = null;

		try {
			await auth.logout();
		} catch {
			logoutError = "Die Abmeldung ist fehlgeschlagen. Bitte versuche es erneut.";
		} finally {
			logoutPending = false;
		}
	}
</script>

<div class="min-w-0">
	{#if auth.session.authenticated && auth.session.user}
		{@const user = auth.session.user}
		{@const hasAvatar = user.avatarUrl && user.avatarUrl.trim().length > 0}

		<DropdownMenu.Root>
			<DropdownMenu.Trigger
				aria-label={`Konto von ${user.displayName}`}
				class="h-11! min-h-11! min-w-0 rounded-xl! px-2! py-0!"
			>
				{#if hasAvatar}
					<img
						class="size-8 shrink-0 rounded-full object-cover"
						src={user.avatarUrl}
						alt={`Profilbild von ${user.displayName}`}
						width="32"
						height="32"
					/>
				{:else}
					<span
						class="bg-secondary text-secondary-foreground inline-flex size-8 shrink-0 items-center justify-center rounded-full text-xs font-extrabold"
						aria-hidden="true">{initials}</span
					>
				{/if}

				<span class="hidden max-w-32 truncate text-sm font-semibold sm:inline" title={user.displayName}>
					{user.displayName}
				</span>

				<ChevronDown aria-hidden="true" size={16} />
			</DropdownMenu.Trigger>

			<DropdownMenu.Content align="end" class="w-72">
				<DropdownMenu.Group aria-label="Konto">
					<DropdownMenu.GroupHeading>
						<span class="block truncate text-sm normal-case">{user.displayName}</span>
						<span class="block truncate text-xs font-medium normal-case">@{user.handle}</span>
					</DropdownMenu.GroupHeading>
					<DropdownMenu.Separator />
					<DropdownMenu.Item disabled={logoutPending} closeOnSelect={false} onselect={() => void logout()}>
						<LogOut aria-hidden="true" size={17} />
						{logoutPending ? "Abmeldung läuft …" : "Abmelden"}
					</DropdownMenu.Item>
					{#if logoutError}
						<p class="text-primary m-0 px-3 py-2 text-xs font-semibold" role="alert">{logoutError}</p>
					{/if}
				</DropdownMenu.Group>
			</DropdownMenu.Content>
		</DropdownMenu.Root>
	{:else}
		<Button onclick={() => (loginDialogVisible = true)}>
			<LogIn aria-hidden="true" size={19} />
			Anmelden
		</Button>
	{/if}
</div>

<Dialog bind:isVisible={loginDialogVisible} title="Bei GeoBingo anmelden" class="sm:max-w-md">
	<div class="grid gap-5">
		<p class="text-muted m-0 text-sm leading-relaxed sm:text-base">
			Wähle einen Anbieter aus. Anschließend kehrst du direkt zu GeoBingo zurück.
		</p>

		<div class="grid gap-3" aria-label="Anmeldeanbieter">
			{#if auth.providers.length === 0}
				<p class="border-border bg-surface-muted m-0 rounded-xl border-2 p-3 text-sm font-semibold" role="status">
					Derzeit ist kein Anmeldeanbieter verfügbar.
				</p>
			{:else}
				{#each auth.providers as provider (provider.key)}
					<Button variant="outline" href={auth.getLoginUrl(provider.key)} class="w-full justify-between!">
						<span class="inline-flex items-center gap-2">
							<LogIn aria-hidden="true" size={18} />
							Mit {provider.displayName} anmelden
						</span>
						<ArrowRight aria-hidden="true" size={18} />
					</Button>
				{/each}
			{/if}
		</div>
	</div>
</Dialog>
