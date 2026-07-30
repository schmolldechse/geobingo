<script lang="ts">
	import ArrowDown from "@lucide/svelte/icons/arrow-down";
	import ArrowUp from "@lucide/svelte/icons/arrow-up";
	import Pencil from "@lucide/svelte/icons/pencil";
	import Plus from "@lucide/svelte/icons/plus";
	import Target from "@lucide/svelte/icons/target";
	import Trash2 from "@lucide/svelte/icons/trash-2";
	import GoalEditorDialog from "@/lib/components/lobby/state/waiting/gamemodes/capturechallenge/GoalEditorDialog.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Dialog from "$lib/components/ui/Dialog.svelte";
	import type {
		CaptureChallengeGoalProjection,
		CaptureGoalInput
	} from "$lib/generated/realtime/GeoBingo.Contracts.GameModes.CaptureChallenge";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import type { LobbyState } from "$lib/lobbies/lobby-state.svelte";
	import { reorderGoalIds } from "../../lobby-waiting-settings";

	let {
		lobby,
		goals,
		actions
	}: {
		lobby: LobbyState;
		goals: CaptureChallengeGoalProjection[];
		actions: LobbyActions;
	} = $props();

	let editorVisible = $state(false);
	let editedGoal = $state<CaptureChallengeGoalProjection | null>(null);
	let removedGoal = $state<CaptureChallengeGoalProjection | null>(null);
	let removeDialogVisible = $state(false);

	const scoreFactorFormatter = new Intl.NumberFormat("en-US", {
		minimumFractionDigits: 1,
		maximumFractionDigits: 1
	});

	function addGoal(): void {
		editedGoal = null;
		editorVisible = true;
	}

	function editGoal(goal: CaptureChallengeGoalProjection): void {
		editedGoal = goal;
		editorVisible = true;
	}

	async function saveGoal(goal: CaptureGoalInput): Promise<void> {
		if (editedGoal) await actions.updateGoal(editedGoal.goalId, goal);
		else await actions.addGoal(goal);

		if (!lobby.lastOperationError) editorVisible = false;
	}

	function requestRemove(goal: CaptureChallengeGoalProjection): void {
		removedGoal = goal;
		removeDialogVisible = true;
	}

	function cancelRemove(): void {
		removedGoal = null;
		removeDialogVisible = false;
	}

	async function confirmRemove(): Promise<void> {
		if (!removedGoal) return;
		await actions.removeGoal(removedGoal.goalId);
		if (!lobby.lastOperationError) cancelRemove();
	}

	async function moveGoal(index: number, direction: -1 | 1): Promise<void> {
		const orderedGoalIds = reorderGoalIds(
			goals.map((goal) => goal.goalId),
			index,
			direction
		);
		await actions.reorderGoals(orderedGoalIds);
	}
</script>

