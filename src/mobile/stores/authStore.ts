import * as SecureStore from "expo-secure-store";
import { Platform } from "react-native";
import { create } from "zustand";

const TOKEN_KEY = "caferms.auth.token";

const isWeb = Platform.OS === "web";
const hasLocalStorage =
  isWeb && typeof window !== "undefined" && !!window.localStorage;

const storage = {
  get: async (): Promise<string | null> => {
    if (isWeb) {
      if (!hasLocalStorage) {
        console.warn("[auth] localStorage unavailable on get");
        return null;
      }
      const v = window.localStorage.getItem(TOKEN_KEY);
      console.log("[auth] storage.get →", v ? `${v.slice(0, 20)}…` : null);
      return v;
    }
    return SecureStore.getItemAsync(TOKEN_KEY);
  },
  set: async (value: string): Promise<void> => {
    if (isWeb) {
      if (!hasLocalStorage) {
        console.warn("[auth] localStorage unavailable on set");
        return;
      }
      window.localStorage.setItem(TOKEN_KEY, value);
      console.log("[auth] storage.set →", `${value.slice(0, 20)}…`);
      return;
    }
    await SecureStore.setItemAsync(TOKEN_KEY, value);
  },
  remove: async (): Promise<void> => {
    if (isWeb) {
      if (!hasLocalStorage) return;
      window.localStorage.removeItem(TOKEN_KEY);
      console.log("[auth] storage.remove");
      return;
    }
    await SecureStore.deleteItemAsync(TOKEN_KEY);
  },
};

type AuthState = {
  token: string | null;
  hydrated: boolean;
  hydrate: () => Promise<void>;
  setToken: (token: string) => Promise<void>;
  logout: () => Promise<void>;
};

export const useAuthStore = create<AuthState>((set) => ({
  token: null,
  hydrated: false,
  hydrate: async () => {
    const token = await storage.get();
    set({ token, hydrated: true });
  },
  setToken: async (token) => {
    await storage.set(token);
    set({ token });
  },
  logout: async () => {
    await storage.remove();
    set({ token: null });
  },
}));
