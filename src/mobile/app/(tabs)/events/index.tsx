import { Link, useFocusEffect } from "expo-router";
import { useCallback, useState } from "react";
import {
  ActivityIndicator,
  Pressable,
  RefreshControl,
  ScrollView,
  Text,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { api } from "@/lib/api";
import { eventColor } from "@/lib/eventColor";
import type { MobileEvent } from "@/lib/types";
import { useFetch } from "@/lib/useFetch";

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString(undefined, {
    weekday: "short",
    day: "numeric",
    month: "short",
  });
}

export default function EventsScreen() {
  const query = useFetch(() => api.get<MobileEvent[]>("/api/events/mobile"));
  const refetch = query.refetch;

  const [isRefreshing, setIsRefreshing] = useState(false);
  const onRefresh = useCallback(async () => {
    setIsRefreshing(true);
    try {
      await refetch();
    } finally {
      setIsRefreshing(false);
    }
  }, [refetch]);

  useFocusEffect(
    useCallback(() => {
      refetch();
    }, [refetch]),
  );

  const events = query.data ?? [];
  const today = events.filter((e) => e.isToday);
  const upcoming = events.filter((e) => !e.isToday);

  return (
    <SafeAreaView className="flex-1 bg-bg">
      <View className="px-4 pt-4 pb-2">
        <Text className="text-2xl font-bold text-ink">Events</Text>
      </View>
      {query.isPending ? (
        <ActivityIndicator className="mt-8" />
      ) : (
        <ScrollView
          style={{ flex: 1 }}
          contentContainerStyle={{ padding: 16, gap: 12 }}
          refreshControl={
            <RefreshControl refreshing={isRefreshing} onRefresh={onRefresh} />
          }
        >
          {events.length === 0 && (
            <Text className="text-muted text-center mt-8">No events scheduled.</Text>
          )}

          {today.map((e) => (
            <Link key={e.id} href={`/(tabs)/events/${e.id}`} asChild>
              <Pressable
                className="rounded-2xl p-5"
                style={{ backgroundColor: eventColor(e.name) }}
              >
                <Text className="text-white/90 text-xs font-bold tracking-widest">
                  TODAY
                </Text>
                <Text className="text-white text-2xl font-bold mt-1">{e.name}</Text>
                <Text className="text-white/90 mt-1">{formatDate(e.nextDate)}</Text>
              </Pressable>
            </Link>
          ))}

          {upcoming.length > 0 && (
            <Text className="text-muted text-xs font-semibold uppercase tracking-wider mt-2">
              Upcoming
            </Text>
          )}
          {upcoming.map((e) => (
            <Link key={e.id} href={`/(tabs)/events/${e.id}`} asChild>
              <Pressable className="flex-row items-stretch bg-accentSoft rounded-xl overflow-hidden">
                <View style={{ width: 6, backgroundColor: eventColor(e.name) }} />
                <View className="flex-1 p-4">
                  <Text className="text-ink font-semibold">{e.name}</Text>
                  <Text className="text-muted text-xs mt-1">{formatDate(e.nextDate)}</Text>
                </View>
              </Pressable>
            </Link>
          ))}
        </ScrollView>
      )}
    </SafeAreaView>
  );
}
