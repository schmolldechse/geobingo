<script module lang="ts">
	type LoadingDotsSize = "sm" | "md" | "lg";
	type LoadingDotsVariant = "brand" | "current";

	export type { LoadingDotsSize, LoadingDotsVariant };
</script>

<script lang="ts">
	import type { ClassValue, HTMLAttributes } from "svelte/elements";

	type Props = {
		size?: LoadingDotsSize;
		variant?: LoadingDotsVariant;
		class?: ClassValue;
	} & Omit<HTMLAttributes<HTMLSpanElement>, "aria-hidden" | "class" | "role">;

	let { size = "md", variant = "brand", class: className, ...restProps }: Props = $props();
</script>

<span
	{...restProps}
	aria-hidden="true"
	data-loading-dots
	data-size={size}
	data-variant={variant}
	class={["loading-dots inline-flex items-center", className]}
>
	<span class="loading-dot loading-dot-primary"></span>
	<span class="loading-dot loading-dot-secondary"></span>
	<span class="loading-dot loading-dot-accent"></span>
</span>

<style>
	.loading-dots {
		--loading-dot-size: 0.5rem;
		--loading-dot-gap: 0.45rem;

		gap: var(--loading-dot-gap);
	}

	.loading-dots[data-size="sm"] {
		--loading-dot-size: 0.375rem;
		--loading-dot-gap: 0.3rem;
	}

	.loading-dots[data-size="lg"] {
		--loading-dot-size: 0.75rem;
		--loading-dot-gap: 0.6rem;
	}

	.loading-dot {
		width: var(--loading-dot-size);
		aspect-ratio: 1;
		border-radius: 9999px;
		animation: loading-dot-bounce var(--loading-dots-duration, 1.25s) ease-in-out infinite;
	}

	.loading-dot-secondary {
		animation-delay: 0.14s;
	}

	.loading-dot-accent {
		animation-delay: 0.28s;
	}

	.loading-dots[data-variant="brand"] .loading-dot-primary {
		background: var(--primary);
	}

	.loading-dots[data-variant="brand"] .loading-dot-secondary {
		background: var(--secondary);
	}

	.loading-dots[data-variant="brand"] .loading-dot-accent {
		background: var(--accent);
	}

	.loading-dots[data-variant="current"] .loading-dot {
		background: currentColor;
	}

	@keyframes loading-dot-bounce {
		0%,
		60%,
		100% {
			opacity: 0.45;
			transform: translateY(0);
		}

		30% {
			opacity: 1;
			transform: translateY(calc(var(--loading-dot-size) * -0.6));
		}
	}

	@media (prefers-reduced-motion: reduce) {
		.loading-dot {
			animation: none;
			opacity: 1;
			transform: none;
		}
	}
</style>
