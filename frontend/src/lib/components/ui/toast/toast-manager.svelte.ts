import { browser, dev } from "$app/environment";

type ToastVariant = "default" | "success" | "info" | "warning" | "error";
type ToastPriority = "polite" | "assertive";
type ToastPosition = "top-left" | "top-center" | "top-right" | "bottom-left" | "bottom-center" | "bottom-right";
type ToastId = string;
type ToastPhase = "queued" | "visible" | "closing";

type ToastAction = {
	label: string;
	onclick: (event: MouseEvent) => void | Promise<void>;
};

type ToastOptions = {
	description?: string;
	variant?: ToastVariant;
	priority?: ToastPriority;
	duration?: number;
	dismissible?: boolean;
	action?: ToastAction;
};

type ToastRecord = {
	id: ToastId;
	title: string;
	description?: string;
	variant: ToastVariant;
	priority: ToastPriority;
	duration: number;
	dismissible: boolean;
	action?: ToastAction;
	open: boolean;
	phase: ToastPhase;
};

type ToastManagerConfiguration = {
	duration: number;
	visibleToasts: number;
	exitDuration: number;
};

type ToastHostOptions = {
	paused?: boolean;
	onClose?: (id: ToastId) => void;
};

type ToastHost = {
	readonly active: boolean;
	configure: (configuration: ToastManagerConfiguration) => void;
	setPaused: (paused: boolean) => void;
	disconnect: () => void;
};

type ToastTimer = {
	remaining: number;
	startedAt?: number;
	handle?: number;
	pending: boolean;
	generation: number;
};

type ToastMethodOptions = Omit<ToastOptions, "variant">;

type ToastFunction = {
	(title: string, options?: ToastOptions): ToastId;
	show: (title: string, options?: ToastOptions) => ToastId;
	success: (title: string, options?: ToastMethodOptions) => ToastId;
	info: (title: string, options?: ToastMethodOptions) => ToastId;
	warning: (title: string, options?: ToastMethodOptions) => ToastId;
	error: (title: string, options?: ToastMethodOptions) => ToastId;
	dismiss: (id?: ToastId) => void;
};

const DEFAULT_DURATION = 5000;
const DEFAULT_VISIBLE_TOASTS = 3;
const DEFAULT_EXIT_DURATION = 150;
const EXIT_FALLBACK_BUFFER = 100;
const MAX_TIMER_DURATION = 2_147_000_000;
const SSR_TOAST_ID = "toast-ssr";

let toastSequence = 0;

const normalizeDuration = (duration: number, fallback = 0): number => {
	if (!Number.isFinite(duration)) return fallback;
	return Math.min(MAX_TIMER_DURATION, Math.max(0, duration));
};

const normalizeVisibleToasts = (visibleToasts: number): number =>
	Math.max(1, Math.floor(Number.isFinite(visibleToasts) ? visibleToasts : DEFAULT_VISIBLE_TOASTS));

const createToastId = (): ToastId => {
	toastSequence += 1;
	return `toast-${Date.now().toString(36)}-${toastSequence.toString(36)}`;
};

const inactiveHost: ToastHost = {
	active: false,
	configure: () => undefined,
	setPaused: () => undefined,
	disconnect: () => undefined
};

class ToastManager {
	#toasts: ToastRecord[] = $state([]);
	#timers = new Map<ToastId, ToastTimer>();
	#exitTimers = new Map<ToastId, number>();
	#defaultDuration = DEFAULT_DURATION;
	#visibleLimit = DEFAULT_VISIBLE_TOASTS;
	#exitDuration = DEFAULT_EXIT_DURATION;
	#paused = false;
	#hostToken: symbol | undefined;
	#onClose: ((id: ToastId) => void) | undefined;

	public get visibleToasts(): ToastRecord[] {
		return this.#toasts.filter((toast) => toast.phase !== "queued");
	}

