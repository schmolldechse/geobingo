<script lang="ts">
	import Check from "@lucide/svelte/icons/check";
	import Gauge from "@lucide/svelte/icons/gauge";
	import Users from "@lucide/svelte/icons/users";
	import { onMount } from "svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Dialog from "$lib/components/ui/Dialog.svelte";
	import Input, { type InputValue } from "$lib/components/ui/Input.svelte";
	import { toast } from "$lib/components/ui/toast";
	import type { GameModeSummary } from "$lib/generated/api";
	import type { LobbySnapshot } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import type { LobbyState } from "$lib/lobbies/lobby-state.svelte";
	import LobbyGameModeSettings from "./LobbyGameModeSettings.svelte";
	import { mergeGameModeCatalog, SETTINGS_UPDATE_DEBOUNCE_MS } from "./lobby-waiting-settings";

	type Props = {
		lobby: LobbyState;
		snapshot: LobbySnapshot;
		actions: LobbyActions;
		availableGameModes: GameModeSummary[];
		gameModeCatalogAvailable: boolean;
	};

	let { lobby, snapshot, actions, availableGameModes, gameModeCatalogAvailable }: Props = $props();

	const uid = $props.id();
	const lobbySizeId = `${uid}-lobby-size`;
	const modeSelectId = `${uid}-game-mode`;

	let maxPlayersDraft = $derived<InputValue>(snapshot.settings.maxPlayers);
	let selectedModeKey = $derived(snapshot.selectedMode.key);
	let requestedModeKey = $state<string | null>(null);
	let modeDialogVisible = $state(false);

	const minimumLobbySize = $derived(Math.max(2, snapshot.members.length));
	const gameModes = $derived(mergeGameModeCatalog(snapshot.selectedMode, availableGameModes));
	const requestedMode = $derived(gameModes.find((mode) => mode.key === requestedModeKey) ?? null);
	const lobbySettingsPending = $derived(lobby.isPending("updateLobbySettings"));
	const modeSelectionPending = $derived(lobby.isPending("selectGameMode"));
	const modeSelectionDisabled = $derived(
		!lobby.canAdminister || !gameModeCatalogAvailable || gameModes.length < 2 || modeSelectionPending
	);

	onMount(() => {
		if (gameModeCatalogAvailable) return;
		toast.warning("Game modes unavailable", {
			description: "The current mode is still available, but switching modes is temporarily disabled.",
			duration: 8000
		});
	});

	async function updateLobbySize(value: InputValue): Promise<void> {
		const maxPlayers = Number(value);
		if (!Number.isFinite(maxPlayers) || maxPlayers === snapshot.settings.maxPlayers) return;

		await actions.updateLobbySettings({ maxPlayers });
		if (lobby.lastOperationError) maxPlayersDraft = snapshot.settings.maxPlayers;
	}

	function requestModeChange(event: Event): void {
		const modeKey = (event.currentTarget as HTMLSelectElement).value;
		if (modeKey === snapshot.selectedMode.key) return;

		requestedModeKey = modeKey;
		modeDialogVisible = true;
	}

	function cancelModeChange(): void {
		requestedModeKey = null;
		selectedModeKey = snapshot.selectedMode.key;
		modeDialogVisible = false;
	}

	async function confirmModeChange(): Promise<void> {
		if (!requestedModeKey) return;
		const modeKey = requestedModeKey;
		await actions.selectGameMode(modeKey, true);

		if (lobby.lastOperationError) {
			cancelModeChange();
			return;
		}

		selectedModeKey = modeKey;
		requestedModeKey = null;
		modeDialogVisible = false;
	}
</script>

<div
	class="grid min-h-0 gap-2.5 min-[900px]:h-full min-[900px]:grid-cols-[minmax(17rem,0.72fr)_minmax(32rem,1.58fr)] min-[900px]:overflow-hidden"
	data-lobby-waiting-settings
