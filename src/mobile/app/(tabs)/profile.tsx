import { Link } from "expo-router";
import {
  ActivityIndicator,
  SafeAreaView,
  ScrollView,
  Text,
  View,
} from "react-native";

import { Button } from "@/components/Button";
import { api } from "@/lib/api";
import type { Me } from "@/lib/types";
import { useFetch } from "@/lib/useFetch";
import { useAuthStore } from "@/stores/authStore";

export default function ProfileScreen() {
  const logout = useAuthStore((s) => s.logout);

  const meQuery = useFetch(() => api.get<Me>("/api/me"));

  return (
    <SafeAreaView className="flex-1 bg-bg">
      <ScrollView contentContainerStyle={{ padding: 16, gap: 16 }}>
        <Text className="text-2xl font-bold text-ink">Profile</Text>

        {meQuery.isPending ? (
          <ActivityIndicator />
        ) : meQuery.data ? (
          <View className="bg-bgSoft border border-border rounded-md p-4 gap-1">
            <Text className="text-ink text-lg font-semibold">
              {meQuery.data.firstName} {meQuery.data.lastName}
            </Text>
            <Text className="text-muted">{meQuery.data.email}</Text>
            <Text className="text-muted text-sm mt-2">
              {meQuery.data.accountType}
            </Text>
          </View>
        ) : (
          <Text className="text-danger">Couldn't load profile.</Text>
        )}

        <Link href="/(tabs)/menu/favorites" className="text-accent">
          Favorites →
        </Link>

        <View className="mt-4">
          <Button label="Sign Out" variant="secondary" onPress={() => logout()} />
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}
