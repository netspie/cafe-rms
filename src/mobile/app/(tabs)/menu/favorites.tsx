import { Link } from "expo-router";
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  Text,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { api } from "@/lib/api";
import type { Favorite } from "@/lib/types";
import { useFetch } from "@/lib/useFetch";

export default function FavoritesScreen() {
  const query = useFetch(() => api.get<Favorite[]>("/api/my/favorites"));

  return (
    <SafeAreaView className="flex-1 bg-bg">
      <View className="px-4 pt-4 pb-2">
        <Text className="text-2xl font-bold text-ink">Ulubione</Text>
      </View>
      {query.isPending ? (
        <ActivityIndicator className="mt-8" />
      ) : (
        <ScrollView style={{ flex: 1 }} contentContainerStyle={{ padding: 16, gap: 8 }}>
          {(query.data ?? []).length === 0 ? (

            <Text className="text-muted text-center mt-8">Brak ulubionych.</Text>
          ) : (
            (query.data ?? []).map((item) => (
              <Link key={item.productId} href={`/(tabs)/menu/${item.productId}`} asChild>
                <Pressable className="bg-accentSoft rounded-xl p-4">
                  <Text className="text-ink font-semibold">{item.productName}</Text>
                  {item.description && (
                    <Text className="text-muted text-sm mt-1">{item.description}</Text>
                  )}
                </Pressable>
              </Link>
            ))
          )}
        </ScrollView>
      )}
    </SafeAreaView>
  );
}
