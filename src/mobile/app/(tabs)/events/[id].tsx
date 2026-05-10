import { Stack, useLocalSearchParams, useRouter } from "expo-router";
import {
  ActivityIndicator,
  Image,
  SafeAreaView,
  ScrollView,
  Text,
  View,
} from "react-native";

import { Button } from "@/components/Button";
import { api } from "@/lib/api";
import type { EventDetail } from "@/lib/types";
import { useFetch } from "@/lib/useFetch";

export default function EventDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();

  const query = useFetch(() => api.get<EventDetail>(`/api/events/${id}`), [id]);

  if (query.isPending || !query.data) {
    return (
      <SafeAreaView className="flex-1 bg-bg">
        <ActivityIndicator className="mt-8" />
      </SafeAreaView>
    );
  }

  const event = query.data;

  return (
    <SafeAreaView className="flex-1 bg-bg">
      <Stack.Screen options={{ headerShown: true, title: event.name }} />
      <ScrollView contentContainerStyle={{ padding: 16, gap: 16 }}>
        {event.imageUrl && (
          <Image
            source={{ uri: event.imageUrl }}
            className="w-full h-48 rounded-md bg-bgSoft"
            resizeMode="cover"
          />
        )}
        <Text className="text-2xl font-bold text-ink">{event.name}</Text>
        <Text className="text-muted">{event.status}</Text>
        {event.description && (
          <Text className="text-ink">{event.description}</Text>
        )}

        {event.days.length > 0 && (
          <View>
            <Text className="text-sm text-muted mb-2 uppercase">Dates</Text>
            {event.days.map((d) => (
              <Text key={d.id} className="text-ink py-1">
                {new Date(d.date).toDateString()}
              </Text>
            ))}
          </View>
        )}

        {event.status === "Published" && (
          <Button
            label="Order at This Event"
            onPress={() =>
              router.push({ pathname: "/checkout", params: { eventId: event.id } })
            }
          />
        )}
      </ScrollView>
    </SafeAreaView>
  );
}
