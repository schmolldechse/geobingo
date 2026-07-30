<script lang="ts">
	import Target from "@lucide/svelte/icons/target";
	import Input, { type InputValue } from "$lib/components/ui/Input.svelte";
	import type { CaptureChallengePublicProjection } from "$lib/generated/realtime/GeoBingo.Contracts.GameModes.CaptureChallenge";
	import type { LobbyActions } from "$lib/lobbies/lobby-actions";
	import type { LobbyState } from "$lib/lobbies/lobby-state.svelte";
	import CaptureChallengeGoalList from "./CaptureChallengeGoalList.svelte";
	import { buildCaptureChallengeSettings, SETTINGS_UPDATE_DEBOUNCE_MS } from "../../lobby-waiting-settings";

	let {
		lobby,
		capture,
		actions
	}: {
		lobby: LobbyState;
		capture: CaptureChallengePublicProjection;
		actions: LobbyActions;
	} = $props();

	const uid = $props.id();
	const captureTimeId = `${uid}-capture-time`;
	const votingTimeId = `${uid}-voting-time`;

	let captureDurationMinutes = $derived<InputValue>(capture.settings.captureDurationSeconds / 60);
	let secondsPerVote = $derived<InputValue>(capture.settings.secondsPerVote);

	const settingsPending = $derived(lobby.isPending("updateCaptureChallengeSettings"));
	const settingsDisabled = $derived(!lobby.canAdminister || settingsPending);

	function resetDraft(): void {
		captureDurationMinutes = capture.settings.captureDurationSeconds / 60;
		secondsPerVote = capture.settings.secondsPerVote;
	}

	async function updateSettings(): Promise<void> {
		const nextCaptureMinutes = Number(captureDurationMinutes);
		const nextSecondsPerVote = Number(secondsPerVote);
		if (!Number.isFinite(nextCaptureMinutes) || !Number.isFinite(nextSecondsPerVote)) {
			resetDraft();
			return;
		}

		const nextSettings = buildCaptureChallengeSettings(nextCaptureMinutes, nextSecondsPerVote);
		if (
			nextSettings.captureDurationSeconds === capture.settings.captureDurationSeconds &&
			nextSettings.secondsPerVote === capture.settings.secondsPerVote
		)
			return;

		await actions.updateCaptureChallengeSettings(nextSettings);
		if (lobby.lastOperationError) resetDraft();
	}
</script>

<section
	class="border-foreground bg-surface flex min-h-0 flex-col rounded-2xl border-2 shadow-[var(--shadow-paper-raised)] min-[900px]:h-full min-[900px]:overflow-hidden"
	aria-labelledby={`${uid}-title`}
	data-capture-challenge-settings
>
	<header class="border-border flex shrink-0 items-center justify-between gap-3 border-b-2 p-4" data-capture-settings-header>
		<div class="min-w-0">
			<p class="text-primary m-0 text-[0.65rem] font-black tracking-[0.15em] uppercase">Game mode settings</p>
			<h2 id={`${uid}-title`} class="mt-1 mb-0 text-lg font-black tracking-tight">Capture Challenge settings</h2>
			<p class="text-muted mt-1 mb-0 text-xs leading-5" data-capture-settings-subtitle>
				Timing and goals belong exclusively to this game mode.
			</p>
		</div>
		<Target class="text-secondary shrink-0" size={23} aria-hidden="true" />
	</header>

	<div class="border-border grid shrink-0 gap-3 border-b-2 p-3 sm:grid-cols-2" data-capture-controls>
		<div class="border-border rounded-xl border-2 p-3" data-capture-control>
			<div class="flex items-start justify-between gap-3">
				<label for={captureTimeId} class="text-sm font-extrabold">
					Capture time
					<span class="text-muted mt-0.5 block text-xs leading-5 font-semibold" data-capture-control-help>
						Time available to complete every goal.
					</span>
				</label>
				<output
					for={captureTimeId}
					class="border-foreground bg-accent text-accent-foreground shrink-0 rounded-full border-2 px-2.5 py-1 font-[Fredoka_Variable] text-xs font-semibold"
				>
					{Number(captureDurationMinutes)} min
				</output>
			</div>
			<Input
				id={captureTimeId}
				type="range"
				bind:value={captureDurationMinutes}
				min={3}
				max={60}
				step={1}
				rangeMinLabel="3 min"
				rangeMaxLabel="60 min"
				debounceTime={SETTINGS_UPDATE_DEBOUNCE_MS}
				onchange={updateSettings}
				disabled={settingsDisabled}
				aria-label="Capture time in minutes"
				class="mt-2"
			/>
		</div>

		<div class="border-border rounded-xl border-2 p-3" data-capture-control>
			<div class="flex items-start justify-between gap-3">
				<label for={votingTimeId} class="text-sm font-extrabold">
					Voting time
					<span class="text-muted mt-0.5 block text-xs leading-5 font-semibold" data-capture-control-help>
						Time to vote on each submitted capture.
					</span>
				</label>
				<output
					for={votingTimeId}
					class="border-foreground bg-accent text-accent-foreground shrink-0 rounded-full border-2 px-2.5 py-1 font-[Fredoka_Variable] text-xs font-semibold"
				>
					{Number(secondsPerVote)} sec
				</output>
			</div>
			<Input
				id={votingTimeId}
				type="range"
				bind:value={secondsPerVote}
				min={5}
				max={60}
				step={5}
				rangeMinLabel="5 sec"
				rangeMaxLabel="60 sec"
				debounceTime={SETTINGS_UPDATE_DEBOUNCE_MS}
				onchange={updateSettings}
				disabled={settingsDisabled}
				aria-label="Voting time in seconds"
				class="mt-2"
			/>
		</div>
	</div>

	<CaptureChallengeGoalList {lobby} goals={capture.goals} {actions} />
</section>

<style>
	@media (min-width: 900px) and (max-height: 760px) {
		[data-capture-settings-header] {
			padding: 0.625rem 0.875rem;
		}

		[data-capture-settings-subtitle],
		[data-capture-control-help] {
			display: none;
		}

		[data-capture-controls] {
			gap: 0.5rem;
			padding: 0.5rem;
		}

		[data-capture-control] {
			padding: 0.625rem;
		}
	}
</style>
