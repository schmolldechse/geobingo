<script lang="ts">
	import { goto } from "$app/navigation";
	import { page } from "$app/state";
	import ErrorScreen from "$lib/components/ErrorScreen.svelte";

	const goBack = () => {
		if (window.history.length > 1) {
			window.history.back();
			return;
		}

		void goto("/");
	};
</script>

<svelte:head>
	<title>{page.status} – GeoBingo</title>
	<meta name="description" content={page.error?.message} />
</svelte:head>

<ErrorScreen
	status={page.status}
	message={page.error?.message ?? "An unexpected error occurred."}
	actionLabel="Go back"
	onaction={goBack}
/>
