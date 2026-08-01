<script lang="ts">
	import FileText from "@lucide/svelte/icons/file-text";
	import Settings from "@lucide/svelte/icons/settings";
	import UserRound from "@lucide/svelte/icons/user-round";

	import Button from "@/lib/components/ui/Button.svelte";
	import * as Tabs from "@/lib/components/ui/tabs/index";

	let value = $state("profile");
	let lastChange = $state("None");
</script>

{#snippet panel(title: string, description: string)}
	<div class="border-border bg-surface grid gap-2 rounded-2xl border-2 p-5 shadow-[var(--shadow-paper)]">
		<h3 class="text-foreground m-0 text-lg font-black">{title}</h3>
		<p class="text-muted m-0 text-sm leading-6">{description}</p>
	</div>
{/snippet}

<div class="mx-auto grid max-w-4xl gap-5">
	<header class="grid gap-1">
		<h2 class="text-foreground m-0 text-xl font-black">Externally controlled selection</h2>
		<p class="text-muted m-0 text-sm">Use the buttons or the tab triggers to update the same bound value.</p>
	</header>

	<div class="flex flex-wrap gap-3">
		<Button size="sm" variant={value === "profile" ? "primary" : "outline"} onclick={() => (value = "profile")}>
			Show profile
		</Button>
		<Button size="sm" variant={value === "documents" ? "primary" : "outline"} onclick={() => (value = "documents")}>
			Show documents
		</Button>
		<Button size="sm" variant={value === "settings" ? "primary" : "outline"} onclick={() => (value = "settings")}>
			Show settings
		</Button>
	</div>

	<div class="border-border bg-surface-muted flex flex-wrap gap-x-6 gap-y-1 rounded-xl border px-4 py-3 text-sm font-bold">
		<output>Active value: <strong>{value}</strong></output>
		<output>Last tab event: <strong>{lastChange}</strong></output>
	</div>

	<Tabs.Root bind:value onvaluechange={(nextValue) => (lastChange = nextValue)}>
		<Tabs.List aria-label="Controlled example">
			<Tabs.Trigger value="profile"><UserRound aria-hidden="true" size={18} />Profile</Tabs.Trigger>
			<Tabs.Trigger value="documents"><FileText aria-hidden="true" size={18} />Documents</Tabs.Trigger>
			<Tabs.Trigger value="settings"><Settings aria-hidden="true" size={18} />Settings</Tabs.Trigger>
		</Tabs.List>
		<Tabs.Content value="profile">
			{@render panel("Profile", "The bound value currently selects the profile panel.")}
		</Tabs.Content>
		<Tabs.Content value="documents">
			{@render panel("Documents", "External controls can select this panel programmatically.")}
		</Tabs.Content>
		<Tabs.Content value="settings">
			{@render panel("Settings", "Trigger interactions also invoke the change callback.")}
		</Tabs.Content>
	</Tabs.Root>
</div>
