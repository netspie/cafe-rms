import "./globals.css";

import { Stack, useRouter, useSegments } from "expo-router";
import { StatusBar } from "expo-status-bar";
import { useEffect } from "react";
import { GestureHandlerRootView } from "react-native-gesture-handler";
import { SafeAreaProvider } from "react-native-safe-area-context";

import { useAuthStore } from "@/stores/authStore";

export default function RootLayout() {
  const hydrated = useAuthStore((s) => s.hydrated);
  const hydrate = useAuthStore((s) => s.hydrate);
  const token = useAuthStore((s) => s.token);
  const segments = useSegments();
  const router = useRouter();

  useEffect(() => {
    hydrate();
  }, [hydrate]);

  const inAuth = segments[0] === "(auth)";

  useEffect(() => {
    if (!hydrated) return;
    if (!token && !inAuth) router.replace("/(auth)/login");
    if (token && inAuth) router.replace("/(tabs)/menu");
  }, [hydrated, token, inAuth, router]);

  const authSettled = hydrated && (token ? true : inAuth);

  return (
    <SafeAreaProvider>
      <GestureHandlerRootView style={{ flex: 1 }}>
        {authSettled && (
          <Stack
            screenOptions={{ headerShown: false, headerBackButtonDisplayMode: "minimal" }}
          />
        )}
        <StatusBar style="dark" />
      </GestureHandlerRootView>
    </SafeAreaProvider>
  );
}
