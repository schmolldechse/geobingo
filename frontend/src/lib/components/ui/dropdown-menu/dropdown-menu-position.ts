import type { DropdownMenuAlign } from "./dropdown-menu-context.svelte";

type DropdownMenuSide = "auto" | "bottom" | "top";
type DropdownMenuResolvedSide = Exclude<DropdownMenuSide, "auto">;

type DropdownMenuRect = {
	top: number;
	right: number;
	bottom: number;
	left: number;
	width: number;
	height: number;
};

type DropdownMenuSize = {
	width: number;
	height: number;
};

type DropdownMenuPositionInput = {
	triggerRect: DropdownMenuRect;
	contentSize: DropdownMenuSize;
	viewportSize: DropdownMenuSize;
	align: DropdownMenuAlign;
	side: DropdownMenuSide;
	sideOffset: number;
	collisionPadding: number;
	maximumHeight: number;
};

type DropdownMenuPosition = {
	side: DropdownMenuResolvedSide;
	top: number;
	left: number;
	availableHeight: number;
};

const clamp = (value: number, minimum: number, maximum: number): number =>
	Math.min(Math.max(value, minimum), Math.max(minimum, maximum));

const getDropdownMenuPosition = ({
	triggerRect,
	contentSize,
	viewportSize,
	align,
	side,
	sideOffset,
	collisionPadding,
	maximumHeight
}: DropdownMenuPositionInput): DropdownMenuPosition => {
	const safeOffset = Math.max(0, sideOffset);
	const safePadding = Math.max(0, collisionPadding);
	const safeMaximumHeight = Math.max(0, maximumHeight);
	const availableBelow = Math.max(0, viewportSize.height - safePadding - triggerRect.bottom - safeOffset);
	const availableAbove = Math.max(0, triggerRect.top - safePadding - safeOffset);
	const resolvedSide: DropdownMenuResolvedSide =
		side === "auto" ? (contentSize.height <= availableBelow || availableBelow >= availableAbove ? "bottom" : "top") : side;
	const availableHeight = resolvedSide === "bottom" ? availableBelow : availableAbove;
	const visibleHeight = Math.min(contentSize.height, availableHeight, safeMaximumHeight);

	const desiredLeft =
		align === "end"
			? triggerRect.right - contentSize.width
			: align === "center"
				? triggerRect.left + (triggerRect.width - contentSize.width) / 2
				: triggerRect.left;
	const left = clamp(desiredLeft, safePadding, viewportSize.width - safePadding - contentSize.width);
	const top =
		resolvedSide === "bottom"
			? triggerRect.bottom + safeOffset
			: Math.max(safePadding, triggerRect.top - safeOffset - visibleHeight);

	return {
		side: resolvedSide,
		top,
		left,
		availableHeight
	};
};

export {
	getDropdownMenuPosition,
	type DropdownMenuPosition,
	type DropdownMenuPositionInput,
	type DropdownMenuResolvedSide,
	type DropdownMenuSide
};
