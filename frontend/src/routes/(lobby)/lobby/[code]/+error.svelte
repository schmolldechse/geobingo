<script lang="ts">
	import { page } from "$app/state";
	import ErrorScreen from "$lib/components/ErrorScreen.svelte";
</script>

<svelte:head>
	<title>{page.status} – GeoBingo</title>
	<meta name="description" content={page.error?.message} />
</svelte:head>

<ErrorScreen
	status={page.status}
	message={page.error?.message ?? "An unexpected lobby error occurred."}
	actionLabel="Back to home"
	actionHref="/"
/>
