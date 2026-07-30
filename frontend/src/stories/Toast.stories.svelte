<script module lang="ts">
	import CheckCircle2 from "@lucide/svelte/icons/circle-check-big";
	import RotateCcw from "@lucide/svelte/icons/rotate-ccw";
	import Sparkles from "@lucide/svelte/icons/sparkles";
	import { defineMeta } from "@storybook/addon-svelte-csf";
	import type { ComponentProps } from "svelte";

	import Button from "../lib/components/ui/Button.svelte";
	import * as Toast from "../lib/components/ui/toast";

	type PlaygroundArgs = Omit<ComponentProps<typeof Toast.Root>, "action" | "children" | "description" | "icon" | "title"> & {
		title: string;
		description: string;
	};

	const variants: Toast.ToastVariant[] = ["default", "success", "info", "warning", "error"];
	const positions: Toast.ToastPosition[] = [
		"top-left",
		"top-center",
		"top-right",
		"bottom-left",
		"bottom-center",
		"bottom-right"
	];
	const variantExamples: Record<Toast.ToastVariant, { title: string; description: string }> = {
		default: {
			title: "Notification received",
			description: "A neutral update that does not require immediate attention."
		},
		success: {
			title: "Changes saved",
			description: "The operation completed successfully."
		},
		info: {
			title: "Update available",
			description: "Additional information is ready to review."
		},
		warning: {
			title: "Connection interrupted",
			description: "Some features may be temporarily unavailable."
		},
		error: {
			title: "Operation failed",
			description: "The request could not be completed."
		}
	};

	let queueRun = 0;
	let selectedPosition = $state<Toast.ToastPosition>("bottom-right");
	let selectedVisibleToasts = $state(3);
	let selectedDuration = $state(5000);
	let selectedHotkey = $state<string | false>("F8");
	let persistentToastId: Toast.ToastId | undefined;
	let controlledOpen = $state(true);
	let controlledState = $state<"open" | "closed">("open");

	const showVariantToast = (variant: Exclude<Toast.ToastVariant, "default">): void => {
		const example = variantExamples[variant];

		Toast.toast[variant](example.title, {
			description: example.description
		});
	};

	const showCallableToast = (): void => {
		Toast.toast("Notification created", {
			description: "Created through the callable toast API."
		});
	};

	const showDefaultToast = (): void => {
		Toast.toast.show("Default notification", {
			description: "Created through toast.show()."
		});
	};

	const showConfiguredToast = (): void => {
		Toast.toast.show("Review required", {
			description: "This example sets variant, priority, duration, and dismissibility explicitly.",
			variant: "warning",
			priority: "assertive",
			duration: 7000,
			dismissible: false
		});
	};

	const showActionToast = (): void => {
		Toast.toast.warning("Item removed", {
			description: "The change can be reverted while this notification is visible.",
			duration: 0,
			action: {
				label: "Undo",
				onclick: () => {
					Toast.toast.success("Item restored");
				}
			}
		});
	};

	const showPersistentToast = (): void => {
		persistentToastId = Toast.toast.info("Persistent notification", {
			description: "This notification remains visible until it is dismissed.",
			duration: 0
		});
	};

	const dismissPersistentToast = (): void => {
		if (!persistentToastId) return;

		Toast.toast.dismiss(persistentToastId);
		persistentToastId = undefined;
	};

	const showQueue = (): void => {
		queueRun += 1;

		for (let index = 1; index <= 5; index += 1) {
			Toast.toast(`Queue ${queueRun} · Notification ${index}`, {
				description: index <= selectedVisibleToasts ? "Visible immediately" : "Waiting in the FIFO queue",
				duration: 2500
			});
		}
	};

	const showPositionToast = (position: Toast.ToastPosition): void => {
		selectedPosition = position;
		Toast.toast.info(position, {
			description: "The notification viewport now uses this position.",
			duration: 3000
		});
	};

	const handleControlledOpenChange = (open: boolean): void => {
		controlledOpen = open;
		controlledState = open ? "open" : "closed";
	};

	const { Story } = defineMeta({
		title: "UI/Toast",
		component: Toast.Root,
		render: playground,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen"
		},
		args: {
			open: true,
			title: "Changes saved",
			description: "The operation completed successfully.",
			variant: "success",
			priority: "polite",
			dismissible: true
		},
		argTypes: {
			open: { control: "boolean" },
			title: { control: "text" },
			description: { control: "text" },
			variant: { control: "select", options: variants },
			priority: { control: "inline-radio", options: ["polite", "assertive"] },
			dismissible: { control: "boolean" }
		}
	});
</script>