	public connect(configuration: ToastManagerConfiguration, options: ToastHostOptions = {}): ToastHost {
		if (!browser) return inactiveHost;

		if (this.#hostToken) {
			if (dev) console.warn("GeoBingo Toast: Es darf nur ein globaler <Toaster /> eingebunden werden.");
			return inactiveHost;
		}

		const token = Symbol("GeoBingoToastHost");
		this.#hostToken = token;
		this.#paused = options.paused ?? false;
		this.#onClose = options.onClose;
		this.#configure(configuration);
		this.#promoteQueued();
		this.#syncTimers();

		return {
			active: true,
			configure: (nextConfiguration) => {
				if (this.#hostToken !== token) return;
				this.#configure(nextConfiguration);
			},
			setPaused: (paused) => {
				if (this.#hostToken !== token || this.#paused === paused) return;
				this.#paused = paused;
				this.#syncTimers();
			},
			disconnect: () => {
				if (this.#hostToken !== token) return;
				this.#hostToken = undefined;
				this.#onClose = undefined;
				this.#reset();
			}
		};
	}

	public add(title: string, options: ToastOptions = {}): ToastId {
		if (!browser) return SSR_TOAST_ID;

		const id = createToastId();
		const variant = options.variant ?? "default";
		const duration = normalizeDuration(options.duration ?? this.#defaultDuration, this.#defaultDuration);
		const record: ToastRecord = {
			id,
			title,
			description: options.description,
			variant,
			priority: options.priority ?? (variant === "error" ? "assertive" : "polite"),
			duration,
			dismissible: options.dismissible ?? true,
			action: options.action,
			open: true,
			phase: "queued"
		};

		this.#toasts.push(record);
		if (duration > 0) {
			this.#timers.set(id, {
				remaining: duration,
				pending: false,
				generation: 0
			});
		}

		this.#promoteQueued();
		this.#syncTimers();

		return id;
	}

	public dismiss(id?: ToastId): void {
		if (!browser) return;

		if (id === undefined) {
			this.#dismissAll();
			return;
		}

		const toast = this.#toasts.find((item) => item.id === id);
		if (!toast || toast.phase === "closing") return;

		if (toast.phase === "queued") {
			this.#removeMany([id]);
			return;
		}

		this.#beginClose(toast);
	}

	public finalize(id: ToastId): void {
		if (!browser) return;

		const toast = this.#toasts.find((item) => item.id === id);
		if (!toast || toast.phase !== "closing") return;

		this.#removeMany([id], false);
		this.#promoteQueued();
		this.#syncTimers();
	}

	#configure(configuration: ToastManagerConfiguration): void {
		this.#defaultDuration = normalizeDuration(configuration.duration, DEFAULT_DURATION);
		this.#visibleLimit = normalizeVisibleToasts(configuration.visibleToasts);
		this.#exitDuration = normalizeDuration(configuration.exitDuration, DEFAULT_EXIT_DURATION);

		if (this.#exitDuration === 0) {
			const closingIds = this.#toasts.filter((toast) => toast.phase === "closing").map((toast) => toast.id);
			this.#removeMany(closingIds, false);
		}

		this.#demoteExcessVisible();
		this.#promoteQueued();
		this.#syncTimers();
	}

	#dismissAll(): void {
		const queuedIds = this.#toasts.filter((toast) => toast.phase === "queued").map((toast) => toast.id);
		this.#removeMany(queuedIds, false);

		const visibleToasts = this.#toasts.filter((toast) => toast.phase === "visible");
		for (const toast of visibleToasts) this.#markClosing(toast);

		if (this.#exitDuration === 0) {
			const closingIds = this.#toasts.filter((toast) => toast.phase === "closing").map((toast) => toast.id);
			this.#removeMany(closingIds, false);
		}

		this.#promoteQueued();
		this.#syncTimers();
	}

	#beginClose(toast: ToastRecord): void {
		this.#markClosing(toast);

		if (this.#exitDuration === 0) {
			this.#removeMany([toast.id], false);
			this.#promoteQueued();
		}

