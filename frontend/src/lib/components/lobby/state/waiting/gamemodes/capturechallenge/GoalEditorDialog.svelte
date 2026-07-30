<script lang="ts">
	import type {
		CaptureChallengeGoalProjection,
		CaptureGoalInput
	} from "$lib/generated/realtime/GeoBingo.Contracts.GameModes.CaptureChallenge";
	import GoalScoreFactorControl from "@/lib/components/lobby/state/waiting/gamemodes/capturechallenge/GoalScoreFactorControl.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Dialog from "$lib/components/ui/Dialog.svelte";
	import Input from "$lib/components/ui/Input.svelte";

	type Props = {
		isVisible?: boolean;
		goal?: CaptureChallengeGoalProjection | null;
		pending?: boolean;
		onsave: (goal: CaptureGoalInput) => void | Promise<void>;
		onclose?: () => void;
	};

	let { isVisible = $bindable(false), goal = null, pending = false, onsave, onclose }: Props = $props();
	let editedTitle = $state<string | null>(null);
	let editedScoreFactor = $state<number | null>(null);
	const title = $derived(editedTitle ?? goal?.title ?? "");
	const scoreFactor = $derived(editedScoreFactor ?? goal?.scoreFactor ?? 1);
	const uid = $props.id();
	const factorLabelId = `${uid}-score-factor`;

	const save = async () => {
		await onsave({
			title: title.trim(),
			scoreFactor
		});
	};
</script>

<Dialog bind:isVisible title={goal ? "Edit goal" : "Add goal"} {onclose} showActions={false}>
	<div class="space-y-4">
		<div>
			<label for="goal-title" class="mb-1.5 block text-sm font-extrabold">Name</label>
			<Input
				id="goal-title"
				value={title}
				onchange={(value) => (editedTitle = String(value))}
				maxlength={80}
				placeholder="For example: Find a red mailbox"
			/>
		</div>
		<div>
			<p id={factorLabelId} class="mb-1.5 text-sm font-extrabold">Score factor</p>
			<GoalScoreFactorControl
				value={scoreFactor}
				onchange={(value) => (editedScoreFactor = value)}
				disabled={pending}
				labelledby={factorLabelId}
			/>
			<p class="text-muted mt-1.5 mb-0 text-xs">Adjust the value from 0 to 10 in 0.5-point steps.</p>
		</div>
		<div class="flex flex-col-reverse gap-2 pt-2 sm:flex-row sm:justify-end">
			<Button variant="outline" onclick={() => (isVisible = false)}>Cancel</Button>
			<Button onclick={save} disabled={pending || title.trim().length === 0}>
				{pending ? "Saving..." : "Save goal"}
			</Button>
		</div>
	</div>
</Dialog>