{#snippet customTitle()}
	<span class="flex items-center gap-2">
		<span>Custom content</span>
		<span class="rounded-full border border-current/30 px-2 py-0.5 text-[0.65rem] tracking-wide uppercase">New</span>
	</span>
{/snippet}

{#snippet customDescription()}
	<span>A title, description, and icon can all be supplied as Svelte snippets.</span>
{/snippet}

{#snippet customIcon()}
	<Sparkles aria-hidden="true" size={19} strokeWidth={2.5} />
{/snippet}

{#snippet playground(args: PlaygroundArgs)}
	<div class="grid min-h-52 place-items-center p-6 sm:p-8">
		<div class="w-full max-w-[26rem]">
			<Toast.Root {...args} />
		</div>
	</div>
{/snippet}

<Story name="Playground" />

<Story name="Variants" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Semantic variants</h2>
			<p class="text-muted m-0 text-sm">Color, icon, and announcement priority communicate each notification type.</p>
		</header>

		<div class="grid max-w-[30rem] gap-4">
			{#each variants as variant (variant)}
				<Toast.Root {variant} title={variantExamples[variant].title} description={variantExamples[variant].description} />
			{/each}
		</div>
	</section>
</Story>

<Story name="Content density" asChild>
	<section class="grid gap-5 p-4 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Content density</h2>
			<p class="text-muted m-0 max-w-[36rem] text-sm">
				Title-only notifications use the compact layout; descriptions, actions, and custom content retain the spacious layout.
			</p>
		</header>

		<div class="grid w-full max-w-[28rem] gap-4">
			<Toast.Root variant="success" title="Changes saved" />
			<Toast.Root
				variant="info"
				title="Background task completed"
				description="A short supporting sentence adds context without requiring an action."
			/>
			<Toast.Root
				variant="warning"
				title="Unsaved changes"
				description="Review the current values before continuing."
				action={{
					label: "Review",
					onclick: () => undefined
				}}
			/>
		</div>
	</section>
</Story>

<Story name="Composition" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Composition and optional elements</h2>
			<p class="text-muted m-0 max-w-[42rem] text-sm">
				Snippet props, custom content, hidden icons, actions, dismissibility, classes, and data attributes can be combined.
			</p>
		</header>

		<div class="grid max-w-[30rem] gap-4">
			<Toast.Root
				variant="success"
				title={customTitle}
				description={customDescription}
				icon={customIcon}
				class="ring-secondary/20 ring-4"
				data-example="snippet-composition"
			>
				<div class="flex items-center gap-2 border-t border-current/25 pt-3 text-xs font-bold">
					<CheckCircle2 aria-hidden="true" size={15} />Additional child content
				</div>
			</Toast.Root>

			<Toast.Root
				variant="info"
				title="Notification without an icon"
				description="Set icon to false when the surrounding context already communicates the status."
				icon={false}
			/>

			<Toast.Root
				variant="warning"
				title="Action without manual dismissal"
				description="The action remains available while the close control is intentionally hidden."
				dismissible={false}
				action={{
					label: "Continue",
					onclick: () => undefined
				}}
			/>
		</div>
	</section>
</Story>

<Story name="Controlled state" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Controlled open state</h2>
			<p class="text-muted m-0 max-w-[38rem] text-sm">
				The bindable open prop and onopenchange callback keep external state synchronized with the dismiss action.
			</p>
		</header>

		<div class="flex flex-wrap items-center gap-3">
			<Button
				variant="outline"
				onclick={() => {
					controlledOpen = true;
					controlledState = "open";
				}}
			>
				Open notification
			</Button>
			<code class="text-muted text-xs font-bold">state: {controlledState}</code>
		</div>

		<div class="w-full max-w-[28rem]">
			<Toast.Root
				bind:open={controlledOpen}
				variant="info"
				priority="assertive"
				title="Controlled notification"
				description="Dismiss this notification and use the button to open it again."
				onopenchange={handleControlledOpenChange}
			/>
		</div>
	</section>
</Story>

<Story name="Programmatic API" asChild>
	<section class="grid gap-7 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Programmatic API</h2>
			<p class="text-muted m-0 max-w-[44rem] text-sm">
				Create notifications from event handlers, configure the viewport, manage a FIFO queue, and dismiss individual or all
				notifications.
			</p>
		</header>

		<div class="grid gap-3">
			<h3 class="text-foreground m-0 text-sm font-black">Creation methods</h3>
			<div class="flex flex-wrap gap-3">
				<Button variant="outline" onclick={showCallableToast}>toast()</Button>
				<Button variant="outline" onclick={showDefaultToast}>toast.show()</Button>
				<Button variant="secondary" onclick={() => showVariantToast("success")}>toast.success()</Button>
				<Button variant="outline" onclick={() => showVariantToast("info")}>toast.info()</Button>
				<Button variant="accent" onclick={() => showVariantToast("warning")}>toast.warning()</Button>
				<Button variant="destructive" onclick={() => showVariantToast("error")}>toast.error()</Button>
				<Button variant="outline" onclick={showConfiguredToast}>Explicit options</Button>
			</div>
		</div>

		<div class="grid gap-3">
			<h3 class="text-foreground m-0 text-sm font-black">Actions and lifecycle</h3>
			<div class="flex flex-wrap gap-3">
				<Button variant="secondary" onclick={showActionToast}>
					<RotateCcw aria-hidden="true" size={17} />With action
				</Button>
				<Button variant="outline" onclick={showPersistentToast}>Persistent</Button>
				<Button variant="ghost" onclick={dismissPersistentToast}>Dismiss by ID</Button>
				<Button onclick={showQueue}>Queue five</Button>
				<Button variant="ghost" onclick={() => Toast.toast.dismiss()}>Dismiss all</Button>
			</div>
		</div>

		<div class="grid gap-3">
			<h3 class="text-foreground m-0 text-sm font-black">Viewport position</h3>
			<div class="grid max-w-2xl grid-cols-2 gap-3 sm:grid-cols-3">
				{#each positions as position (position)}
					<Button variant={selectedPosition === position ? "accent" : "outline"} onclick={() => showPositionToast(position)}>
						{position}
					</Button>
				{/each}
			</div>
		</div>

		<div class="grid gap-3">
			<h3 class="text-foreground m-0 text-sm font-black">Viewport configuration</h3>
			<div class="flex flex-wrap gap-3">
				<Button variant={selectedVisibleToasts === 1 ? "accent" : "outline"} onclick={() => (selectedVisibleToasts = 1)}>
					1 visible
				</Button>
				<Button variant={selectedVisibleToasts === 3 ? "accent" : "outline"} onclick={() => (selectedVisibleToasts = 3)}>
					3 visible
				</Button>
				<Button variant={selectedDuration === 1500 ? "accent" : "outline"} onclick={() => (selectedDuration = 1500)}>
					1.5 s default
				</Button>
				<Button variant={selectedDuration === 5000 ? "accent" : "outline"} onclick={() => (selectedDuration = 5000)}>
					5 s default
				</Button>
				<Button variant={selectedDuration === 0 ? "accent" : "outline"} onclick={() => (selectedDuration = 0)}>
					Persistent default
				</Button>
				<Button
					variant={selectedHotkey === false ? "accent" : "outline"}
					onclick={() => (selectedHotkey = selectedHotkey === false ? "F8" : false)}
				>
					Hotkey {selectedHotkey === false ? "off" : selectedHotkey}
				</Button>
			</div>
		</div>

		<p class="text-muted m-0 max-w-2xl text-sm">
			Hovering, focusing the viewport, or hiding the document pauses active timers. The configured hotkey focuses the viewport;
			Escape dismisses the focused or newest notification.
		</p>

		<Toast.Toaster
			position={selectedPosition}
			duration={selectedDuration}
			visibleToasts={selectedVisibleToasts}
			label="Notification viewport"
			hotkey={selectedHotkey}
			class="sm:max-w-[28rem]"
			data-example="programmatic-viewport"
		/>
	</section>
</Story>

<Story name="Long content" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Long and structured content</h2>
			<p class="text-muted m-0 text-sm">Content wraps without pushing the close control or action outside the viewport.</p>
		</header>

		<div class="w-full max-w-[30rem]">
			<Toast.Root
				variant="info"
				title="A detailed background operation completed with additional information"
				description="This intentionally long description demonstrates how the notification handles multiple lines while preserving a clear reading order and stable controls."
				action={{
					label: "View details",
					onclick: () => undefined
				}}
			>
				<div class="flex items-center gap-2 border-t border-current/25 pt-3 text-xs font-bold">
					<CheckCircle2 aria-hidden="true" size={15} />Last updated just now
				</div>
			</Toast.Root>
		</div>
	</section>
</Story>

<Story name="Mobile width" asChild>
	<section class="grid gap-5 p-4">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Mobile width</h2>
			<p class="text-muted m-0 max-w-[22rem] text-sm">
				Text, action, and close control remain readable without horizontal overflow at a narrow viewport.
			</p>
		</header>

		<div class="w-full max-w-[22rem]">
			<Toast.Root
				variant="warning"
				title="Connection interrupted"
				description="Your changes remain available while the application attempts to reconnect."
				action={{
					label: "Try again",
					onclick: () => undefined
				}}
			/>
		</div>
	</section>
</Story>
