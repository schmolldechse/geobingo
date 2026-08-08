<script lang="ts">
	import type { Snippet } from "svelte";
	import type { ClassValue, HTMLAttributes, HTMLImgAttributes } from "svelte/elements";

	type AvatarStatus = "fallback" | "loading" | "loaded" | "error";

	type Props = {
		src?: string | null;
		alt: string;
		fallback?: Snippet;
		class?: ClassValue;
		loading?: HTMLImgAttributes["loading"];
		decoding?: HTMLImgAttributes["decoding"];
		referrerpolicy?: HTMLImgAttributes["referrerpolicy"];
	} & Omit<HTMLAttributes<HTMLDivElement>, "children" | "class">;

	let {
		src,
		alt,
		fallback,
		class: className,
		loading = "lazy",
		decoding = "async",
		referrerpolicy = "no-referrer",
		...restProps
	}: Props = $props();

	let imageElement = $state<HTMLImageElement | null>(null);
	let loadedImage = $state<HTMLImageElement | null>(null);
	let failedImage = $state<HTMLImageElement | null>(null);

	const normalizedSource = $derived(src?.trim() || null);
	const status = $derived.by((): AvatarStatus => {
		if (!normalizedSource) return "fallback";
		if (imageElement && failedImage === imageElement) return "error";
		if (imageElement && loadedImage === imageElement) return "loaded";
		return "loading";
	});

	function markLoaded(event: Event): void {
		const image = event.currentTarget as HTMLImageElement;
		if (image !== imageElement) return;
		loadedImage = image;
		if (failedImage === image) failedImage = null;
	}

	function markFailed(event: Event): void {
		const image = event.currentTarget as HTMLImageElement;
		if (image !== imageElement) return;
		failedImage = image;
		if (loadedImage === image) loadedImage = null;
	}
</script>

<div
	{...restProps}
	data-avatar-root
	data-status={status}
	class={["bg-accent text-accent-foreground relative grid shrink-0 place-items-center overflow-hidden rounded-full", className]}
>
	<span
		data-avatar-fallback
		data-status={status}
		aria-hidden={status === "loaded" ? "true" : undefined}
		class={[
			"col-start-1 row-start-1 grid size-full place-items-center transition-opacity duration-150 motion-reduce:transition-none",
			status === "loaded" && "opacity-0"
		]}
	>
		{#if fallback}
			{@render fallback()}
		{:else}
			<span aria-hidden="true">?</span>
		{/if}
	</span>

	{#key normalizedSource}
		{@const renderedSource = normalizedSource}
		{#if renderedSource}
			<img
				bind:this={imageElement}
				data-avatar-image
				data-status={status}
				src={renderedSource}
				{alt}
				{loading}
				{decoding}
				{referrerpolicy}
				aria-hidden={status === "loaded" ? undefined : "true"}
				class={[
					"absolute inset-0 size-full object-cover opacity-0 transition-opacity duration-150 motion-reduce:transition-none",
					status === "loaded" && "opacity-100"
				]}
				onload={markLoaded}
				onerror={markFailed}
			/>
		{/if}
	{/key}
</div>
