import { create } from "zustand";

export type OrderLine = {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  priceGroupId: string | null;
};

type OrderState = {
  lines: OrderLine[];
  add: (line: OrderLine) => void;
  setQuantity: (productId: string, quantity: number) => void;
  remove: (productId: string) => void;
  clear: () => void;
  totalCount: () => number;
  totalNet: () => number;
};

export const useOrderStore = create<OrderState>((set, get) => ({
  lines: [],
  add: (line) =>
    set((state) => {
      const existing = state.lines.find((l) => l.productId === line.productId);
      if (existing) {
        return {
          lines: state.lines.map((l) =>
            l.productId === line.productId
              ? { ...l, quantity: l.quantity + line.quantity }
              : l,
          ),
        };
      }
      return { lines: [...state.lines, line] };
    }),
  setQuantity: (productId, quantity) =>
    set((state) => ({
      lines:
        quantity <= 0
          ? state.lines.filter((l) => l.productId !== productId)
          : state.lines.map((l) =>
              l.productId === productId ? { ...l, quantity } : l,
            ),
    })),
  remove: (productId) =>
    set((state) => ({
      lines: state.lines.filter((l) => l.productId !== productId),
    })),
  clear: () => set({ lines: [] }),
  totalCount: () => get().lines.reduce((sum, l) => sum + l.quantity, 0),
  totalNet: () =>
    get().lines.reduce((sum, l) => sum + l.unitPrice * l.quantity, 0),
}));
