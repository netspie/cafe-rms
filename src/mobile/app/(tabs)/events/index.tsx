import { Link } from "expo-router";
import {
  ActivityIndicator,
  Pressable,
  SafeAreaView,
  ScrollView,
  Text,
  View,
} from "react-native";

import { api } from "@/lib/api";
import type { EventItem, PagedResult } from "@/lib/types";
import { useFetch } from "@/lib/useFetch";

export default function EventsScreen() {
  const query = useFetch(() =>
    api.get<PagedResult<EventItem>>("/api/events?sort=-createdAt&pageSize=50"),
  );

  return (
    <SafeAreaView className="flex-1 bg-bg">
      <View className="px-4 pt-4 pb-2">
        <Text className="text-2xl font-bold text-ink">Events</Text>
      </View>
      {query.isPending ? (
        <ActivityIndicator className="mt-8" />
      ) : (
        <ScrollView style={{ flex: 1 }} contentContainerStyle={{ padding: 16, gap: 8 }}>
          {(query.data?.items ?? []).length === 0 ? (
            <Text className="text-muted text-center mt-8">No events scheduled.</Text>
          ) : (
            (query.data?.items ?? []).map((item) => (
              <Link key={item.id} href={`/(tabs)/events/${item.id}`} asChild>
                <Pressable className="bg-bgSoft border border-border rounded-md p-4">
                  <Text className="text-ink font-semibold">{item.name}</Text>
                  <Text className="text-muted text-xs mt-1">
                    {item.status} · {new Date(item.createdAt).toLocaleDateString()}
                  </Text>
                </Pressable>
              </Link>
            ))
          )}
        </ScrollView>
      )}
    </SafeAreaView>
  );
}
