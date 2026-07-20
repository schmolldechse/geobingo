import type { Session } from "$lib/api/generated/rest";

declare global {
	namespace App {
		interface Locals {
			session: Session;
		}
	}
}

export {};
