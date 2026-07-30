type ToastLayout = "compact" | "rich";

type ToastLayoutContent = {
	description?: unknown;
	action?: unknown;
	children?: unknown;
};

export const resolveToastLayout = ({ description, action, children }: ToastLayoutContent): ToastLayout =>
	description === undefined && action === undefined && children === undefined ? "compact" : "rich";
