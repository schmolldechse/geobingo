import { Duration } from "luxon";

type LobbyModeSummaryMetric = {
	label: string;
	value: string;
};

type LobbyModeSummaryModel = {
	eyebrow: string;
	name: string;
	description: string;
	metrics: LobbyModeSummaryMetric[];
};

type LobbyModeSummarySource = {
	selectedMode: {
		displayName: string;
		description: string;
	};
	gameMode: {
		modeKey: string;
		captureChallenge?: {
			goals: unknown[];
			settings: {
				captureDurationSeconds: number;
				secondsPerVote: number;
			};
		};
	};
};

type LobbyStartPresentation = {
	kind: "ready" | "blocked" | "pending" | "waiting";
	title: string;
	description: string;
	canStart: boolean;
	buttonLabel: string | null;
	issueCount: number;
};

type LobbyStartPresentationSource = {
	isHost: boolean;
	pending: boolean;
	issueMessages: readonly string[];
};

const formatDuration = (seconds: number): string => {
	const duration = Duration.fromObject({ seconds: Math.max(0, Math.round(seconds)) }).shiftTo("minutes", "seconds");
	const minutes = duration.minutes;
	const remainingSeconds = duration.seconds;

	if (minutes === 0) return `${remainingSeconds} sec`;
	if (remainingSeconds === 0) return `${minutes} min`;
	return `${minutes} min ${remainingSeconds} sec`;
};

const getPlayerInitials = (displayName: string, handle: string): string => {
	const source = displayName.trim() || handle.trim().replace(/^@/, "");
	if (!source) return "?";

	return source
		.split(/\s+/u)
		.slice(0, 2)
		.map((part) => part.charAt(0).toLocaleUpperCase())
		.join("");
};

const getLobbyModeSummary = (source: LobbyModeSummarySource): LobbyModeSummaryModel => {
	const summary: LobbyModeSummaryModel = {
		eyebrow: "Next round",
		name: source.selectedMode.displayName,
		description: source.selectedMode.description,
		metrics: []
	};
	const captureChallenge = source.gameMode.captureChallenge;

	if (source.gameMode.modeKey !== "capture_challenge" || !captureChallenge) return summary;

	return {
		...summary,
		metrics: [
			{
				label: "Goals",
				value: `${captureChallenge.goals.length} ${captureChallenge.goals.length === 1 ? "goal" : "goals"}`
			},
			{
				label: "Capture time",
				value: formatDuration(captureChallenge.settings.captureDurationSeconds)
			},
			{
				label: "Voting time",
				value: formatDuration(captureChallenge.settings.secondsPerVote)
			}
		]
	};
};

const getLobbyStartPresentation = (source: LobbyStartPresentationSource): LobbyStartPresentation => {
	const issueCount = source.issueMessages.length;

	if (!source.isHost) {
		return {
			kind: "waiting",
			title: "Waiting for the host",
			description: "The round will start as soon as the host is ready.",
			canStart: false,
			buttonLabel: null,
			issueCount
		};
	}

	if (source.pending) {
		return {
			kind: "pending",
			title: "Starting the round",
			description: "GeoBingo is preparing the next round.",
			canStart: false,
			buttonLabel: "Starting…",
			issueCount
		};
	}

	if (issueCount > 0) {
		return {
			kind: "blocked",
			title: "Setup needs attention",
			description:
				issueCount === 1 ? source.issueMessages[0] : `${issueCount} setup issues need attention before the round can start.`,
			canStart: false,
			buttonLabel: "Start round",
			issueCount
		};
	}

	return {
		kind: "ready",
		title: "Ready to start",
		description: "The lobby setup is complete.",
		canStart: true,
		buttonLabel: "Start round",
		issueCount: 0
	};
};

export {
	formatDuration,
	getLobbyModeSummary,
	getLobbyStartPresentation,
	getPlayerInitials,
	type LobbyModeSummaryMetric,
	type LobbyModeSummaryModel,
	type LobbyModeSummarySource,
	type LobbyStartPresentation,
	type LobbyStartPresentationSource
};