<div class="flex min-h-0 flex-1 flex-col" data-capture-challenge-goals>
	<header class="border-border flex shrink-0 items-center justify-between gap-3 border-b-2 p-3 sm:px-4" data-goals-header>
		<div class="min-w-0">
			<div class="flex flex-wrap items-center gap-2">
				<h3 class="m-0 text-base font-black">Goals</h3>
				<span
					class="bg-surface-muted inline-flex min-w-6 items-center justify-center rounded-full px-2 py-0.5 text-xs font-black"
				>
					{goals.length}
				</span>
			</div>
			<p class="text-muted mt-1 mb-0 text-xs leading-5" data-goals-subtitle>Capture Challenge content for the next round.</p>
		</div>

		{#if lobby.canAdminister}
			<Button size="sm" variant="accent" onclick={addGoal} disabled={goals.length >= 25 || lobby.isPending("addGoal")}>
				<Plus size={16} aria-hidden="true" />
				<span class="max-[420px]:sr-only">Add goal</span>
			</Button>
		{/if}
	</header>

	{#if goals.length === 0}
		<div class="grid min-h-52 flex-1 place-items-center p-6 text-center">
			<div>
				<Target class="text-secondary mx-auto" size={34} aria-hidden="true" />
				<p class="mt-3 mb-1 font-black">No goals yet</p>
				<p class="text-muted m-0 text-sm leading-6">
					{lobby.canAdminister ? "Add at least one goal before starting the round." : "The host is preparing the goals."}
				</p>
			</div>
		</div>
	{:else}
		<ul class="m-0 min-h-0 flex-1 list-none divide-y-2 divide-[var(--border)] overflow-visible p-0 min-[900px]:overflow-y-auto">
			{#each goals as goal, index (goal.goalId)}
				<li class="grid min-w-0 grid-cols-[minmax(0,1fr)_auto] items-center gap-3 px-3 py-3 max-[560px]:grid-cols-1 sm:px-4">
					<div class="flex min-w-0 flex-wrap items-center gap-2">
						<p class="m-0 min-w-0 overflow-hidden font-extrabold text-ellipsis">{goal.title}</p>
						<span class="bg-accent/30 text-accent-foreground shrink-0 rounded-full px-2 py-0.5 text-xs font-black">
							× {scoreFactorFormatter.format(goal.scoreFactor)}
						</span>
					</div>

					{#if lobby.canAdminister}
						<div class="flex shrink-0 items-center justify-end gap-1 max-[560px]:justify-start">
							<Button
								size="icon"
								variant="ghost"
								class="border-border size-9 min-h-9 border-2 p-0"
								aria-label={`Move ${goal.title} up`}
								title="Move up"
								disabled={index === 0 || lobby.isPending("reorderGoals")}
								onclick={() => moveGoal(index, -1)}
							>
								<ArrowUp size={15} aria-hidden="true" />
							</Button>
							<Button
								size="icon"
								variant="ghost"
								class="border-border size-9 min-h-9 border-2 p-0"
								aria-label={`Move ${goal.title} down`}
								title="Move down"
								disabled={index === goals.length - 1 || lobby.isPending("reorderGoals")}
								onclick={() => moveGoal(index, 1)}
							>
								<ArrowDown size={15} aria-hidden="true" />
							</Button>
							<Button
								size="icon"
								variant="ghost"
								class="border-border size-9 min-h-9 border-2 p-0"
								aria-label={`Edit ${goal.title}`}
								title="Edit goal"
								disabled={lobby.isPending(`updateGoal:${goal.goalId}`)}
								onclick={() => editGoal(goal)}
							>
								<Pencil size={15} aria-hidden="true" />
							</Button>
							<Button
								size="icon"
								variant="ghost"
								class="border-border text-primary hover:bg-primary/10 size-9 min-h-9 border-2 p-0"
								aria-label={`Remove ${goal.title}`}
								title="Remove goal"
								disabled={lobby.isPending(`removeGoal:${goal.goalId}`)}
								onclick={() => requestRemove(goal)}
							>
								<Trash2 size={15} aria-hidden="true" />
							</Button>
						</div>
					{/if}
				</li>
			{/each}
		</ul>
	{/if}
</div>

{#if editorVisible}
	<GoalEditorDialog
		bind:isVisible={editorVisible}
		goal={editedGoal}
		pending={editedGoal ? lobby.isPending(`updateGoal:${editedGoal.goalId}`) : lobby.isPending("addGoal")}
		onsave={saveGoal}
	/>
{/if}

<Dialog bind:isVisible={removeDialogVisible} title="Remove goal?" showActions={false} onclose={cancelRemove}>
	<p class="text-muted m-0 leading-6">
		“{removedGoal?.title}” will be removed from the lobby configuration. The remaining order stays unchanged.
	</p>
	<div class="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
		<Button variant="outline" onclick={cancelRemove}>Cancel</Button>
		<Button
			variant="destructive"
			onclick={confirmRemove}
			disabled={removedGoal ? lobby.isPending(`removeGoal:${removedGoal.goalId}`) : false}
		>
			{removedGoal && lobby.isPending(`removeGoal:${removedGoal.goalId}`) ? "Removing..." : "Remove goal"}
		</Button>
	</div>
</Dialog>

<style>
	@media (min-width: 900px) and (max-height: 760px) {
		[data-goals-header] {
			padding-block: 0.5rem;
		}

		[data-goals-subtitle] {
			display: none;
		}
	}
</style>
