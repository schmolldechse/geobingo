import { getContext, setContext } from "svelte";

const CONTEXT_KEY = Symbol("GeoBingoTabs");

type TabsOrientation = "horizontal" | "vertical";
type TabsActivationMode = "automatic" | "manual";
type TabsState = "active" | "inactive";

type TabsTriggerRecord = {
	element: HTMLButtonElement;
	isDisabled: () => boolean;
};

interface TabsContextProps {
	get value(): string;
	set value(value: string);
	get orientation(): TabsOrientation;
	get activationMode(): TabsActivationMode;
	get loop(): boolean;
	get disabled(): boolean;
	get baseId(): string;
	onvaluechange?: (value: string) => void;
}

class TabsContext {
	triggers: TabsTriggerRecord[] = [];

	constructor(private props: TabsContextProps) {}

	get value(): string {
		return this.props.value;
	}

	get orientation(): TabsOrientation {
		return this.props.orientation;
	}

	get activationMode(): TabsActivationMode {
		return this.props.activationMode;
	}

	get loop(): boolean {
		return this.props.loop;
	}

	get disabled(): boolean {
		return this.props.disabled;
	}

	get state(): (value: string) => TabsState {
		return (value) => (this.value === value ? "active" : "inactive");
	}

	getTriggerId(value: string): string {
		return `${this.props.baseId}-trigger-${encodeURIComponent(value)}`;
	}

	getContentId(value: string): string {
		return `${this.props.baseId}-content-${encodeURIComponent(value)}`;
	}

	isTriggerDisabled(disabled: boolean): boolean {
		return this.disabled || disabled;
	}

	setValue(value: string): void {
		if (this.disabled || value === this.value) return;

		this.props.value = value;
		this.props.onvaluechange?.(value);
	}

	registerTrigger(record: TabsTriggerRecord): () => void {
		this.triggers.push(record);

		return () => {
			this.triggers = this.triggers.filter((trigger) => trigger !== record);
		};
	}

	handleTriggerKeydown(current: HTMLButtonElement, event: KeyboardEvent): void {
		const enabledTriggers = this.triggers.filter((trigger) => !trigger.isDisabled());
		if (enabledTriggers.length === 0) return;

		const currentIndex = enabledTriggers.findIndex((trigger) => trigger.element === current);
		if (currentIndex < 0) return;

		let nextIndex: number | undefined;

		if (event.key === "Home") nextIndex = 0;
		else if (event.key === "End") nextIndex = enabledTriggers.length - 1;
		else if (this.orientation === "horizontal" && event.key === "ArrowLeft") nextIndex = currentIndex - 1;
		else if (this.orientation === "horizontal" && event.key === "ArrowRight") nextIndex = currentIndex + 1;
		else if (this.orientation === "vertical" && event.key === "ArrowUp") nextIndex = currentIndex - 1;
		else if (this.orientation === "vertical" && event.key === "ArrowDown") nextIndex = currentIndex + 1;
		else return;

		event.preventDefault();

		if (this.loop) {
			nextIndex = (nextIndex + enabledTriggers.length) % enabledTriggers.length;
		} else if (nextIndex < 0 || nextIndex >= enabledTriggers.length) {
			return;
		}

		enabledTriggers[nextIndex]?.element.focus();
	}
}

function setTabsContext(value: TabsContext): void {
	setContext(CONTEXT_KEY, value);
}

function getTabsContext(): TabsContext {
	const value = getContext<TabsContext | undefined>(CONTEXT_KEY);
	if (!value) throw new Error("Tabs components must be used inside Tabs.Root.");
	return value;
}

export {
	TabsContext,
	getTabsContext,
	setTabsContext,
	type TabsActivationMode,
	type TabsOrientation,
	type TabsState,
	type TabsTriggerRecord
};
