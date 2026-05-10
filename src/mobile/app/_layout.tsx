import "./globals.css";

import { Stack, useRouter, useSegments } from "expo-router";
import { StatusBar } from "expo-status-bar";
import { useEffect } from "react";
import { GestureHandlerRootView } from "react-native-gesture-handler";

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

  useEffect(() => {
    if (!hydrated) return;
    const inAuth = segments[0] === "(auth)";
    if (!token && !inAuth) router.replace("/(auth)/login");
    if (token && inAuth) router.replace("/(tabs)/menu");
  }, [hydrated, token, segments, router]);

  return (
    <GestureHandlerRootView style={{ flex: 1 }}>
      <Stack screenOptions={{ headerShown: false }} />
      <StatusBar style="dark" />
    </GestureHandlerRootView>
  );
}
