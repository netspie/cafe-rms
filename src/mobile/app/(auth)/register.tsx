import { Link } from "expo-router";
import { useState } from "react";
import { Alert, Text, TextInput, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { Button } from "@/components/Button";
import { ApiError, api } from "@/lib/api";
import type { LoginResponse } from "@/lib/types";
import { useAuthStore } from "@/stores/authStore";

export default function RegisterScreen() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [isPending, setIsPending] = useState(false);
  const setToken = useAuthStore((s) => s.setToken);

  const submit = async () => {
    setIsPending(true);
    try {
      await api.post("/api/auth/register/guest", {
        email,
        password,
        firstName,
        lastName,
      });
      const res = await api.post<LoginResponse>("/api/auth/login", {
        email,
        password,
      });
      await setToken(res.accessToken);
    } catch (err) {
      if (err instanceof ApiError) Alert.alert("Registration Failed", err.message);
      else Alert.alert("Registration Failed", String(err));
    } finally {
      setIsPending(false);
    }
  };

  return (
    <SafeAreaView className="flex-1 bg-bg">
      <View className="flex-1 px-6 justify-center gap-3">
        <Text className="text-3xl font-bold text-ink mb-2">Create Account</Text>

        <Field label="First Name" value={firstName} onChangeText={setFirstName} />
        <Field label="Last Name" value={lastName} onChangeText={setLastName} />
        <Field
          label="Email"
          value={email}
          onChangeText={setEmail}
          autoCapitalize="none"
          keyboardType="email-address"
        />
        <Field
          label="Password"
          value={password}
          onChangeText={setPassword}
          secureTextEntry
        />

        <Button
          label="Create Account"
          onPress={submit}
          loading={isPending}
          disabled={!email || !password || !firstName || !lastName}
        />

        <Link href="/(auth)/login" className="text-accent text-center mt-2">
          Already have an account? Sign In
        </Link>
      </View>
    </SafeAreaView>
  );
}

function Field(props: {
  label: string;
  value: string;
  onChangeText: (v: string) => void;
  autoCapitalize?: "none" | "sentences";
  keyboardType?: "default" | "email-address";
  secureTextEntry?: boolean;
}) {
  return (
    <View>
      <Text className="text-sm text-muted mb-1">{props.label}</Text>
      <TextInput
        value={props.value}
        onChangeText={props.onChangeText}
        autoCapitalize={props.autoCapitalize}
        keyboardType={props.keyboardType}
        secureTextEntry={props.secureTextEntry}
        className="bg-white border border-ink rounded-xl px-3 py-3 text-ink"
      />
    </View>
  );
}