>
	<div class="grid min-h-0 content-start gap-2.5 min-[900px]:overflow-y-auto min-[900px]:pr-1">
		<section
			class="border-foreground bg-surface rounded-2xl border-2 shadow-[var(--shadow-paper)]"
			aria-labelledby={`${lobbySizeId}-title`}
		>
			<header class="border-border flex items-start justify-between gap-3 border-b-2 p-4">
				<div class="min-w-0">
					<p class="text-primary m-0 text-[0.65rem] font-black tracking-[0.15em] uppercase">Lobby</p>
					<h2 id={`${lobbySizeId}-title`} class="mt-1 mb-0 text-base font-black tracking-tight">Lobby settings</h2>
					<p class="text-muted mt-1 mb-0 text-xs leading-5">Controls that apply independently of the game mode.</p>
				</div>
				<span class="bg-secondary/15 text-secondary grid size-10 shrink-0 place-items-center rounded-xl">
					<Users size={20} aria-hidden="true" />
				</span>
			</header>

			<div class="p-4">
				<div class="flex items-start justify-between gap-3">
					<label for={lobbySizeId} class="text-sm font-extrabold">
						Lobby size
						<span class="text-muted mt-0.5 block text-xs leading-5 font-semibold">
							Maximum number of players who can join.
						</span>
					</label>
					<output
						for={lobbySizeId}
						class="border-foreground bg-accent text-accent-foreground shrink-0 rounded-full border-2 px-2.5 py-1 font-[Fredoka_Variable] text-xs font-semibold"
					>
						{Number(maxPlayersDraft)} players
					</output>
				</div>
				<Input
					id={lobbySizeId}
					type="range"
					bind:value={maxPlayersDraft}
					min={minimumLobbySize}
					max={32}
					step={1}
					rangeMinLabel={`${minimumLobbySize}`}
					rangeMaxLabel="32 players"
					debounceTime={SETTINGS_UPDATE_DEBOUNCE_MS}
					onchange={updateLobbySize}
					disabled={!lobby.canAdminister || lobbySettingsPending}
					aria-label="Maximum lobby size"
					class="mt-2"
				/>
			</div>
		</section>

		<section
			class="border-foreground bg-surface rounded-2xl border-2 shadow-[var(--shadow-paper)]"
			aria-labelledby={`${modeSelectId}-title`}
		>
			<header class="border-border flex items-start justify-between gap-3 border-b-2 p-4">
				<div class="min-w-0">
					<p class="text-primary m-0 text-[0.65rem] font-black tracking-[0.15em] uppercase">Round</p>
					<h2 id={`${modeSelectId}-title`} class="mt-1 mb-0 text-base font-black tracking-tight">Game mode</h2>
					<p class="text-muted mt-1 mb-0 text-xs leading-5">The selected mode owns its settings and goals.</p>
				</div>
				<span class="bg-secondary/15 text-secondary grid size-10 shrink-0 place-items-center rounded-xl">
					<Gauge size={20} aria-hidden="true" />
				</span>
			</header>

			<div class="p-4">
				<label for={modeSelectId} class="text-sm font-extrabold">Selected mode</label>
				<select
					id={modeSelectId}
					bind:value={selectedModeKey}
					onchange={requestModeChange}
					disabled={modeSelectionDisabled}
					class="border-border bg-surface text-foreground focus:border-secondary mt-2 min-h-11 w-full rounded-xl border-2 px-3 py-2 text-sm font-extrabold outline-none disabled:cursor-not-allowed disabled:opacity-50"
				>
					{#each gameModes as mode (mode.key)}
						<option value={mode.key}>{mode.displayName}</option>
					{/each}
				</select>

				<div class="bg-secondary/10 text-secondary mt-3 flex items-start gap-2 rounded-xl p-3 text-xs leading-5 font-bold">
					<Check class="mt-0.5 shrink-0" size={16} aria-hidden="true" />
					<span>
						Active mode · {snapshot.selectedMode.displayName}
						{#if !gameModeCatalogAvailable}
							· Switching is temporarily unavailable
						{/if}
					</span>
				</div>
			</div>
		</section>
	</div>

	<LobbyGameModeSettings {lobby} {snapshot} {actions} />
</div>

<Dialog bind:isVisible={modeDialogVisible} title="Change game mode?" showActions={false} onclose={cancelModeChange}>
	<p class="text-muted m-0 leading-6">
		Switching to <strong class="text-foreground">{requestedMode?.displayName ?? "this mode"}</strong> resets the current game-mode
		settings and goals. This action cannot be undone.
	</p>
	<div class="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
		<Button variant="outline" onclick={cancelModeChange}>Cancel</Button>
		<Button onclick={confirmModeChange} disabled={modeSelectionPending}>
			{modeSelectionPending ? "Changing..." : "Change mode"}
		</Button>
	</div>
</Dialog>
