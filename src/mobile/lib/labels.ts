import type { AccountType, EventStatus, OrderStatus } from "@/lib/types";

export function orderStatusLabel(status: OrderStatus): string {
  switch (status) {
    case "Placed":
      return "Złożone";
    case "Closed":
      return "Zamknięte";
    case "Cancelled":
      return "Anulowane";
  }
}

export function eventStatusLabel(status: EventStatus): string {
  switch (status) {
    case "Draft":
      return "Szkic";
    case "Published":
      return "Opublikowane";
    case "Closed":
      return "Zakończone";
    case "Cancelled":
      return "Anulowane";
  }
}

export function accountTypeLabel(accountType: AccountType): string {
  return accountType === "Staff" ? "Personel" : "Gość";
}