		this.#syncTimers();
	}

	#markClosing(toast: ToastRecord): void {
		if (toast.phase !== "visible") return;

		toast.phase = "closing";
		toast.open = false;
		this.#clearToastTimer(toast.id);
		this.#onClose?.(toast.id);

		if (this.#exitDuration === 0) return;

		const exitTimer = window.setTimeout(() => {
			this.#exitTimers.delete(toast.id);
			this.finalize(toast.id);
		}, this.#exitDuration + EXIT_FALLBACK_BUFFER);

		this.#exitTimers.set(toast.id, exitTimer);
	}

	#demoteExcessVisible(): void {
		const closingCount = this.#toasts.filter((toast) => toast.phase === "closing").length;
		const allowedVisibleCount = Math.max(0, this.#visibleLimit - closingCount);
		const visibleToasts = this.#toasts.filter((toast) => toast.phase === "visible");
		let excessCount = visibleToasts.length - allowedVisibleCount;

		for (let index = visibleToasts.length - 1; index >= 0 && excessCount > 0; index -= 1) {
			visibleToasts[index].phase = "queued";
			this.#onClose?.(visibleToasts[index].id);
			excessCount -= 1;
		}
	}

	#promoteQueued(): void {
		if (!this.#hostToken) return;

		let occupiedSlots = this.#toasts.filter((toast) => toast.phase !== "queued").length;
		if (occupiedSlots >= this.#visibleLimit) return;

		for (const toast of this.#toasts) {
			if (toast.phase !== "queued") continue;

			toast.phase = "visible";
			occupiedSlots += 1;
			if (occupiedSlots >= this.#visibleLimit) break;
		}
	}

	#removeMany(ids: ToastId[], synchronize = true): void {
		if (ids.length === 0) {
			if (synchronize) {
				this.#promoteQueued();
				this.#syncTimers();
			}
			return;
		}

		const idsToRemove = new Set(ids);

		for (const id of idsToRemove) {
			this.#clearToastTimer(id);

			const exitTimer = this.#exitTimers.get(id);
			if (exitTimer !== undefined) {
				window.clearTimeout(exitTimer);
				this.#exitTimers.delete(id);
			}
		}

		for (let index = this.#toasts.length - 1; index >= 0; index -= 1) {
			if (idsToRemove.has(this.#toasts[index].id)) this.#toasts.splice(index, 1);
		}

		if (!synchronize) return;

		this.#promoteQueued();
		this.#syncTimers();
	}

	#syncTimers(): void {
		if (!browser) return;

		const activeIds = new Set(
			this.#hostToken && !this.#paused ? this.#toasts.filter((toast) => toast.phase === "visible").map((toast) => toast.id) : []
		);

		for (const [id, timer] of this.#timers) {
			if (!activeIds.has(id)) {
				this.#pauseTimer(timer);
				continue;
			}

			if (timer.handle === undefined && !timer.pending) this.#startTimer(id, timer);
		}
	}

	#startTimer(id: ToastId, timer: ToastTimer): void {
		const generation = timer.generation + 1;
		timer.generation = generation;

		if (timer.remaining <= 0) {
			timer.pending = true;
			queueMicrotask(() => {
				const currentTimer = this.#timers.get(id);
				const currentToast = this.#toasts.find((toast) => toast.id === id);
				if (currentTimer !== timer || timer.generation !== generation || !timer.pending || currentToast?.phase !== "visible") {
					return;
				}

				timer.pending = false;
				this.dismiss(id);
			});
			return;
		}

		timer.startedAt = performance.now();
		timer.handle = window.setTimeout(() => {
			const currentTimer = this.#timers.get(id);
			const currentToast = this.#toasts.find((toast) => toast.id === id);
			if (currentTimer !== timer || timer.generation !== generation || currentToast?.phase !== "visible") return;

			timer.handle = undefined;
			timer.startedAt = undefined;
			timer.remaining = 0;
			this.dismiss(id);
		}, timer.remaining);
	}

	#pauseTimer(timer: ToastTimer): void {
		if (timer.handle === undefined && !timer.pending) return;

		timer.generation += 1;
		timer.pending = false;

		if (timer.handle === undefined || timer.startedAt === undefined) return;

		window.clearTimeout(timer.handle);
		timer.remaining = Math.max(0, timer.remaining - (performance.now() - timer.startedAt));
		timer.handle = undefined;
		timer.startedAt = undefined;
	}

	#clearToastTimer(id: ToastId): void {
		const timer = this.#timers.get(id);
		if (!timer) return;

		timer.generation += 1;
		timer.pending = false;
		if (timer.handle !== undefined) window.clearTimeout(timer.handle);
		this.#timers.delete(id);
	}

	#reset(): void {
		for (const timer of this.#timers.values()) {
			timer.generation += 1;
			if (timer.handle !== undefined) window.clearTimeout(timer.handle);
		}

		for (const exitTimer of this.#exitTimers.values()) window.clearTimeout(exitTimer);

		this.#timers.clear();
		this.#exitTimers.clear();
		this.#toasts.splice(0);
		this.#paused = false;
	}
}

const toastManager = new ToastManager();

const showToast = (title: string, options: ToastOptions = {}): ToastId => toastManager.add(title, options);

const toast = Object.assign(showToast, {
	show: showToast,
	success: (title: string, options: ToastMethodOptions = {}) =>
		showToast(title, {
			...options,
			variant: "success"
		}),
	info: (title: string, options: ToastMethodOptions = {}) =>
		showToast(title, {
			...options,
			variant: "info"
		}),
	warning: (title: string, options: ToastMethodOptions = {}) =>
		showToast(title, {
			...options,
			variant: "warning"
		}),
	error: (title: string, options: ToastMethodOptions = {}) =>
		showToast(title, {
			...options,
			variant: "error"
		}),
	dismiss: (id?: ToastId) => toastManager.dismiss(id)
}) satisfies ToastFunction;

export {
	toast,
	toastManager,
	type ToastAction,
	type ToastFunction,
	type ToastHost,
	type ToastId,
	type ToastOptions,
	type ToastPosition,
	type ToastPriority,
	type ToastRecord,
	type ToastVariant
};
