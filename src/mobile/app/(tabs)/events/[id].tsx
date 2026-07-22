import { Ionicons } from "@expo/vector-icons";
import { Stack, useLocalSearchParams, useRouter } from "expo-router";
import {
  ActivityIndicator,
  Image,
  Pressable,
  ScrollView,
  Text,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { Button } from "@/components/Button";
import { api } from "@/lib/api";
import { eventStatusLabel } from "@/lib/labels";
import type { EventDetail } from "@/lib/types";
import { useFetch } from "@/lib/useFetch";

export default function EventDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();

  const goBack = () =>
    router.canGoBack() ? router.back() : router.replace("/(tabs)/events");

  const headerBack = () => (
    <Pressable onPress={goBack} className="pl-2 pr-3 py-1">
      <Ionicons name="chevron-back" size={26} color="#E1741E" />
    </Pressable>
  );

  const query = useFetch(() => api.get<EventDetail>(`/api/events/${id}`), [id]);

  if (query.isPending || !query.data) {
    return (
      <View className="flex-1 bg-bg">
        <Stack.Screen options={{ headerShown: true, title: "", headerLeft: headerBack }} />
        <ActivityIndicator className="mt-8" />
      </View>
    );
  }

  const event = query.data;
  const todayStr = new Date().toLocaleDateString("en-CA");
  const isHappeningToday = event.days.some((d) => d.date.slice(0, 10) === todayStr);

  return (
    <SafeAreaView edges={["bottom"]} className="flex-1 bg-bg">
      <Stack.Screen options={{ headerShown: true, title: event.name, headerLeft: headerBack }} />
      <ScrollView contentContainerStyle={{ padding: 16, paddingBottom: 40, gap: 16 }}>
        {event.imageUrl && (
          <Image
            source={{ uri: event.imageUrl }}
            className="w-full h-48 rounded-xl bg-accentSoft"
            resizeMode="cover"
          />
        )}
        <Text className="text-2xl font-bold text-ink">{event.name}</Text>
        <Text className="text-muted">{eventStatusLabel(event.status)}</Text>
        {event.description && (
          <Text className="text-ink">{event.description}</Text>
        )}

        {event.days.length > 0 && (
          <View>
            <Text className="text-sm text-muted mb-2 uppercase">Terminy</Text>
            {event.days.map((d) => (
              <Text key={d.id} className="text-ink py-1">
                {new Date(d.date).toLocaleDateString("pl-PL", {
                  weekday: "long",
                  day: "numeric",
                  month: "long",
                  year: "numeric",
                })}
              </Text>
            ))}
          </View>
        )}

        {event.status === "Published" && isHappeningToday && (
          <Button
            label="Przeglądaj menu"
            onPress={() => router.push("/(tabs)/menu")}
          />
        )}
      </ScrollView>
    </SafeAreaView>
  );
}
