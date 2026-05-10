import {
  ActivityIndicator,
  SafeAreaView,
  ScrollView,
  Text,
  View,
} from "react-native";

import { api } from "@/lib/api";
import type { LoyaltyPointLog, PagedResult } from "@/lib/types";
import { useFetch } from "@/lib/useFetch";

export default function LoyaltyScreen() {
  const balanceQuery = useFetch(() =>
    api.get<{ balance: number }>("/api/my/loyalty/balance"),
  );

  const historyQuery = useFetch(() =>
    api.get<PagedResult<LoyaltyPointLog>>(
      "/api/my/loyalty/history?sort=-createdAt&pageSize=50",
    ),
  );

  return (
    <SafeAreaView className="flex-1 bg-bg">
      <ScrollView style={{ flex: 1 }} contentContainerStyle={{ paddingBottom: 32 }}>
        <View className="px-4 pt-4 pb-2">
          <Text className="text-2xl font-bold text-ink">Loyalty</Text>
        </View>

        <View className="mx-4 my-2 bg-accentSoft border border-border rounded-md p-6 items-center">
          <Text className="text-muted text-sm mb-2">Your balance</Text>
          {balanceQuery.isPending ? (
            <ActivityIndicator />
          ) : (
            <Text className="text-5xl font-bold text-accent">
              {balanceQuery.data?.balance ?? 0}
            </Text>
          )}
          <Text className="text-muted text-sm mt-2">points</Text>
        </View>

        <Text className="text-muted px-4 mt-4 mb-2 text-sm uppercase">History</Text>
        {historyQuery.isPending ? (
          <ActivityIndicator className="mt-4" />
        ) : (historyQuery.data?.items ?? []).length === 0 ? (
          <Text className="text-muted text-center mt-8">No history yet.</Text>
        ) : (
          <View className="px-4">
            {(historyQuery.data?.items ?? []).map((item, idx) => (
              <View key={item.id}>
                {idx > 0 && <View className="h-px bg-border my-1" />}
                <View className="flex-row justify-between items-center py-2">
                  <View className="flex-1">
                    <Text className="text-ink">{item.reason ?? "(no reason)"}</Text>
                    <Text className="text-muted text-xs">
                      {new Date(item.createdAt).toLocaleString()}
                    </Text>
                  </View>
                  <Text
                    className={`font-semibold ${
                      item.points >= 0 ? "text-accent" : "text-danger"
                    }`}
                  >
                    {item.points > 0 ? "+" : ""}
                    {item.points}
                  </Text>
                </View>
              </View>
            ))}
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}
