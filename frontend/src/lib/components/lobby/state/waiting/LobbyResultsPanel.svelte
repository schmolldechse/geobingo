<script lang="ts">
	import Camera from "@lucide/svelte/icons/camera";
	import ChartNoAxesCombined from "@lucide/svelte/icons/chart-no-axes-combined";
	import Crown from "@lucide/svelte/icons/crown";
	import Medal from "@lucide/svelte/icons/medal";
	import ThumbsDown from "@lucide/svelte/icons/thumbs-down";
	import ThumbsUp from "@lucide/svelte/icons/thumbs-up";
	import Trophy from "@lucide/svelte/icons/trophy";
	import Avatar from "$lib/components/ui/Avatar.svelte";
	import Badge from "$lib/components/ui/Badge.svelte";
	import {
		type CompletedRoundResults,
		type CumulativePlayerResult,
		type LobbySnapshot,
		type PlayerRoundResult
	} from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import type { ResultState } from "$lib/lobbies/result-state.svelte";
	import { getRoundSelection, isRoundSelected } from "./lobby-results-navigation";
	import { getPlayerInitials } from "./lobby-waiting-presentation";

	type ResultPlayer = PlayerRoundResult | CumulativePlayerResult;

	let {
		snapshot,
		results,
		actions
	}: {
		snapshot: LobbySnapshot;
		results: ResultState;
		actions: LobbyActions;
	} = $props();
	const historicalView = $derived.by(() => {
		if (results.selection.kind !== "round") return null;
		return results.historicalRounds.get(results.selection.roundId) ?? null;
	});
	const roundResults = $derived.by((): CompletedRoundResults | null => {
		if (results.selection.kind === "latest") return snapshot.lastCompletedRoundResults ?? null;
		if (results.selection.kind === "round") return historicalView?.results ?? null;
		return null;
	});
	const leaderboard = $derived<ResultPlayer[]>(
		results.selection.kind === "cumulative" ? snapshot.cumulativeResults : (roundResults?.players ?? [])
	);
	const captureResults = $derived(roundResults?.captures ?? []);
	const completedRounds = $derived(
		[...snapshot.completedRoundSummaries].sort((left, right) => right.roundNumber - left.roundNumber)
	);
	const selectedRoundNumber = $derived.by(() => {
		if (roundResults) return roundResults.roundNumber;
		const selection = results.selection;
		if (selection.kind !== "round") return null;
		return completedRounds.find((round) => round.roundId === selection.roundId)?.roundNumber ?? null;
	});
	const historicalLoading = $derived(results.selection.kind === "round" && historicalView === null);

	function playerName(player: ResultPlayer | null): string {
		return player?.displayName.trim() || "Former player";
	}

	function playerHandle(player: ResultPlayer): string {
		return player.handle.trim().replace(/^@/, "");
	}

	function captureOwner(userId: string): PlayerRoundResult | null {
		return roundResults?.players.find((player) => player.userId === userId) ?? null;
	}

	function goalTitle(goalId: string): string {
		return snapshot.gameMode.captureChallenge?.goals.find((goal) => goal.goalId === goalId)?.title ?? "Capture goal";
	}

	function formatScore(score: number): string {
		return new Intl.NumberFormat("en", { minimumFractionDigits: 0, maximumFractionDigits: 2 }).format(score);
	}
</script>

