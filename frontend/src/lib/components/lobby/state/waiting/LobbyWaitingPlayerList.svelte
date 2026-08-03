<script lang="ts">
	import Ban from "@lucide/svelte/icons/ban";
	import Crown from "@lucide/svelte/icons/crown";
	import Ellipsis from "@lucide/svelte/icons/ellipsis";
	import ShieldCheck from "@lucide/svelte/icons/shield-check";
	import UserRound from "@lucide/svelte/icons/user-round";
	import UserRoundX from "@lucide/svelte/icons/user-round-x";
	import Users from "@lucide/svelte/icons/users";
	import Avatar from "$lib/components/ui/Avatar.svelte";
	import Badge from "$lib/components/ui/Badge.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Dialog from "$lib/components/ui/Dialog.svelte";
	import * as DropdownMenu from "$lib/components/ui/dropdown-menu";
	import type { LobbyMemberView, LobbySnapshot } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import type { LobbyState } from "$lib/lobbies/lobby-state.svelte";
	import { getPlayerInitials } from "./lobby-waiting-presentation";

	type PlayerAction = "transfer" | "kick" | "ban";

	let {
		lobby,
		snapshot,
		actions
	}: {
		lobby: LobbyState;
		snapshot: LobbySnapshot;
		actions: LobbyActions;
	} = $props();

	let selectedPlayer = $state<LobbyMemberView | null>(null);
	let selectedAction = $state<PlayerAction | null>(null);
	let confirmationVisible = $state(false);

	const ownUserId = $derived(lobby.personalProjection?.userId);
	const uid = $props.id();
	const titleId = `${uid}-players-title`;

	const confirmationTitle = $derived.by(() => {
		if (!selectedPlayer || !selectedAction) return "Manage player";
		if (selectedAction === "transfer") return `Make ${selectedPlayer.displayName} the host?`;
		if (selectedAction === "kick") return `Kick ${selectedPlayer.displayName}?`;
		return `Ban ${selectedPlayer.displayName}?`;
	});
	const confirmationDescription = $derived.by(() => {
		if (selectedAction === "transfer") {
			return "You will give up the host role. The new host can change settings, manage players, and start the round.";
		}
		if (selectedAction === "kick") {
			return "The player will be removed from this lobby, but they can join again with the lobby code.";
		}
		return "The player will be removed and cannot rejoin this lobby.";
	});
	const confirmationButtonLabel = $derived(
		selectedAction === "transfer" ? "Transfer host" : selectedAction === "kick" ? "Kick player" : "Ban player"
	);
	const pendingOperationName = $derived(
		selectedPlayer && selectedAction
			? selectedAction === "transfer"
				? `transferHost:${selectedPlayer.userId}`
				: `${selectedAction}:${selectedPlayer.userId}`
			: ""
	);
	const confirmationPending = $derived(pendingOperationName.length > 0 && lobby.isPending(pendingOperationName));

	function requestAction(player: LobbyMemberView, action: PlayerAction): void {
		selectedPlayer = player;
		selectedAction = action;
		confirmationVisible = true;
	}

	function clearConfirmation(): void {
		confirmationVisible = false;
		selectedPlayer = null;
		selectedAction = null;
	}

	async function confirmPlayerAction(): Promise<void> {
		if (!selectedPlayer || !selectedAction || confirmationPending) return;

		if (selectedAction === "transfer") await actions.transferHost(selectedPlayer.userId);
		else if (selectedAction === "kick") await actions.kickPlayer(selectedPlayer.userId);
		else await actions.banPlayer(selectedPlayer.userId);

		if (!lobby.lastOperationError) clearConfirmation();
	}
</script>

<section
	class="border-foreground bg-surface flex min-h-0 flex-col overflow-hidden rounded-2xl border-2 shadow-[4px_4px_0_var(--foreground)]"
	aria-labelledby={titleId}
