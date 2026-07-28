import type { IRetryPolicy, RetryContext } from "@microsoft/signalr";

export const reconnectDelays = [0, 2_000, 5_000, 10_000] as const;

export const boundedReconnectPolicy: IRetryPolicy = {
	nextRetryDelayInMilliseconds: (context: RetryContext) => reconnectDelays[context.previousRetryCount] ?? null
};
