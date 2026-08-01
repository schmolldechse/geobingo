<script module lang="ts">
	import Bell from "@lucide/svelte/icons/bell";
	import ChartNoAxesCombined from "@lucide/svelte/icons/chart-no-axes-combined";
	import Folder from "@lucide/svelte/icons/folder";
	import UserRound from "@lucide/svelte/icons/user-round";
	import { defineMeta } from "@storybook/addon-svelte-csf";
	import type { ComponentProps } from "svelte";

	import * as Tabs from "../../lib/components/ui/tabs/index";
	import type { TabsActivationMode, TabsOrientation } from "../../lib/components/ui/tabs/index";
	import TabsControlledExample from "./TabsControlledExample.svelte";

	type PlaygroundArgs = Omit<ComponentProps<typeof Tabs.Root>, "children" | "onvaluechange">;

	const orientations: TabsOrientation[] = ["horizontal", "vertical"];
	const activationModes: TabsActivationMode[] = ["automatic", "manual"];

	const { Story } = defineMeta({
		title: "UI/Tabs",
		component: Tabs.Root,
		render: playground,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen"
		},
		args: {
			value: "overview",
			orientation: "horizontal",
			activationMode: "automatic",
			loop: true,
			disabled: false
		},
		argTypes: {
			value: {
				control: "select",
				options: ["overview", "activity", "settings"],
				description: "The value of the active tab"
			},
			orientation: {
				control: "select",
				options: orientations,
				description: "The visual orientation and arrow-key direction"
			},
			activationMode: {
				control: "select",
				options: activationModes,
				description: "Whether focus activates a tab automatically or requires confirmation"
			},
			loop: {
				control: "boolean",
				description: "Wrap keyboard focus from the last tab to the first and back"
			},
			disabled: {
				control: "boolean",
				description: "Disable every trigger in the tab set"
			}
		}
	});
</script>

