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
import type { OrderItem, PagedResult } from "@/lib/types";
import { useFetch } from "@/lib/useFetch";

export default function OrdersScreen() {
  const query = useFetch(() =>
    api.get<PagedResult<OrderItem>>(
      "/api/my/orders?sort=-createdAt&pageSize=50",
    ),
  );

  return (
    <SafeAreaView className="flex-1 bg-bg">
      <View className="px-4 pt-4 pb-2">
        <Text className="text-2xl font-bold text-ink">My Orders</Text>
      </View>
      {query.isPending ? (
        <ActivityIndicator className="mt-8" />
      ) : (
        <ScrollView style={{ flex: 1 }} contentContainerStyle={{ padding: 16, gap: 8 }}>
          {(query.data?.items ?? []).length === 0 ? (
            <Text className="text-muted text-center mt-8">No orders yet.</Text>
          ) : (
            (query.data?.items ?? []).map((item) => (
              <Link key={item.id} href={`/(tabs)/orders/${item.id}`} asChild>
                <Pressable className="bg-bgSoft border border-border rounded-md p-4">
                  <View className="flex-row justify-between items-center mb-1">
                    <StatusBadge status={item.status} />
                    <Text className="text-muted text-xs">
                      {new Date(item.createdAt).toLocaleString()}
                    </Text>
                  </View>
                  <Text className="text-ink mt-1">Order {item.id.slice(0, 8)}</Text>
                </Pressable>
              </Link>
            ))
          )}
        </ScrollView>
      )}
    </SafeAreaView>
  );
}

function StatusBadge({ status }: { status: string }) {
  const color =
    status === "Closed"
      ? "bg-accentSoft text-accent"
      : status === "Cancelled"
        ? "bg-bgSoft text-danger"
        : "bg-bgSoft text-ink";
  return (
    <View className={`px-2 py-0.5 rounded-full ${color.split(" ")[0]}`}>
      <Text className={`text-xs font-semibold ${color.split(" ")[1]}`}>
        {status}
      </Text>
    </View>
  );
}