{#snippet playerAvatar(player: ResultPlayer | null, compact: boolean)}
	<Avatar
		src={player?.avatarUrl}
		alt=""
		class={[
			"border-foreground border-2 font-[Fredoka_Variable] font-[650]",
			compact ? "size-8 text-[0.68rem]" : "size-10 text-xs sm:size-11 sm:text-sm"
		]}
	>
		{#snippet fallback()}
			<span aria-hidden="true">{player ? getPlayerInitials(player.displayName, player.handle) : "?"}</span>
		{/snippet}
	</Avatar>
{/snippet}

<section
	class="grid min-h-0 gap-2.5 min-[900px]:h-full min-[900px]:grid-cols-[minmax(15rem,0.72fr)_minmax(0,2fr)] min-[900px]:overflow-hidden"
	aria-labelledby="results-heading"
	data-lobby-results-panel
>
	<aside
		class="border-border bg-surface grid min-h-0 content-start gap-3 rounded-2xl border-2 p-3.5 min-[900px]:overflow-y-auto"
	>
		<div>
			<p class="text-secondary m-0 text-[0.62rem] font-black tracking-[0.14em] uppercase">Scoreboard</p>
			<h2 id="results-heading" class="mt-1 mb-0 font-[Fredoka_Variable] text-2xl font-[650]">Results</h2>
			<p class="text-muted mt-1 mb-0 text-sm">Review the overall standings or a completed round.</p>
		</div>

		<nav class="grid gap-3" aria-label="Result views">
			<button
				type="button"
				class={[
					"flex min-h-11 w-full cursor-pointer items-center gap-3 rounded-xl border-2 px-3 py-2.5 text-left text-sm font-black transition-[transform,background-color,color,border-color,box-shadow] motion-reduce:transition-none",
					results.selection.kind === "cumulative"
						? "border-foreground bg-secondary text-secondary-foreground shadow-[2px_2px_0_var(--foreground)]"
						: "border-border bg-surface hover:border-foreground hover:bg-secondary/10"
				]}
				aria-current={results.selection.kind === "cumulative" ? "page" : undefined}
				onclick={() => void actions.selectResults({ kind: "cumulative" })}
			>
				<span
					class={[
						"grid size-8 shrink-0 place-items-center rounded-lg",
						results.selection.kind === "cumulative"
							? "bg-secondary-foreground/15 text-secondary-foreground"
							: "bg-secondary/15 text-secondary"
					]}
					aria-hidden="true"
				>
					<ChartNoAxesCombined size={18} />
				</span>
				<span class="min-w-0 flex-1">Overall standings</span>
			</button>

			{#if completedRounds.length > 0}
				<div class="border-border grid gap-2 border-t-2 border-dashed pt-3">
					<p class="text-muted m-0 text-[0.62rem] font-black tracking-[0.12em] uppercase">Completed rounds</p>
					<div class="grid gap-2">
						{#each completedRounds as round, index (round.roundId)}
							{@const isLatest = index === 0}
							{@const selected = isRoundSelected(results.selection, round.roundId, isLatest)}
							<button
								type="button"
								class={[
									"flex min-h-11 w-full cursor-pointer items-center gap-3 rounded-xl border-2 px-3 py-2.5 text-left text-sm font-black transition-[transform,background-color,color,border-color,box-shadow] motion-reduce:transition-none",
									selected
										? "border-foreground bg-secondary text-secondary-foreground shadow-[2px_2px_0_var(--foreground)]"
										: "border-border bg-surface hover:border-foreground hover:bg-secondary/10"
								]}
								aria-current={selected ? "page" : undefined}
								onclick={() => void actions.selectResults(getRoundSelection(round.roundId, isLatest))}
							>
								<span
									class={[
										"grid size-8 shrink-0 place-items-center rounded-lg font-[Fredoka_Variable] text-sm font-[650]",
										selected ? "bg-secondary-foreground/15 text-secondary-foreground" : "bg-primary/10 text-primary"
									]}
									aria-hidden="true"
								>
									{round.roundNumber}
								</span>
								<span class="min-w-0 flex-1">Round {round.roundNumber}</span>
								{#if isLatest}
									<Badge tone="accent" text="Latest" class="shrink-0" />
								{/if}
							</button>
						{/each}
					</div>
				</div>
			{/if}
		</nav>
	</aside>

	<div class="border-border bg-surface min-h-0 rounded-2xl border-2 p-3.5 min-[900px]:overflow-y-auto sm:p-5">
		{#if completedRounds.length === 0}
			<div class="grid min-h-64 place-items-center text-center">
				<div class="grid max-w-sm justify-items-center gap-3">
					<Trophy class="text-secondary" size={42} aria-hidden="true" />
					<h3 class="m-0 font-[Fredoka_Variable] text-2xl font-[650]">No results yet</h3>
					<p class="text-muted m-0 text-sm leading-6">Complete a Capture Challenge round to create the first scoreboard.</p>
				</div>
			</div>
		{:else}
			<header class="mb-4 flex flex-wrap items-end justify-between gap-3">
				<div>
					<p class="text-secondary m-0 text-[0.62rem] font-black tracking-[0.14em] uppercase">
						{selectedRoundNumber === null ? "All completed rounds" : `Round ${selectedRoundNumber}`}
					</p>
					<h3 class="mt-1 mb-0 font-[Fredoka_Variable] text-2xl font-[650]">
						{selectedRoundNumber === null ? "Overall standings" : "Round standings"}
					</h3>
				</div>
				{#if roundResults?.completedAt}
					<time class="text-muted text-xs font-bold" datetime={String(roundResults.completedAt)}>
						Completed {new Date(roundResults.completedAt).toLocaleString("en")}
					</time>
				{/if}
			</header>

			{#if historicalLoading}
				<div class="text-muted grid min-h-48 place-items-center text-sm font-bold" role="status">Loading round results…</div>
			{:else}
				<div class="grid gap-2" aria-label="Player rankings">
					{#each leaderboard as player (player.userId)}
						<div
							class="border-border bg-surface-muted grid grid-cols-[auto_auto_minmax(0,1fr)_auto] items-center gap-2 rounded-xl border-2 px-2.5 py-2.5 sm:gap-3 sm:px-3"
							class:border-accent={player.rank === 1}
							class:bg-accent={player.rank === 1}
						>
							<div
								class="border-foreground bg-surface grid size-8 place-items-center rounded-full border-2 font-[Fredoka_Variable] text-xs font-[650] sm:size-9 sm:text-sm"
							>
								{#if player.rank === 1}<Crown size={18} aria-label="First place" />{:else}{player.rank}{/if}
							</div>
							{@render playerAvatar(player, false)}
							<div class="min-w-0">
								<p class="m-0 overflow-hidden font-[Fredoka_Variable] font-[650] text-ellipsis whitespace-nowrap">
									{playerName(player)}
								</p>
								<p class="text-muted m-0 overflow-hidden text-[0.68rem] font-bold text-ellipsis whitespace-nowrap">
									@{playerHandle(player)} · Rank {player.rank}
								</p>
							</div>
							<p class="m-0 font-[Fredoka_Variable] text-base font-[650] whitespace-nowrap tabular-nums sm:text-xl">
								{formatScore(player.score)} <span class="text-muted text-[0.62rem] font-black uppercase">pts</span>
							</p>
						</div>
					{/each}
				</div>

				{#if captureResults.length > 0}
					<section class="border-border mt-5 border-t-2 border-dashed pt-4" aria-labelledby="capture-results-heading">
						<div class="mb-3 flex items-center gap-2">
							<Camera class="text-primary" size={19} aria-hidden="true" />
							<h4 id="capture-results-heading" class="m-0 font-[Fredoka_Variable] text-lg font-[650]">Capture breakdown</h4>
						</div>
						<div class="grid gap-2 sm:grid-cols-2">
							{#each captureResults as result (result.captureId)}
								{@const owner = captureOwner(result.ownerUserId)}
								<article class="border-border bg-surface-muted grid gap-2 rounded-xl border-2 p-3">
									<div class="flex items-start justify-between gap-3">
										<div class="flex min-w-0 items-start gap-2.5">
											{@render playerAvatar(owner, true)}
											<div class="min-w-0">
												<p class="text-secondary m-0 truncate text-sm font-black">{playerName(owner)}</p>
												<h5 class="mt-0.5 mb-0 line-clamp-2 font-[Fredoka_Variable] font-[650]">
													{goalTitle(result.goalId)}
												</h5>
											</div>
										</div>
										<Badge tone="accent" text={`${formatScore(result.score)} pts`} class="shrink-0 text-xs!" />
									</div>
									<div class="text-muted flex flex-wrap items-center gap-3 text-xs font-black">
										<span class="inline-flex items-center gap-1"><ThumbsUp size={14} aria-hidden="true" /> {result.good}</span>
										<span class="inline-flex items-center gap-1"><ThumbsDown size={14} aria-hidden="true" /> {result.bad}</span>
										<span class="inline-flex items-center gap-1"
											><Medal size={14} aria-hidden="true" /> {result.eligible} eligible</span
										>
									</div>
									{#if result.noEligibleVoters}
										<p class="text-muted m-0 text-xs">No eligible voters; this capture scores 0 points.</p>
									{/if}
								</article>
							{/each}
						</div>
					</section>
				{/if}
			{/if}
		{/if}
	</div>
</section>
