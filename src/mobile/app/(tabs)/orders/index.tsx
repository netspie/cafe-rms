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
import { orderStatusLabel } from "@/lib/labels";
import type { OrderItem, OrderStatus, PagedResult } from "@/lib/types";
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
        <Text className="text-2xl font-bold text-ink">Moje zamówienia</Text>
      </View>
      {query.isPending ? (
        <ActivityIndicator className="mt-8" />
      ) : (
        <ScrollView style={{ flex: 1 }} contentContainerStyle={{ padding: 16, gap: 8 }}>
          {(query.data?.items ?? []).length === 0 ? (
            <Text className="text-muted text-center mt-8">Brak zamówień.</Text>
          ) : (
            (query.data?.items ?? []).map((item) => (
              <Link key={item.id} href={`/(tabs)/orders/${item.id}`} asChild>
                <Pressable className="bg-accentSoft rounded-xl p-4">
                  <View className="flex-row justify-between items-center mb-1">
                    <StatusBadge status={item.status} />
                    <Text className="text-muted text-xs">
                      {new Date(item.createdAt).toLocaleString("pl-PL")}
                    </Text>
                  </View>
                  <Text className="text-ink mt-1">Zamówienie {item.id.slice(0, 8)}</Text>
                </Pressable>
              </Link>
            ))
          )}
        </ScrollView>
      )}
    </SafeAreaView>
  );
}

function StatusBadge({ status }: { status: OrderStatus }) {
  const background =
    status === "Cancelled" ? "bg-danger" : status === "Closed" ? "bg-ink" : "bg-accent";
  return (
    <View className={`px-2.5 py-0.5 rounded-full ${background}`}>
      <Text className="text-xs font-semibold text-white">{orderStatusLabel(status)}</Text>
    </View>
  );
}
