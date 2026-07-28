import type { Session } from "$lib/generated/api";

declare global {
	namespace App {
		interface Locals {
			session: Session;
		}
	}
}

export {};