{#snippet panel(title: string, description: string)}
	<div class="border-border bg-surface grid gap-2 rounded-2xl border-2 p-5 shadow-[var(--shadow-paper)]">
		<h3 class="text-foreground m-0 text-lg font-black">{title}</h3>
		<p class="text-muted m-0 text-sm leading-6">{description}</p>
	</div>
{/snippet}

{#snippet standardTabs()}
	<Tabs.List aria-label="Workspace sections">
		<Tabs.Trigger value="overview">Overview</Tabs.Trigger>
		<Tabs.Trigger value="activity">Activity</Tabs.Trigger>
		<Tabs.Trigger value="settings">Settings</Tabs.Trigger>
	</Tabs.List>
	<Tabs.Content value="overview">
		{@render panel("Overview", "A concise summary of the current workspace and its most important information.")}
	</Tabs.Content>
	<Tabs.Content value="activity">
		{@render panel("Activity", "Recent changes, updates, and events appear in this panel.")}
	</Tabs.Content>
	<Tabs.Content value="settings">
		{@render panel("Settings", "Preferences and configuration options can be composed inside any tab panel.")}
	</Tabs.Content>
{/snippet}

{#snippet playground(args: PlaygroundArgs)}
	<section class="p-6 sm:p-8">
		<div class="mx-auto max-w-4xl">
			<Tabs.Root {...args}>
				{@render standardTabs()}
			</Tabs.Root>
		</div>
	</section>
{/snippet}

<Story name="Playground" />

<Story name="Icons and Badges" asChild>
	<section class="p-6 sm:p-8">
		<div class="mx-auto max-w-4xl">
			<Tabs.Root value="notifications">
				<Tabs.List aria-label="Account sections">
					<Tabs.Trigger value="profile"><UserRound aria-hidden="true" size={18} />Profile</Tabs.Trigger>
					<Tabs.Trigger value="notifications">
						<Bell aria-hidden="true" size={18} />
						Notifications
						<span
							aria-hidden="true"
							class="bg-surface-muted text-foreground inline-flex min-w-6 items-center justify-center rounded-full px-1.5 py-0.5 text-xs font-black"
						>
							3
						</span>
						<span class="sr-only">3 unread</span>
					</Tabs.Trigger>
					<Tabs.Trigger value="reports">
						<ChartNoAxesCombined aria-hidden="true" size={18} />Reports
					</Tabs.Trigger>
					<Tabs.Trigger value="archive" disabled><Folder aria-hidden="true" size={18} />Archive</Tabs.Trigger>
				</Tabs.List>
				<Tabs.Content value="profile">
					{@render panel("Profile", "Personal details and account information.")}
				</Tabs.Content>
				<Tabs.Content value="notifications">
					{@render panel("Notifications", "Three unread notifications are waiting for review.")}
				</Tabs.Content>
				<Tabs.Content value="reports">
					{@render panel("Reports", "Charts, metrics, and downloadable reports.")}
				</Tabs.Content>
				<Tabs.Content value="archive">
					{@render panel("Archive", "This section is currently unavailable.")}
				</Tabs.Content>
			</Tabs.Root>
		</div>
	</section>
</Story>

<Story name="Orientations" asChild>
	<section class="grid gap-8 p-6 lg:grid-cols-2 lg:p-8">
		<div class="grid content-start gap-3">
			<div>
				<h2 class="text-foreground m-0 text-xl font-black">Horizontal</h2>
				<p class="text-muted mt-1 mb-0 text-sm">Use Left and Right Arrow to move between tabs.</p>
			</div>
			<Tabs.Root value="summary">
				<Tabs.List aria-label="Horizontal example">
					<Tabs.Trigger value="summary">Summary</Tabs.Trigger>
					<Tabs.Trigger value="documents">Documents</Tabs.Trigger>
					<Tabs.Trigger value="preferences">Preferences</Tabs.Trigger>
				</Tabs.List>
				<Tabs.Content value="summary">
					{@render panel("Summary", "Horizontal tabs place the active indicator below the trigger.")}
				</Tabs.Content>
				<Tabs.Content value="documents">
					{@render panel("Documents", "Files and written content belong here.")}
				</Tabs.Content>
				<Tabs.Content value="preferences">
					{@render panel("Preferences", "Configure the example from this panel.")}
				</Tabs.Content>
			</Tabs.Root>
		</div>

		<div class="grid content-start gap-3">
			<div>
				<h2 class="text-foreground m-0 text-xl font-black">Vertical</h2>
				<p class="text-muted mt-1 mb-0 text-sm">Use Up and Down Arrow to move between tabs.</p>
			</div>
			<Tabs.Root value="summary" orientation="vertical">
				<Tabs.List aria-label="Vertical example">
					<Tabs.Trigger value="summary">Summary</Tabs.Trigger>
					<Tabs.Trigger value="documents">Documents</Tabs.Trigger>
					<Tabs.Trigger value="preferences">Preferences</Tabs.Trigger>
				</Tabs.List>
				<Tabs.Content value="summary">
					{@render panel("Summary", "Vertical tabs use a side indicator and a responsive two-column layout.")}
				</Tabs.Content>
				<Tabs.Content value="documents">
					{@render panel("Documents", "The panel remains aligned with the vertical list on larger screens.")}
				</Tabs.Content>
				<Tabs.Content value="preferences">
					{@render panel("Preferences", "On mobile, this panel moves below the list.")}
				</Tabs.Content>
			</Tabs.Root>
		</div>
	</section>
</Story>

<Story name="Activation Modes" asChild>
	<section class="grid gap-8 p-6 lg:grid-cols-2 lg:p-8">
		<div class="grid content-start gap-3">
			<div>
				<h2 class="text-foreground m-0 text-xl font-black">Automatic activation</h2>
				<p class="text-muted mt-1 mb-0 text-sm">Moving focus with an arrow key immediately opens the matching panel.</p>
			</div>
			<Tabs.Root value="summary" activationMode="automatic">
				<Tabs.List aria-label="Automatic activation example">
					<Tabs.Trigger value="summary">Summary</Tabs.Trigger>
					<Tabs.Trigger value="details">Details</Tabs.Trigger>
					<Tabs.Trigger value="history">History</Tabs.Trigger>
				</Tabs.List>
				<Tabs.Content value="summary">
					{@render panel("Summary", "This panel follows keyboard focus automatically.")}
				</Tabs.Content>
				<Tabs.Content value="details">
					{@render panel("Details", "Focus this tab to activate it.")}
				</Tabs.Content>
				<Tabs.Content value="history">
					{@render panel("History", "No additional confirmation is required.")}
				</Tabs.Content>
			</Tabs.Root>
		</div>

		<div class="grid content-start gap-3">
			<div>
				<h2 class="text-foreground m-0 text-xl font-black">Manual activation</h2>
				<p class="text-muted mt-1 mb-0 text-sm">Move focus first, then press Enter or Space to open the panel.</p>
			</div>
			<Tabs.Root value="summary" activationMode="manual">
				<Tabs.List aria-label="Manual activation example">
					<Tabs.Trigger value="summary">Summary</Tabs.Trigger>
					<Tabs.Trigger value="details">Details</Tabs.Trigger>
					<Tabs.Trigger value="history">History</Tabs.Trigger>
				</Tabs.List>
				<Tabs.Content value="summary">
					{@render panel("Summary", "Arrow keys move focus without changing this panel.")}
				</Tabs.Content>
				<Tabs.Content value="details">
					{@render panel("Details", "Press Enter or Space while this tab has focus.")}
				</Tabs.Content>
				<Tabs.Content value="history">
					{@render panel("History", "Manual activation separates focus from selection.")}
				</Tabs.Content>
			</Tabs.Root>
		</div>
	</section>
</Story>

<Story name="Disabled States" asChild>
	<section class="grid gap-8 p-6 lg:grid-cols-2 lg:p-8">
		<div class="grid content-start gap-3">
			<div>
				<h2 class="text-foreground m-0 text-xl font-black">Disabled trigger</h2>
				<p class="text-muted mt-1 mb-0 text-sm">Keyboard navigation skips unavailable tabs.</p>
			</div>
			<Tabs.Root value="general">
				<Tabs.List aria-label="Partially disabled example">
					<Tabs.Trigger value="general">General</Tabs.Trigger>
					<Tabs.Trigger value="billing" disabled>Billing</Tabs.Trigger>
					<Tabs.Trigger value="security">Security</Tabs.Trigger>
				</Tabs.List>
				<Tabs.Content value="general">
					{@render panel("General", "This trigger and panel remain available.")}
				</Tabs.Content>
				<Tabs.Content value="billing">
					{@render panel("Billing", "This panel cannot be selected while its trigger is disabled.")}
				</Tabs.Content>
				<Tabs.Content value="security">
					{@render panel("Security", "Navigation moves directly from General to Security.")}
				</Tabs.Content>
			</Tabs.Root>
		</div>

		<div class="grid content-start gap-3">
			<div>
				<h2 class="text-foreground m-0 text-xl font-black">Disabled tab set</h2>
				<p class="text-muted mt-1 mb-0 text-sm">The active panel stays visible while every trigger is unavailable.</p>
			</div>
			<Tabs.Root value="general" disabled>
				<Tabs.List aria-label="Fully disabled example">
					<Tabs.Trigger value="general">General</Tabs.Trigger>
					<Tabs.Trigger value="billing">Billing</Tabs.Trigger>
					<Tabs.Trigger value="security">Security</Tabs.Trigger>
				</Tabs.List>
				<Tabs.Content value="general">
					{@render panel("General", "The current content remains readable in the disabled state.")}
				</Tabs.Content>
				<Tabs.Content value="billing">
					{@render panel("Billing", "This panel is inactive.")}
				</Tabs.Content>
				<Tabs.Content value="security">
					{@render panel("Security", "This panel is inactive.")}
				</Tabs.Content>
			</Tabs.Root>
		</div>
	</section>
</Story>

<Story name="Controlled Value" asChild>
	<section class="p-6 sm:p-8">
		<TabsControlledExample />
	</section>
</Story>

<Story name="Responsive Overflow" asChild>
	<section class="grid gap-8 p-6 lg:grid-cols-[20rem_minmax(0,1fr)] lg:p-8">
		<div class="grid content-start gap-3">
			<div>
				<h2 class="text-foreground m-0 text-xl font-black">Mobile width</h2>
				<p class="text-muted mt-1 mb-0 text-sm">Swipe or scroll horizontally without squeezing the labels.</p>
			</div>
			<div class="border-foreground bg-background overflow-hidden rounded-2xl border-2 p-3 shadow-[var(--shadow-paper)]">
				<Tabs.Root value="overview">
					<Tabs.List aria-label="Mobile overflow example">
						<Tabs.Trigger value="overview">Overview</Tabs.Trigger>
						<Tabs.Trigger value="recent">Recent activity</Tabs.Trigger>
						<Tabs.Trigger value="documents">Shared documents</Tabs.Trigger>
						<Tabs.Trigger value="analytics">Analytics</Tabs.Trigger>
						<Tabs.Trigger value="members">Team members</Tabs.Trigger>
						<Tabs.Trigger value="settings">Workspace settings</Tabs.Trigger>
					</Tabs.List>
					<Tabs.Content value="overview">
						{@render panel("Overview", "The tab list remains a single scrollable row at mobile width.")}
					</Tabs.Content>
					<Tabs.Content value="recent">{@render panel("Recent activity", "Recent updates appear here.")}</Tabs.Content>
					<Tabs.Content value="documents">
						{@render panel("Shared documents", "Shared files appear here.")}
					</Tabs.Content>
					<Tabs.Content value="analytics">{@render panel("Analytics", "Metrics appear here.")}</Tabs.Content>
					<Tabs.Content value="members">{@render panel("Team members", "Member details appear here.")}</Tabs.Content>
					<Tabs.Content value="settings">
						{@render panel("Workspace settings", "Workspace preferences appear here.")}
					</Tabs.Content>
				</Tabs.Root>
			</div>
		</div>

		<div class="grid content-start gap-3">
			<div>
				<h2 class="text-foreground m-0 text-xl font-black">Desktop width</h2>
				<p class="text-muted mt-1 mb-0 text-sm">The same component expands naturally when more space is available.</p>
			</div>
			<Tabs.Root value="overview">
				<Tabs.List aria-label="Desktop overflow example">
					<Tabs.Trigger value="overview">Overview</Tabs.Trigger>
					<Tabs.Trigger value="recent">Recent activity</Tabs.Trigger>
					<Tabs.Trigger value="documents">Shared documents</Tabs.Trigger>
					<Tabs.Trigger value="analytics">Analytics</Tabs.Trigger>
					<Tabs.Trigger value="members">Team members</Tabs.Trigger>
					<Tabs.Trigger value="settings">Workspace settings</Tabs.Trigger>
				</Tabs.List>
				<Tabs.Content value="overview">
					{@render panel("Overview", "The full-width list preserves the same interaction and visual hierarchy.")}
				</Tabs.Content>
				<Tabs.Content value="recent">{@render panel("Recent activity", "Recent updates appear here.")}</Tabs.Content>
				<Tabs.Content value="documents">{@render panel("Shared documents", "Shared files appear here.")}</Tabs.Content>
				<Tabs.Content value="analytics">{@render panel("Analytics", "Metrics appear here.")}</Tabs.Content>
				<Tabs.Content value="members">{@render panel("Team members", "Member details appear here.")}</Tabs.Content>
				<Tabs.Content value="settings">
					{@render panel("Workspace settings", "Workspace preferences appear here.")}
				</Tabs.Content>
			</Tabs.Root>
		</div>
	</section>
</Story>
