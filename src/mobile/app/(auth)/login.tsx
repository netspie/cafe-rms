import { Link } from "expo-router";
import { useState } from "react";
import { Alert, Text, TextInput, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { Button } from "@/components/Button";
import { ApiError, api } from "@/lib/api";
import type { LoginResponse } from "@/lib/types";
import { useAuthStore } from "@/stores/authStore";

export default function LoginScreen() {
  const [email, setEmail] = useState("user1@shiba.pl");
  const [password, setPassword] = useState("Demo1234");
  const [isPending, setIsPending] = useState(false);
  const setToken = useAuthStore((s) => s.setToken);

  const submit = async () => {
    setIsPending(true);
    try {
      const res = await api.post<LoginResponse>("/api/auth/login", { email, password });
      await setToken(res.accessToken);
    } catch (err) {
      if (err instanceof ApiError) Alert.alert("Sign In Failed", err.message);
      else Alert.alert("Sign In Failed", String(err));
    } finally {
      setIsPending(false);
    }
  };

  return (
    <SafeAreaView className="flex-1 bg-bg">
      <View className="flex-1 px-6 justify-center gap-4">
        <Text className="text-3xl font-bold text-ink mb-4">Sign In</Text>

        <View>
          <Text className="text-sm text-muted mb-1">Email</Text>
          <TextInput
            value={email}
            onChangeText={setEmail}
            autoCapitalize="none"
            keyboardType="email-address"
            className="bg-white border border-ink rounded-xl px-3 py-3 text-ink"
          />
        </View>

        <View>
          <Text className="text-sm text-muted mb-1">Password</Text>
          <TextInput
            value={password}
            onChangeText={setPassword}
            secureTextEntry
            className="bg-white border border-ink rounded-xl px-3 py-3 text-ink"
          />
        </View>

        <Button
          label="Sign In"
          onPress={submit}
          loading={isPending}
          disabled={!email || !password}
        />

        <Link href="/(auth)/register" className="text-accent text-center mt-2">
          Don't have an account? Register
        </Link>
      </View>
    </SafeAreaView>
  );
}
