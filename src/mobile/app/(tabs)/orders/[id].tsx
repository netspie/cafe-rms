import { Stack, useLocalSearchParams } from "expo-router";
import { useEffect, useState } from "react";
import {
  ActivityIndicator,
  Alert,
  ScrollView,
  Text,
  TextInput,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { Button } from "@/components/Button";
import { ApiError, api } from "@/lib/api";
import type { OrderDetail } from "@/lib/types";
import { useFetch } from "@/lib/useFetch";

export default function OrderDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const [reason, setReason] = useState("");
  const [showCancel, setShowCancel] = useState(false);
  const [isCancelling, setIsCancelling] = useState(false);

  const query = useFetch(
    () => api.get<OrderDetail>(`/api/my/orders/${id}`),
    [id],
  );

  const status = query.data?.status;
  useEffect(() => {
    if (status !== "Placed") return;
    const interval = setInterval(async () => {
      try {
        const latest = await api.get<OrderDetail>(`/api/my/orders/${id}`);
        if (latest.status !== status) await query.refetch();
      } catch {}
    }, 7000);
    return () => clearInterval(interval);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [status, id]);

  const cancel = async () => {
    setIsCancelling(true);
    try {
      await api.post(`/api/my/orders/${id}/cancel`, { reason: reason || null });
      await query.refetch();
      setShowCancel(false);
    } catch (err) {
      if (err instanceof ApiError)
        Alert.alert("Cancel Failed", err.detail ?? err.message);
    } finally {
      setIsCancelling(false);
    }
  };

  if (query.isPending || !query.data) {
    return (
      <View className="flex-1 bg-bg">
        <Stack.Screen options={{ headerShown: true, title: "Order Detail" }} />
        <ActivityIndicator className="mt-8" />
      </View>
    );
  }

  const order = query.data;
  const subtotal = order.lines.reduce(
    (sum, l) => sum + (l.netPerOne + l.vatPerOne) * l.quantity,
    0,
  );
  const total = Math.max(0, subtotal - order.discount - order.loyaltyPointsUsed);

  return (
    <SafeAreaView edges={["bottom"]} className="flex-1 bg-bg">
      <Stack.Screen options={{ headerShown: true, title: "Order Detail" }} />
      <ScrollView contentContainerStyle={{ padding: 16, paddingBottom: 40, gap: 16 }}>
        <View className="flex-row justify-between items-center">
          <Text className="text-xl font-bold text-ink">
            Order {order.id.slice(0, 8)}
          </Text>
          <Text className="text-muted">{order.status}</Text>
        </View>

        <View className="border-t border-accentSoft pt-3 gap-2">
          {order.lines.map((line) => (
            <View key={line.id} className="flex-row justify-between">
              <Text className="text-ink">
                {line.quantity} × {line.productName}
              </Text>
              <Text className="text-ink">
                {((line.netPerOne + line.vatPerOne) * line.quantity).toFixed(2)} PLN
              </Text>
            </View>
          ))}
        </View>

        <View className="border-t border-accentSoft pt-3 gap-1">
          <Row label="Subtotal" value={`${subtotal.toFixed(2)} PLN`} />
          {order.discount > 0 && (
            <Row label="Promo Discount" value={`-${order.discount.toFixed(2)} PLN`} />
          )}
          {order.loyaltyPointsUsed > 0 && (
            <Row
              label={`Loyalty (${order.loyaltyPointsUsed} pts)`}
              value={`-${order.loyaltyPointsUsed.toFixed(2)} PLN`}
            />
          )}
          <View className="flex-row justify-between border-t border-accentSoft pt-2 mt-1">
            <Text className="text-ink font-semibold">Total</Text>
            <Text className="text-ink font-bold text-lg">{total.toFixed(2)} PLN</Text>
          </View>
        </View>

        {order.cancelledAt && order.cancellationReason && (
          <View className="bg-accentSoft rounded-xl p-3">
            <Text className="text-muted text-sm">Cancellation Reason</Text>
            <Text className="text-ink mt-1">{order.cancellationReason}</Text>
          </View>
        )}

        {order.status === "Placed" && !showCancel && (
          <Button
            label="Cancel Order"
            variant="danger"
            onPress={() => setShowCancel(true)}
          />
        )}
        {showCancel && (
          <View className="gap-2">
            <Text className="text-sm text-muted">Reason (Optional)</Text>
            <TextInput
              value={reason}
              onChangeText={setReason}
              className="bg-white border border-ink rounded-xl px-3 py-2.5 text-ink"
            />
            <Button
              label="Confirm Cancellation"
              variant="danger"
              onPress={cancel}
              loading={isCancelling}
            />
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <View className="flex-row justify-between">
      <Text className="text-muted">{label}</Text>
      <Text className="text-ink">{value}</Text>
    </View>
  );
}
