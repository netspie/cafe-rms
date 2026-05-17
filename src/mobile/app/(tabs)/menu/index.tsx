import { Ionicons } from "@expo/vector-icons";
import { Link } from "expo-router";
import { useState } from "react";
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  Text,
  TextInput,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { api } from "@/lib/api";
import type { PagedResult, ProductItem, Tag } from "@/lib/types";
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

  const productsQuery = useFetch(
    () => {
      const params = new URLSearchParams({ pageSize: "50", sort: "name" });
      if (search) params.set("name", search);
      if (selectedTag) params.set("tagId", selectedTag);
      return api.get<PagedResult<ProductItem>>(
        `/api/products?${params.toString()}`,
      );
    },
    [search, selectedTag],
  );

  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: "#FFFFFF" }}>
      <View className="px-4 pt-4 pb-2">
        <Text className="text-2xl font-bold text-ink mb-3">Menu</Text>
        <TextInput
          value={search}
          onChangeText={setSearch}
          placeholder="Search by Name"
          className="border border-border rounded-md px-3 py-2 text-ink mb-3"
        />
        {tagsQuery.data && (
          <ScrollView horizontal showsHorizontalScrollIndicator={false}>
            <View className="flex-row gap-2">
              <TagChip
                label="All"
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
          contentContainerStyle={{ padding: 16, paddingBottom: 100, gap: 8 }}
        >
          {(productsQuery.data?.items ?? []).length === 0 ? (
            <Text className="text-muted text-center mt-8">
              No products match.
            </Text>
          ) : (
            (productsQuery.data?.items ?? []).map((item) => (
              <Link key={item.id} href={`/(tabs)/menu/${item.id}`} asChild>
                <Pressable className="bg-bgSoft rounded-md p-4 border border-border">
                  <Text className="text-ink font-semibold">{item.name}</Text>
                  {item.barcode && (
                    <Text className="text-muted text-xs mt-1">{item.barcode}</Text>
                  )}
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
      className={`px-3 py-1 rounded-full border ${
        active ? "bg-accent border-accent" : "bg-bg border-border"
      }`}
    >
      <Text className={active ? "text-white" : "text-ink"}>{label}</Text>
    </Pressable>
  );
}
