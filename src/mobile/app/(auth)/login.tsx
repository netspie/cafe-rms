import { Link } from "expo-router";
import { useState } from "react";
import { Alert, Image, Text, TextInput, View } from "react-native";
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
      if (err instanceof ApiError) Alert.alert("Logowanie nieudane", err.message);
      else Alert.alert("Logowanie nieudane", String(err));
    } finally {
      setIsPending(false);
    }
  };

  return (
    <SafeAreaView className="flex-1 bg-bg">
      <View className="flex-1 px-6 justify-center gap-4">
        <View className="items-center mb-6">
          <Image
            source={require("../../assets/images/icon.png")}
            className="w-32 h-32 rounded-3xl"
            resizeMode="cover"
          />
          <Text className="text-4xl font-bold text-ink mt-4">Shiba Cafe</Text>
        </View>

        <View>
          <Text className="text-sm text-muted mb-1">E-mail</Text>
          <TextInput
            value={email}
            onChangeText={setEmail}
            autoCapitalize="none"
            keyboardType="email-address"
            className="bg-white border border-ink rounded-xl px-3 py-3 text-ink"
          />
        </View>

        <View>
          <Text className="text-sm text-muted mb-1">Hasło</Text>
          <TextInput
            value={password}
            onChangeText={setPassword}
            secureTextEntry
            className="bg-white border border-ink rounded-xl px-3 py-3 text-ink"
          />
        </View>

        <Button
          label="Zaloguj się"
          onPress={submit}
          loading={isPending}
          disabled={!email || !password}
        />

        <Link href="/(auth)/register" className="text-accent text-center mt-2">
          Nie masz konta? Zarejestruj się
        </Link>
      </View>
    </SafeAreaView>
  );
}
