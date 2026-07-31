<script lang="ts">
	import ChartNoAxesCombined from "@lucide/svelte/icons/chart-no-axes-combined";
	import Check from "@lucide/svelte/icons/check";
	import Copy from "@lucide/svelte/icons/copy";
	import DoorOpen from "@lucide/svelte/icons/door-open";
	import Settings from "@lucide/svelte/icons/settings";
	import Share2 from "@lucide/svelte/icons/share-2";
	import Trash2 from "@lucide/svelte/icons/trash-2";
	import Users from "@lucide/svelte/icons/users";
	import { onDestroy } from "svelte";
	import BrandMark from "$lib/components/BrandMark.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Dialog from "$lib/components/ui/Dialog.svelte";
	import * as Tabs from "$lib/components/ui/tabs";
	import { toast } from "$lib/components/ui/toast";
	import type { GameModeSummary } from "$lib/generated/api";
	import type { LobbySnapshot } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import { setLobbyContext } from "$lib/lobbies/lobby-context.svelte";
	import type { LobbyState } from "$lib/lobbies/lobby-state.svelte";
	import type { ResultState } from "$lib/lobbies/result-state.svelte";
	import LobbyModeSummary from "./LobbyModeSummary.svelte";
	import LobbyResultsPanel from "./LobbyResultsPanel.svelte";
	import LobbyStartCommandBar from "./LobbyStartCommandBar.svelte";
	import LobbyWaitingPlayerList from "./LobbyWaitingPlayerList.svelte";
	import LobbyWaitingSettings from "./LobbyWaitingSettings.svelte";
	import { getLobbyModeSummary } from "./lobby-waiting-presentation";

	type LobbyTab = "lobby" | "settings" | "results";
	type InviteFeedback = "idle" | "code-copied" | "link-copied" | "shared";

	let {
		lobby,
		snapshot,
		results,
		actions,
		availableGameModes,
		gameModeCatalogAvailable
	}: {
		lobby: LobbyState;
		snapshot: LobbySnapshot;
		results: ResultState;
		actions: LobbyActions;
		availableGameModes: GameModeSummary[];
		gameModeCatalogAvailable: boolean;
	} = $props();

	let activeTab = $state<LobbyTab>("lobby");
	let inviteFeedback = $state<InviteFeedback>("idle");
	let leaveDialogVisible = $state(false);
	let closeDialogVisible = $state(false);
	let feedbackTimeout: number | undefined;

	const resultsCount = $derived(snapshot.completedRoundSummaries.length);
	const modeSummary = $derived(getLobbyModeSummary(snapshot));
	const roundStartIssueMessages = $derived(
		(snapshot.gameMode.captureChallenge?.roundStartIssues ?? []).map((issue) => issue.message)
	);
	const feedbackAnnouncement = $derived(
		inviteFeedback === "code-copied"
			? "Lobby code copied."
			: inviteFeedback === "link-copied"
				? "Lobby link copied."
				: inviteFeedback === "shared"
					? "Lobby shared."
					: ""
	);

	setLobbyContext({
		get lobby() {
			return lobby;
		},
		get results() {
			return results;
		},
		get actions() {
			return actions;
		}
	});

	function showInviteFeedback(feedback: Exclude<InviteFeedback, "idle">): void {
		inviteFeedback = feedback;
		if (feedbackTimeout !== undefined) window.clearTimeout(feedbackTimeout);
		feedbackTimeout = window.setTimeout(() => {
			inviteFeedback = "idle";
			feedbackTimeout = undefined;
		}, 2000);
	}

	async function writeToClipboard(value: string): Promise<void> {
		if (!navigator.clipboard) throw new Error("Clipboard access is unavailable.");
		await navigator.clipboard.writeText(value);
	}

	async function copyLobbyCode(): Promise<void> {
		try {
			await writeToClipboard(snapshot.code);
			showInviteFeedback("code-copied");
		} catch {
			toast.error("Could not copy the lobby code", {
				description: "Copy the code manually and try again."
			});
		}
	}

	async function shareLobby(): Promise<void> {
		const lobbyUrl = window.location.href;

		if (!navigator.share) {
			try {
				await writeToClipboard(lobbyUrl);
				showInviteFeedback("link-copied");
			} catch {
				toast.error("Could not copy the lobby link", {
					description: "Copy the address from your browser and share it manually."
				});
			}
			return;
		}

		try {
			await navigator.share({
				title: "Join my GeoBingo lobby",
				text: `Join my GeoBingo lobby with code ${snapshot.code}.`,
				url: lobbyUrl
			});
			showInviteFeedback("shared");
		} catch (error) {
			if (error instanceof DOMException && error.name === "AbortError") return;
			toast.error("Could not share the lobby", {
				description: "Please try again or copy the lobby code."
			});
		}
	}

	async function leaveLobby(): Promise<void> {
		await actions.leaveLobby();
		if (!lobby.lastOperationError) leaveDialogVisible = false;
	}

	async function closeLobby(): Promise<void> {
		await actions.closeLobby();
		if (!lobby.lastOperationError) closeDialogVisible = false;
	}

	onDestroy(() => {
		if (feedbackTimeout !== undefined) window.clearTimeout(feedbackTimeout);
	});