>
	<header class="border-border flex shrink-0 items-center justify-between gap-3 border-b-2 px-3 py-3 sm:px-4">
		<div class="min-w-0">
			<p class="text-primary m-0 text-[0.64rem] font-black tracking-[0.14em] uppercase">Group</p>
			<h2 id={titleId} class="mt-1 mb-0 flex flex-wrap items-center gap-2 text-lg leading-tight font-black">
				Players
				<span
					class="bg-surface-muted inline-flex min-h-6 items-center rounded-full px-2 text-xs font-black"
					aria-label={`${snapshot.members.length} of ${snapshot.settings.maxPlayers} player slots filled`}
				>
					{snapshot.members.length}/{snapshot.settings.maxPlayers}
				</span>
			</h2>
		</div>
		<Users class="text-secondary shrink-0" size={22} aria-hidden="true" />
	</header>

	<div class="min-h-0 min-[900px]:overflow-y-auto min-[900px]:overscroll-contain">
		<ul class="m-0 list-none p-0">
			{#each snapshot.members as player (player.userId)}
				{@const isHost = player.userId === snapshot.hostUserId}
				{@const isCurrentPlayer = player.userId === ownUserId}
				<li
					class="border-border hover:bg-surface-muted/45 grid min-h-[4.75rem] grid-cols-[auto_minmax(0,1fr)_auto] items-center gap-3 border-b px-3 py-2.5 last:border-b-0 sm:px-4"
				>
					<div class="relative size-11 shrink-0">
						<Avatar
							src={player.avatarUrl}
							alt=""
							class="border-foreground size-11 border-2 font-[Fredoka_Variable] text-sm font-[650]"
						>
							{#snippet fallback()}
								<span aria-hidden="true">{getPlayerInitials(player.displayName, player.handle)}</span>
							{/snippet}
						</Avatar>
						<span
							class={[
								"border-surface absolute right-0 bottom-0 size-3 rounded-full border-2",
								player.connected ? "bg-secondary" : "bg-primary"
							]}
							aria-hidden="true"
						></span>
						<span class="sr-only">{player.connected ? "Connected" : "Connection lost"}</span>
					</div>

					<div class="min-w-0">
						<div class="flex min-w-0 flex-wrap items-center gap-1.5">
							<p class="m-0 min-w-0 truncate text-sm font-black">{player.displayName}</p>
							{#if isHost}
								<Badge tone="accent" class="shrink-0 tracking-[0.04em] uppercase">
									<Crown size={12} strokeWidth={2.5} aria-hidden="true" />
									Host
								</Badge>
							{/if}
							{#if isCurrentPlayer}
								<Badge tone="secondary" class="shrink-0 tracking-[0.04em] uppercase">
									<UserRound size={12} strokeWidth={2.5} aria-hidden="true" />
									You
								</Badge>
							{/if}
						</div>
						<p class="text-muted mt-1 mb-0 truncate text-xs font-semibold">@{player.handle}</p>
					</div>

					{#if lobby.canAdminister && !isCurrentPlayer}
						<DropdownMenu.Root>
							<DropdownMenu.Trigger>
								{#snippet child({ props, open })}
									<Button
										{...props}
										variant="ghost"
										size="icon"
										class={[
											"text-foreground hover:bg-surface-muted size-10 min-h-10 border-0 p-0 shadow-none",
											open && "bg-surface-muted"
										]}
										aria-label={`Manage ${player.displayName}`}
										title={`Manage ${player.displayName}`}
									>
										<Ellipsis size={20} aria-hidden="true" />
									</Button>
								{/snippet}
							</DropdownMenu.Trigger>
							<DropdownMenu.Content align="end" class="w-56">
								<DropdownMenu.Group aria-label={`Manage ${player.displayName}`}>
									<DropdownMenu.GroupHeading class="normal-case">
										<span class="text-foreground block truncate text-sm tracking-normal">{player.displayName}</span>
									</DropdownMenu.GroupHeading>
									<DropdownMenu.Item onselect={() => requestAction(player, "transfer")}>
										<ShieldCheck size={17} aria-hidden="true" />
										Transfer host
									</DropdownMenu.Item>
									<DropdownMenu.Separator />
									<DropdownMenu.Item class="text-primary" onselect={() => requestAction(player, "kick")}>
										<UserRoundX size={17} aria-hidden="true" />
										Kick player
									</DropdownMenu.Item>
									<DropdownMenu.Item class="text-primary" onselect={() => requestAction(player, "ban")}>
										<Ban size={17} aria-hidden="true" />
										Ban player
									</DropdownMenu.Item>
								</DropdownMenu.Group>
							</DropdownMenu.Content>
						</DropdownMenu.Root>
					{/if}
				</li>
			{/each}
		</ul>
	</div>
</section>

<Dialog
	bind:isVisible={confirmationVisible}
	title={confirmationTitle}
	showActions={false}
	onclose={() => {
		selectedPlayer = null;
		selectedAction = null;
	}}
>
	<p class="text-muted m-0">{confirmationDescription}</p>
	<div class="mt-5 flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
		<Button variant="outline" onclick={clearConfirmation} disabled={confirmationPending}>Cancel</Button>
		<Button
			variant={selectedAction === "transfer" ? "primary" : "destructive"}
			onclick={() => void confirmPlayerAction()}
			disabled={confirmationPending}
		>
			{confirmationPending ? "Working…" : confirmationButtonLabel}
		</Button>
	</div>
</Dialog>
