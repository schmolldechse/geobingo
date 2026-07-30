import type { ToastFunction, ToastId } from "$lib/components/ui/toast";
import type { LobbyEvent, LobbyEventHandler } from "$lib/lobbies/lobby-events";

type LobbyToastPort = Pick<ToastFunction, "dismiss" | "error" | "success" | "warning">;

const SUCCESS_DURATION = 4000;
const ERROR_DURATION = 8000;

const successTitles: Record<string, string> = {
	updateLobbySettings: "Lobby settings updated",
	updateCaptureChallengeSettings: "Game settings updated",
	addGoal: "Goal added",
	updateGoal: "Goal updated",
	removeGoal: "Goal removed",
	reorderGoals: "Goal order updated",
	selectGameMode: "Game mode updated",
	transferHost: "Host transferred",
	kick: "Player kicked",
	ban: "Player banned"
};

const operationKey = (operationName: string): string => operationName.split(":", 1)[0];

export const createLobbyToastHandler = (toast: LobbyToastPort): LobbyEventHandler => {
	let connectionToastId: ToastId | undefined;
	let lastErrorCorrelationId: string | undefined;
	let successToastId: ToastId | undefined;

	const showSuccess = (title: string): void => {
		if (successToastId) toast.dismiss(successToastId);
		successToastId = toast.success(title, { duration: SUCCESS_DURATION });
	};

	const dismissConnectionToast = (): void => {
		if (!connectionToastId) return;
		toast.dismiss(connectionToastId);
		connectionToastId = undefined;
	};

	return (event: LobbyEvent): void => {
		switch (event.type) {
			case "operation-accepted": {
				const title = successTitles[operationKey(event.operationName)];
				if (title) showSuccess(title);
				return;
			}
			case "operation-failed": {
				const correlationId = event.error.correlationId.trim();
				if (correlationId && correlationId === lastErrorCorrelationId) return;
				if (correlationId) lastErrorCorrelationId = correlationId;
				toast.error("Action failed", {
					description: event.error.details,
					duration: ERROR_DURATION
				});
				return;
			}
			case "connection-interrupted":
				if (connectionToastId) return;
				connectionToastId = toast.warning("Connection interrupted", {
					description: "Trying to reconnect automatically.",
					duration: 0
				});
				return;
			case "connection-restored":
				dismissConnectionToast();
				showSuccess("Connection restored");
				return;
			case "connection-lost":
				dismissConnectionToast();
				toast.error("Connection lost", {
					description: event.details,
					duration: ERROR_DURATION
				});
		}
	};
};