</script>

<main
	class="bg-background text-foreground relative min-h-dvh overflow-x-hidden px-2.5 py-2.5 min-[900px]:h-dvh min-[900px]:min-h-0 min-[900px]:overflow-hidden sm:px-4 sm:py-3"
	data-lobby-waiting-state
	data-active-tab={activeTab}
>
	<div
		class="border-accent pointer-events-none absolute -top-48 -right-36 size-[27rem] rounded-full border-[3.5rem] opacity-15"
		aria-hidden="true"
	></div>
	<div
		class="border-secondary pointer-events-none absolute -bottom-48 -left-44 size-[25rem] rotate-[-18deg] rounded-[48%_52%_67%_33%/35%_37%_63%_65%] border-2 opacity-20"
		aria-hidden="true"
	></div>

	<div
		class="relative z-10 mx-auto flex min-h-[calc(100dvh-1.25rem)] w-full max-w-[100rem] flex-col gap-2.5 min-[900px]:h-[calc(100dvh-1.5rem)] min-[900px]:min-h-0 sm:min-h-[calc(100dvh-1.5rem)]"
	>
		<header class="flex flex-col gap-2.5">
			<div class="flex items-center justify-between gap-2">
				<a href="/" class="text-foreground self-start rounded-lg no-underline" aria-label="Go to the GeoBingo homepage">
					<BrandMark />
				</a>

				<div class="flex min-w-0 items-center justify-end gap-2">
					<Button
						variant="ghost"
						class="border-border text-foreground hover:border-foreground min-w-0 border-2 max-sm:size-10 max-sm:min-h-10 max-sm:gap-1.5 max-sm:px-0 max-sm:text-xs max-sm:whitespace-nowrap"
						onclick={() => (leaveDialogVisible = true)}
					>
						<DoorOpen size={18} aria-hidden="true" />
						<span class="max-sm:sr-only">Leave lobby</span>
					</Button>
					{#if lobby.isHost}
						<Button
							variant="destructive"
							class="min-w-0 shadow-none max-sm:size-10 max-sm:min-h-10 max-sm:gap-1.5 max-sm:px-0 max-sm:text-xs max-sm:whitespace-nowrap"
							onclick={() => (closeDialogVisible = true)}
						>
							<Trash2 size={18} aria-hidden="true" />
							<span class="max-sm:sr-only">Close lobby</span>
						</Button>
					{/if}
				</div>
			</div>

			<section
				class="border-foreground bg-secondary text-secondary-foreground relative grid overflow-hidden rounded-2xl border-2 p-3.5 shadow-[4px_4px_0_var(--foreground)] min-[480px]:grid-cols-[minmax(0,1fr)_auto] min-[480px]:items-center min-[480px]:gap-4 sm:px-4"
				aria-labelledby="lobby-code-title"
			>
				<div
					class="border-secondary-foreground pointer-events-none absolute -top-14 right-[22%] size-40 rounded-full border-[1.5rem] opacity-[0.08]"
					aria-hidden="true"
				></div>

				<div class="relative z-10 flex min-w-0 flex-col items-start gap-2.5">
					<p id="lobby-code-title" class="m-0 text-[0.64rem] font-black tracking-[0.14em] uppercase opacity-70">Lobby code</p>
					<p
						class="m-0 mt-0.5 overflow-hidden font-[Fredoka_Variable] text-[clamp(2rem,12vw,2.8rem)] leading-none font-[650] tracking-[0.14em] text-ellipsis whitespace-nowrap sm:text-[clamp(1.75rem,3vw,2.55rem)] sm:tracking-[0.17em]"
						aria-label={snapshot.code.split("").join(" ")}
					>
						{snapshot.code}
					</p>
				</div>

				<div class="relative z-10 mt-3 flex items-center justify-end gap-2 min-[480px]:mt-0">
					<Button
						variant="accent"
						class="min-w-0 px-3 max-sm:size-11 max-sm:px-0"
						aria-label="Copy code"
						title="Copy code"
						onclick={copyLobbyCode}
					>
						{#if inviteFeedback === "code-copied"}
							<Check size={18} aria-hidden="true" />
						{:else}
							<Copy size={18} aria-hidden="true" />
						{/if}
						<span class="max-sm:sr-only">Copy code</span>
					</Button>
					<Button
						variant="ghost"
						class="border-secondary-foreground/40 text-secondary-foreground hover:border-secondary-foreground hover:bg-secondary-foreground/10 hover:text-secondary-foreground min-w-0 border-2 bg-transparent px-3 max-sm:size-11 max-sm:px-0"
						aria-label="Share lobby"
						title="Share lobby"
						onclick={shareLobby}
					>
						{#if inviteFeedback === "shared" || inviteFeedback === "link-copied"}
							<Check size={18} aria-hidden="true" />
						{:else}
							<Share2 size={18} aria-hidden="true" />
						{/if}
						<span class="max-sm:sr-only">Share lobby</span>
					</Button>
				</div>
			</section>
		</header>

		<Tabs.Root
			bind:value={activeTab}
			aria-label="Lobby views"
			class="flex min-h-0 flex-1 flex-col pb-32 min-[360px]:pb-24 min-[900px]:pb-0"
		>
			<Tabs.List>
				<Tabs.Trigger value="lobby" class="max-sm:flex-1 max-sm:px-2 [&_svg]:max-sm:hidden">
					<Users size={18} aria-hidden="true" />
					Lobby
				</Tabs.Trigger>
				<Tabs.Trigger value="settings" class="max-sm:flex-1 max-sm:px-2 [&_svg]:max-sm:hidden">
					<Settings size={18} aria-hidden="true" />
					Settings
				</Tabs.Trigger>
				<Tabs.Trigger value="results" class="max-sm:flex-1 max-sm:px-2 [&_svg]:max-sm:hidden">
					<ChartNoAxesCombined size={18} aria-hidden="true" />
					Results
					<span
						class="bg-surface-muted text-foreground inline-flex h-5 min-w-5 items-center justify-center rounded-full px-1.5 text-[0.65rem] font-black"
					>
						{resultsCount}
					</span>
				</Tabs.Trigger>
			</Tabs.List>

			<Tabs.Content value="lobby" class="min-h-0 flex-1 min-[900px]:overflow-hidden">
				<div
					class="grid min-h-0 gap-2.5 min-[900px]:h-full min-[900px]:grid-cols-[minmax(0,2fr)_minmax(18rem,1fr)] min-[900px]:overflow-hidden"
				>
					<LobbyWaitingPlayerList {lobby} {snapshot} {actions} />
					<LobbyModeSummary summary={modeSummary} />
				</div>
			</Tabs.Content>
			<Tabs.Content value="settings" class="min-h-0 flex-1 min-[900px]:overflow-hidden">
				<LobbyWaitingSettings {lobby} {snapshot} {actions} {availableGameModes} {gameModeCatalogAvailable} />
			</Tabs.Content>
			<Tabs.Content value="results" class="min-h-0 flex-1 min-[900px]:overflow-hidden">
				<LobbyResultsPanel {snapshot} {results} {actions} />
			</Tabs.Content>
		</Tabs.Root>

		<LobbyStartCommandBar {lobby} {actions} issueMessages={roundStartIssueMessages} />

		<p class="sr-only" aria-live="polite">{feedbackAnnouncement}</p>
	</div>
</main>

<Dialog bind:isVisible={leaveDialogVisible} title="Leave this lobby?" showActions={false}>
	<p class="text-muted m-0">
		{#if lobby.isHost}
			Your host role will be transferred to the next available player before you leave.
		{:else}
			You will leave the current lobby and return to the homepage.
		{/if}
	</p>
	<div class="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
		<Button variant="outline" onclick={() => (leaveDialogVisible = false)}>Cancel</Button>
		<Button variant="destructive" onclick={leaveLobby} disabled={lobby.isPending("leaveLobby")}>
			{lobby.isPending("leaveLobby") ? "Leaving..." : "Leave lobby"}
		</Button>
	</div>
</Dialog>

{#if lobby.isHost}
	<Dialog bind:isVisible={closeDialogVisible} title="Close this lobby?" showActions={false}>
		<p class="text-muted m-0">
			Every player will be disconnected and the lobby code will stop working immediately. This action cannot be undone.
		</p>
		<div class="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
			<Button variant="outline" onclick={() => (closeDialogVisible = false)}>Cancel</Button>
			<Button variant="destructive" onclick={closeLobby} disabled={lobby.isPending("closeLobby")}>
				{lobby.isPending("closeLobby") ? "Closing..." : "Close lobby"}
			</Button>
		</div>
	</Dialog>
{/if}
