<script module lang="ts">
	import { defineMeta } from "@storybook/addon-svelte-csf";

	import Avatar from "../lib/components/ui/Avatar.svelte";

	const { Story } = defineMeta({
		title: "UI/Avatar",
		component: Avatar,
		tags: ["autodocs"],
		parameters: {
			layout: "fullscreen"
		}
	});
</script>

<script lang="ts">
	const avatarImage = `data:image/svg+xml;charset=utf-8,${encodeURIComponent(`
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 128 128">
			<rect width="128" height="128" fill="#2f5368" />
			<circle cx="64" cy="46" r="25" fill="#f1c84b" />
			<path d="M18 128c4-35 21-52 46-52s42 17 46 52" fill="#f1c84b" />
		</svg>
	`)}`;
	const brokenImage = "/storybook/avatar-image-does-not-exist.png";

	let changingSource: string | undefined = $state(brokenImage);

	function showImage(): void {
		changingSource = avatarImage;
	}

	function showError(): void {
		changingSource = `${brokenImage}?retry=${Date.now()}`;
	}
</script>

<Story name="Zustände" asChild>
	<section class="grid gap-6 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Bild- und Fallback-Zustände</h2>
			<p class="text-muted m-0 text-sm">Geladenes Bild, fehlende Quelle, Leerwert und Ladefehler.</p>
		</header>

		<div class="border-border bg-surface flex flex-wrap gap-7 rounded-2xl border-2 p-6">
			<div class="grid justify-items-center gap-2">
				<Avatar src={avatarImage} alt="Beispielprofil" class="border-foreground size-12 border-2 font-black" />
				<span class="text-muted text-xs font-bold">Geladen</span>
			</div>
			<div class="grid justify-items-center gap-2">
				<Avatar src={undefined} alt="" class="border-foreground size-12 border-2 font-black" />
				<span class="text-muted text-xs font-bold">Keine Quelle</span>
			</div>
			<div class="grid justify-items-center gap-2">
				<Avatar src="   " alt="" class="border-foreground size-12 border-2 font-black" />
				<span class="text-muted text-xs font-bold">Leerwert</span>
			</div>
			<div class="grid justify-items-center gap-2">
				<Avatar src={brokenImage} alt="" class="border-foreground size-12 border-2 font-black">
					{#snippet fallback()}<span aria-hidden="true">EF</span>{/snippet}
				</Avatar>
				<span class="text-muted text-xs font-bold">Fehler</span>
			</div>
		</div>
	</section>
</Story>

<Story name="Fallback-Snippet" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Anpassbare Fallbacks</h2>
			<p class="text-muted m-0 text-sm">Der Standard kann mit beliebigem Snippet-Inhalt ersetzt werden.</p>
		</header>

		<div class="border-border bg-surface flex flex-wrap items-end gap-6 rounded-2xl border-2 p-6">
			<div class="grid justify-items-center gap-2">
				<Avatar src={null} alt="" class="border-foreground size-12 border-2 text-lg font-black" />
				<span class="text-muted text-xs font-bold">Standard</span>
			</div>
			<div class="grid justify-items-center gap-2">
				<Avatar src={null} alt="" class="border-foreground size-12 border-2 font-[Fredoka_Variable] font-[650]">
					{#snippet fallback()}<span aria-hidden="true">CD</span>{/snippet}
				</Avatar>
				<span class="text-muted text-xs font-bold">Initialen</span>
			</div>
			<div class="grid justify-items-center gap-2">
				<Avatar src={null} alt="" aria-label="Anonymes Profil" class="bg-secondary text-secondary-foreground size-12">
					{#snippet fallback()}<span aria-hidden="true">●</span>{/snippet}
				</Avatar>
				<span class="text-muted text-xs font-bold">Eigenes Symbol</span>
			</div>
		</div>
	</section>
</Story>

<Story name="Größen und Responsive" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Consumer-gesteuerte Größen</h2>
			<p class="text-muted m-0 text-sm">Die Fundstellen bestimmen Größe, Rahmen und responsive Breakpoints.</p>
		</header>

		<div class="border-border bg-surface flex flex-wrap items-end gap-6 rounded-2xl border-2 p-6">
			{#each ["size-8", "size-10", "size-11"] as sizeClass}
				<div class="grid justify-items-center gap-2">
					<Avatar src={avatarImage} alt="" class={["border-foreground border-2", sizeClass]} />
					<span class="text-muted text-xs font-bold">{sizeClass}</span>
				</div>
			{/each}
			<div class="grid justify-items-center gap-2">
				<Avatar
					src={avatarImage}
					alt=""
					class="border-foreground size-[42px] border-2 min-[621px]:size-[54px] min-[901px]:size-[62px]"
				/>
				<span class="text-muted text-xs font-bold">Responsive 42–62 px</span>
			</div>
		</div>
	</section>
</Story>

<Story name="Laden und erneut versuchen" asChild>
	<section class="grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="text-foreground m-0 text-xl font-black">Quellenwechsel</h2>
			<p class="text-muted m-0 text-sm">Ein Wechsel der Quelle startet die Zustandsauflösung erneut.</p>
		</header>

		<div class="border-border bg-surface flex max-w-md items-center gap-4 rounded-2xl border-2 p-6">
			<Avatar src={changingSource} alt="Wechselndes Beispielprofil" loading="eager" class="border-foreground size-14 border-2">
				{#snippet fallback()}<span aria-hidden="true">LW</span>{/snippet}
			</Avatar>
			<div class="grid gap-2">
				<div class="flex flex-wrap gap-2">
					<button
						type="button"
						class="border-foreground bg-secondary text-secondary-foreground min-h-9 rounded-lg border-2 px-3 text-xs font-black"
						onclick={showImage}>Bild laden</button
					>
					<button
						type="button"
						class="border-foreground bg-surface min-h-9 rounded-lg border-2 px-3 text-xs font-black"
						onclick={showError}>Fehler auslösen</button
					>
				</div>
				<p class="text-muted m-0 text-xs">Eager Loading mit beschreibendem Alt-Text.</p>
			</div>
		</div>
	</section>
</Story>

<Story name="Dunkles Farbschema" asChild>
	<section data-theme="dark" class="bg-background text-foreground grid gap-5 p-6 sm:p-8">
		<header class="grid gap-1">
			<h2 class="m-0 text-xl font-black">GeoBingo Dark Theme</h2>
			<p class="text-muted m-0 text-sm">Semantische Tokens passen Bildrahmen und Fallback automatisch an.</p>
		</header>

		<div class="border-border bg-surface flex flex-wrap gap-5 rounded-2xl border-2 p-6">
			<Avatar src={avatarImage} alt="" class="border-foreground size-12 border-2" />
			<Avatar src={null} alt="" class="border-foreground size-12 border-2 font-black">
				{#snippet fallback()}<span aria-hidden="true">DK</span>{/snippet}
			</Avatar>
			<Avatar src={null} alt="" class="bg-secondary text-secondary-foreground size-12 font-black">
				{#snippet fallback()}<span aria-hidden="true">GB</span>{/snippet}
			</Avatar>
		</div>
	</section>
</Story>
