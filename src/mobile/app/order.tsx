import { Stack, useRouter } from "expo-router";
import {
  Pressable,
  ScrollView,
  Text,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { Button } from "@/components/Button";
import { useOrderStore } from "@/stores/orderStore";

export default function OrderScreen() {
  const router = useRouter();
  const lines = useOrderStore((s) => s.lines);
  const setQuantity = useOrderStore((s) => s.setQuantity);
  const remove = useOrderStore((s) => s.remove);
  const totalNet = useOrderStore((s) => s.totalNet());

  return (
    <SafeAreaView className="flex-1 bg-bg">
      <Stack.Screen options={{ headerShown: true, title: "Order" }} />
      <ScrollView style={{ flex: 1 }} contentContainerStyle={{ padding: 16, paddingBottom: 200, gap: 8 }}>
        {lines.length === 0 ? (
          <Text className="text-muted text-center mt-8">Order is empty.</Text>
        ) : (
          lines.map((item) => (
            <View key={item.productId} className="bg-bgSoft border border-border rounded-md p-3">
              <Text className="text-ink font-semibold">{item.productName}</Text>
              <Text className="text-muted text-sm mt-1">
                {item.unitPrice.toFixed(2)} PLN each
              </Text>
              <View className="flex-row items-center justify-between mt-3">
                <View className="flex-row items-center gap-3">
                  <Pressable
                    onPress={() => setQuantity(item.productId, item.quantity - 1)}
                    className="w-8 h-8 rounded-md border border-border items-center justify-center"
                  >
                    <Text className="text-ink">−</Text>
                  </Pressable>
                  <Text className="text-ink w-6 text-center">{item.quantity}</Text>
                  <Pressable
                    onPress={() => setQuantity(item.productId, item.quantity + 1)}
                    className="w-8 h-8 rounded-md border border-border items-center justify-center"
                  >
                    <Text className="text-ink">+</Text>
                  </Pressable>
                </View>
                <Pressable onPress={() => remove(item.productId)}>
                  <Text className="text-danger">Remove</Text>
                </Pressable>
              </View>
            </View>
          ))
        )}
      </ScrollView>

      {lines.length > 0 && (
        <View className="absolute bottom-0 left-0 right-0 p-4 bg-bg border-t border-border gap-3">
          <View className="flex-row justify-between">
            <Text className="text-muted">Subtotal (Net)</Text>
            <Text className="text-ink font-semibold">
              {totalNet.toFixed(2)} PLN
            </Text>
          </View>
          <Button label="Checkout" onPress={() => router.push("/checkout")} />
        </View>
      )}
    </SafeAreaView>
  );
}
