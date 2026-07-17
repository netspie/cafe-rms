import { create } from "zustand";

export type OrderLineModifier = { id: string; name: string };

export type OrderLine = {
  key: string;
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  modifiers: OrderLineModifier[];
};

type OrderState = {
  lines: OrderLine[];
  add: (line: Omit<OrderLine, "key">) => void;
  setQuantity: (key: string, quantity: number) => void;
  remove: (key: string) => void;
  clear: () => void;
  totalCount: () => number;
  totalGross: () => number;
};

function lineKey(productId: string, modifiers: OrderLineModifier[]): string {
  const ids = modifiers.map((m) => m.id).sort();
  return ids.length > 0 ? `${productId}|${ids.join(",")}` : productId;
}

export const useOrderStore = create<OrderState>((set, get) => ({
  lines: [],
  add: (line) =>
    set((state) => {
      const key = lineKey(line.productId, line.modifiers);
      const existing = state.lines.find((l) => l.key === key);
      if (existing) {
        return {
          lines: state.lines.map((l) =>
            l.key === key ? { ...l, quantity: l.quantity + line.quantity } : l,
          ),
        };
      }
      return { lines: [...state.lines, { ...line, key }] };
    }),
  setQuantity: (key, quantity) =>
    set((state) => ({
      lines:
        quantity <= 0
          ? state.lines.filter((l) => l.key !== key)
          : state.lines.map((l) => (l.key === key ? { ...l, quantity } : l)),
    })),
  remove: (key) =>
    set((state) => ({
      lines: state.lines.filter((l) => l.key !== key),
    })),
  clear: () => set({ lines: [] }),
  totalCount: () => get().lines.reduce((sum, l) => sum + l.quantity, 0),
  totalGross: () =>
    get().lines.reduce((sum, l) => sum + l.unitPrice * l.quantity, 0),
}));
