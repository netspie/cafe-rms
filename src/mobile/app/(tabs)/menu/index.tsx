import { Ionicons } from "@expo/vector-icons";
import { Link, useFocusEffect } from "expo-router";
import { useCallback, useState } from "react";
import {
  ActivityIndicator,
  Image,
  Pressable,
  RefreshControl,
  ScrollView,
  Text,
  TextInput,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { api, imageUrl } from "@/lib/api";
import { eventColor } from "@/lib/eventColor";
import type { MenuItem, MobileEvent, PagedResult, Tag } from "@/lib/types";
import { useFetch } from "@/lib/useFetch";
import { useOrderStore } from "@/stores/orderStore";

export default function MenuScreen() {
  const [search, setSearch] = useState("");
  const [selectedTag, setSelectedTag] = useState<string | null>(null);
  const orderCount = useOrderStore((s) => s.totalCount());

  const tagsQuery = useFetch(
    () => api.get<PagedResult<Tag>>("/api/tags?pageSize=50"),
    [],
  );

  const eventsQuery = useFetch(
    () => api.get<MobileEvent[]>("/api/events/mobile"),
    [],
  );
  const todayEvent = eventsQuery.data?.find((e) => e.isToday) ?? null;

  const productsQuery = useFetch(
    () => {
      const params = new URLSearchParams();
      if (search) params.set("name", search);
      if (selectedTag) params.set("tagId", selectedTag);
      return api.get<MenuItem[]>(`/api/menu?${params.toString()}`);
    },
    [search, selectedTag],
  );

  const refetchProducts = productsQuery.refetch;
  const [isRefreshing, setIsRefreshing] = useState(false);
  const onRefresh = useCallback(async () => {
    setIsRefreshing(true);
    try {
      await refetchProducts();
    } finally {
      setIsRefreshing(false);
    }
  }, [refetchProducts]);

  useFocusEffect(
    useCallback(() => {
      refetchProducts();
    }, [refetchProducts]),
  );

  return (
    <SafeAreaView edges={["top"]} style={{ flex: 1, backgroundColor: "#FFFFFF" }}>
      {todayEvent && (
        <View className="px-4 pt-4 pb-1">
          <Link href={`/(tabs)/events/${todayEvent.id}`} asChild>
            <Pressable
              className="rounded-2xl p-5"
              style={{ backgroundColor: eventColor(todayEvent.name) }}
            >
              <Text className="text-white/90 text-xs font-bold tracking-widest">
                DZISIEJSZE WYDARZENIE
              </Text>
              <Text className="text-white text-2xl font-bold mt-1">
                {todayEvent.name}
              </Text>
              <View className="flex-row items-center gap-1 mt-2">
                <Text className="text-white/90 text-sm font-medium">Zobacz szczegóły</Text>
                <Ionicons name="chevron-forward" size={16} color="white" />
              </View>
            </Pressable>
          </Link>
        </View>
      )}
      <View className={`px-4 pb-3 ${todayEvent ? "pt-3" : "pt-4"}`}>
        <Text className="text-2xl font-bold text-ink mb-3">Menu</Text>
        <TextInput
          value={search}
          onChangeText={setSearch}
          placeholder="Szukaj po nazwie"
          placeholderTextColor="#6B6B6B"
          className="bg-white border border-ink rounded-xl px-3 py-2.5 text-ink mb-3"
        />
        {tagsQuery.data && (
          <ScrollView horizontal showsHorizontalScrollIndicator={false}>
            <View className="flex-row gap-2">
              <TagChip
                label="Wszystkie"
                active={!selectedTag}
                onPress={() => setSelectedTag(null)}
              />
              {tagsQuery.data.items.map((tag) => (
                <TagChip
                  key={tag.id}
                  label={tag.name}
                  active={selectedTag === tag.id}
                  onPress={() => setSelectedTag(tag.id)}
                />
              ))}
            </View>
          </ScrollView>
        )}
      </View>

      {productsQuery.isPending ? (
        <ActivityIndicator className="mt-8" />
      ) : (
        <ScrollView
          style={{ flex: 1, minHeight: 0 }}
          contentContainerStyle={{ paddingHorizontal: 16, paddingTop: 0, paddingBottom: orderCount > 0 ? 100 : 16, gap: 8 }}
          refreshControl={
            <RefreshControl refreshing={isRefreshing} onRefresh={onRefresh} />
          }
        >
          {(productsQuery.data ?? []).length === 0 ? (
            <Text className="text-muted text-center mt-8">
              Brak pasujących produktów.
            </Text>
          ) : (
            (productsQuery.data ?? []).map((item) => (
              <Link key={item.id} href={`/(tabs)/menu/${item.id}`} asChild>
                <Pressable className="bg-accentSoft rounded-xl p-3 flex-row items-center gap-3">
                  {imageUrl(item.imageUrl) ? (
                    <Image
                      source={{ uri: imageUrl(item.imageUrl)! }}
                      className="w-14 h-14 rounded-lg bg-white"
                      resizeMode="cover"
                    />
                  ) : (
                    <View className="w-14 h-14 rounded-lg bg-white items-center justify-center">
                      <Text className="text-accent text-lg font-bold">
                        {item.name.charAt(0)}
                      </Text>
                    </View>
                  )}
                  <View className="flex-1">
                    <Text className="text-ink font-semibold">{item.name}</Text>
                    {item.barcode && (
                      <Text className="text-muted text-xs mt-1">{item.barcode}</Text>
                    )}
                  </View>
                  <View className="items-end">
                    {item.originalPrice != null && item.originalPrice > item.price && (
                      <Text className="text-xs text-muted line-through">
                        {item.originalPrice.toFixed(2)} PLN
                      </Text>
                    )}
                    <Text className="text-accent font-semibold">
                      {item.price.toFixed(2)} PLN
                    </Text>
                    {item.isEventPrice && (
                      <Text className="text-xs text-accent mt-1">Cena wydarzenia</Text>
                    )}
                  </View>
                </Pressable>
              </Link>
            ))
          )}
        </ScrollView>
      )}

      {orderCount > 0 && (
        <Link href="/order" asChild>
          <Pressable className="absolute bottom-6 right-6 bg-accent rounded-full px-5 py-3 flex-row items-center gap-2 shadow-md">
            <Ionicons name="receipt-outline" size={20} color="white" />
            <Text className="text-white font-semibold">{orderCount}</Text>
          </Pressable>
        </Link>
      )}
    </SafeAreaView>
  );
}

function TagChip({
  label,
  active,
  onPress,
}: {
  label: string;
  active: boolean;
  onPress: () => void;
}) {
  return (
    <Pressable
      onPress={onPress}
      className={`px-3 py-1.5 rounded-full border ${
        active ? "bg-accent border-accent" : "bg-white border-ink"
      }`}
    >
      <Text className={active ? "text-white font-semibold" : "text-ink"}>{label}</Text>
    </Pressable>
  );
}
