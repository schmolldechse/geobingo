<script lang="ts">
	import { goto } from "$app/navigation";
	import ErrorScreen from "$lib/components/ErrorScreen.svelte";
	import LobbyLoadingScreen from "$lib/components/lobby/LobbyLoadingScreen.svelte";
	import LobbyPreparingState from "$lib/components/lobby/state/preparing/LobbyPreparingState.svelte";
	import LobbyWaitingState from "$lib/components/lobby/state/waiting/LobbyWaitingState.svelte";
	import { LobbyStatus } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
	import { LobbyActions } from "$lib/lobbies/lobby-actions";
	import { getLobbyConclusionPresentation } from "$lib/lobbies/lobby-conclusion";
	import { LobbyState } from "$lib/lobbies/lobby-state.svelte";
	import { ResultState } from "$lib/lobbies/result-state.svelte";
	import { GameConnection } from "$lib/realtime/game-connection.svelte";
	import { onMount } from "svelte";
	import type { PageProps } from "./$types";

	let { data }: PageProps = $props();

	const results = new ResultState();
	const lobby = new LobbyState(results);

	let actions: LobbyActions | null = $state(null);

	const conclusion = $derived(lobby.conclusion ? getLobbyConclusionPresentation(lobby.conclusion) : null);

	$effect(() => {
		if (conclusion?.kind === "redirect") {
			void goto(conclusion.href, { replaceState: true });
		}
	});

	onMount(() => {
		const connection = new GameConnection(data.code, lobby, results);
		actions = new LobbyActions(connection, lobby, results);
		void connection.start();

		return () => {
			void connection.stop();
		};
	});
</script>

<svelte:head>
	<title>Lobby {data.code} - GeoBingo</title>
	<meta name="description" content="Multiplayer Street View adventures" />
</svelte:head>

{#if conclusion?.kind === "screen"}
	<ErrorScreen status={conclusion.status} message={conclusion.message} actionLabel="Back to home" actionHref="/" />
{:else if conclusion?.kind === "redirect"}
	<p class="sr-only" aria-live="polite">Returning to the homepage.</p>
{:else if actions && lobby.snapshot}
	{#if lobby.snapshot.status === LobbyStatus.WAITING}
		<LobbyWaitingState
			{lobby}
			snapshot={lobby.snapshot}
			{results}
			{actions}
			availableGameModes={data.availableGameModes}
			gameModeCatalogAvailable={data.gameModeCatalogAvailable}
		/>
	{:else if lobby.snapshot.status === LobbyStatus.PREPARING}
		<LobbyPreparingState snapshot={lobby.snapshot} />
	{:else if lobby.snapshot.status === LobbyStatus.PLAYING}
		<p>{lobby.snapshot.status}</p>
	{/if}
{:else}
	<LobbyLoadingScreen code={data.code} connectionStatus={lobby.connectionStatus} />
{/if}
