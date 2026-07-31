<script lang="ts">
	import Camera from "@lucide/svelte/icons/camera";
	import ChartNoAxesCombined from "@lucide/svelte/icons/chart-no-axes-combined";
	import Crown from "@lucide/svelte/icons/crown";
	import Medal from "@lucide/svelte/icons/medal";
	import ThumbsDown from "@lucide/svelte/icons/thumbs-down";
	import ThumbsUp from "@lucide/svelte/icons/thumbs-up";
	import Trophy from "@lucide/svelte/icons/trophy";
	import { ResultsScope, type LobbySnapshot } from "$lib/generated/realtime/GeoBingo.Contracts.Lobbies";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import type { ResultState } from "$lib/lobbies/result-state.svelte";

	let {
		snapshot,
		results,
		actions
	}: {
		snapshot: LobbySnapshot;
		results: ResultState;
		actions: LobbyActions;
	} = $props();

	const visibleView = $derived.by(() => {
		switch (results.visibleScope) {
			case ResultsScope.LAST_COMPLETED_ROUND:
				return results.latestRound;
			case ResultsScope.SPECIFIC_ROUND:
				return results.visibleRoundId ? (results.specificRounds.get(results.visibleRoundId) ?? null) : null;
			default:
				return results.cumulative;
		}
	});
	const leaderboard = $derived(
		visibleView?.roundResults?.players ?? visibleView?.cumulativeResults ?? snapshot.cumulativeResults
	);
	const captureResults = $derived(visibleView?.roundResults?.captures ?? []);
	const completedRounds = $derived(
		[...snapshot.completedRoundSummaries].sort((left, right) => right.roundNumber - left.roundNumber)
	);
	const selectedRoundNumber = $derived(visibleView?.roundNumber ?? null);

	function playerName(userId: string): string {
		return snapshot.members.find((member) => member.userId === userId)?.displayName ?? "Former player";
	}

	function goalTitle(goalId: string): string {
		return snapshot.gameMode.captureChallenge?.goals.find((goal) => goal.goalId === goalId)?.title ?? "Capture goal";
	}

	function formatScore(score: number): string {
		return new Intl.NumberFormat("en", { minimumFractionDigits: 0, maximumFractionDigits: 2 }).format(score);
	}
</script>

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

		<nav class="grid gap-2" aria-label="Result views">
			<button
				type="button"
				class="border-border bg-surface-muted hover:border-foreground flex min-h-11 cursor-pointer items-center gap-2 rounded-xl border-2 px-3 text-left text-sm font-black transition-colors"
				class:border-foreground={results.visibleScope === ResultsScope.CUMULATIVE}
				class:bg-accent={results.visibleScope === ResultsScope.CUMULATIVE}
				onclick={() => void actions.selectResults(ResultsScope.CUMULATIVE)}
			>
				<ChartNoAxesCombined size={18} aria-hidden="true" />
				Overall standings
			</button>
			<button
				type="button"
				class="border-border bg-surface-muted hover:border-foreground flex min-h-11 cursor-pointer items-center gap-2 rounded-xl border-2 px-3 text-left text-sm font-black transition-colors disabled:cursor-not-allowed disabled:opacity-50"
				class:border-foreground={results.visibleScope === ResultsScope.LAST_COMPLETED_ROUND}
				class:bg-accent={results.visibleScope === ResultsScope.LAST_COMPLETED_ROUND}
				disabled={completedRounds.length === 0}
				onclick={() => void actions.selectResults(ResultsScope.LAST_COMPLETED_ROUND)}
			>
				<Trophy size={18} aria-hidden="true" />
				Latest round
			</button>
		</nav>

		{#if completedRounds.length > 0}
			<div class="border-border grid gap-2 border-t-2 border-dashed pt-3">
				<p class="text-muted m-0 text-[0.62rem] font-black tracking-[0.12em] uppercase">Round history</p>
				<div class="flex flex-wrap gap-2">
					{#each completedRounds as round (round.roundId)}
						<button
							type="button"
							class="border-border bg-surface hover:border-foreground min-h-9 cursor-pointer rounded-lg border-2 px-3 text-xs font-black"
							class:border-foreground={results.visibleScope === ResultsScope.SPECIFIC_ROUND &&
								results.visibleRoundId === round.roundId}
							class:bg-secondary={results.visibleScope === ResultsScope.SPECIFIC_ROUND &&
								results.visibleRoundId === round.roundId}
							class:text-secondary-foreground={results.visibleScope === ResultsScope.SPECIFIC_ROUND &&
								results.visibleRoundId === round.roundId}
							onclick={() => void actions.selectResults(ResultsScope.SPECIFIC_ROUND, round.roundId)}
						>
							Round {round.roundNumber}
						</button>
					{/each}
				</div>
			</div>
		{/if}
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
				{#if visibleView?.completedAt}
					<time class="text-muted text-xs font-bold" datetime={String(visibleView.completedAt)}>
						Completed {new Date(visibleView.completedAt).toLocaleString("en")}
					</time>
				{/if}
			</header>

			<div class="grid gap-2" aria-label="Player rankings">
				{#each leaderboard as player (player.userId)}
					<div
						class="border-border bg-surface-muted grid grid-cols-[auto_minmax(0,1fr)_auto] items-center gap-3 rounded-xl border-2 px-3 py-2.5"
						class:border-accent={player.rank === 1}
						class:bg-accent={player.rank === 1}
					>
						<div
							class="border-foreground bg-surface grid size-9 place-items-center rounded-full border-2 font-[Fredoka_Variable] text-sm font-[650]"
						>
							{#if player.rank === 1}<Crown size={18} aria-label="First place" />{:else}{player.rank}{/if}
						</div>
						<div class="min-w-0">
							<p class="m-0 overflow-hidden font-[Fredoka_Variable] font-[650] text-ellipsis whitespace-nowrap">
								{playerName(player.userId)}
							</p>
							<p class="text-muted m-0 text-[0.68rem] font-bold">Rank {player.rank}</p>
						</div>
						<p class="m-0 font-[Fredoka_Variable] text-xl font-[650] tabular-nums">
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
							<article class="border-border bg-surface-muted grid gap-2 rounded-xl border-2 p-3">
								<div class="flex items-start justify-between gap-3">
									<div class="min-w-0">
										<p class="text-secondary m-0 text-[0.58rem] font-black tracking-[0.1em] uppercase">
											{playerName(result.ownerUserId)}
										</p>
										<h5 class="mt-0.5 mb-0 line-clamp-2 font-[Fredoka_Variable] font-[650]">{goalTitle(result.goalId)}</h5>
									</div>
									<span class="border-foreground bg-accent shrink-0 rounded-full border px-2 py-1 text-xs font-black"
										>{formatScore(result.score)} pts</span
									>
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
	</div>
</section>
