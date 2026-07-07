import { Stack, useRouter } from "expo-router";
import { useEffect, useState } from "react";
import {
  Alert,
  ScrollView,
  Text,
  TextInput,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { Button } from "@/components/Button";
import { ApiError, api } from "@/lib/api";
import type {
  Me,
  PagedResult,
  SalesChannel,
  Table,
  ValidatePromoResponse,
} from "@/lib/types";
import { useFetch } from "@/lib/useFetch";
import { useOrderStore } from "@/stores/orderStore";

export default function CheckoutScreen() {
  const router = useRouter();
  const lines = useOrderStore((s) => s.lines);
  const totalGross = useOrderStore((s) => s.totalGross());
  const clear = useOrderStore((s) => s.clear);

  const [tableId, setTableId] = useState<string | null>(null);
  const [salesChannelId, setSalesChannelId] = useState<string | null>(null);
  const [promotionCode, setPromotionCode] = useState("");
  const [promoValidation, setPromoValidation] =
    useState<ValidatePromoResponse | null>(null);
  const [loyaltyPointsUsed, setLoyaltyPointsUsed] = useState("0");
  const [isValidatingPromo, setIsValidatingPromo] = useState(false);
  const [isPlacingOrder, setIsPlacingOrder] = useState(false);

  const meQuery = useFetch(() => api.get<Me>("/api/me"));
  const tablesQuery = useFetch(() =>
    api.get<PagedResult<Table>>("/api/tables?pageSize=100"),
  );
  const channelsQuery = useFetch(() =>
    api.get<PagedResult<SalesChannel>>("/api/sales-channels?pageSize=100"),
  );
  const balanceQuery = useFetch(() =>
    api.get<{ balance: number }>("/api/my/loyalty/balance"),
  );

  useEffect(() => {
    if (channelsQuery.data && !salesChannelId) {
      setSalesChannelId(channelsQuery.data.items[0]?.id ?? null);
    }
  }, [channelsQuery.data, salesChannelId]);

  const validatePromo = async () => {
    setIsValidatingPromo(true);
    try {
      const res = await api.post<ValidatePromoResponse>(
        "/api/promotion-codes/validate",
        { code: promotionCode },
      );
      setPromoValidation(res);
    } finally {
      setIsValidatingPromo(false);
    }
  };

  const placeOrder = async () => {
    setIsPlacingOrder(true);
    try {
      const res = await api.post<{ orderId: string }>("/api/my/orders", {
        outletId: meQuery.data?.outletId,
        tableId,
        salesChannelId,
        promotionCode: promoValidation?.valid ? promotionCode : null,
        loyaltyPointsUsed: parseInt(loyaltyPointsUsed, 10) || 0,
        lines: lines.map((l) => ({
          productId: l.productId,
          quantity: l.quantity,
        })),
      });
      clear();
      router.replace(`/(tabs)/orders/${res.orderId}`);
    } catch (err) {
      if (err instanceof ApiError)
        Alert.alert("Order Failed", err.detail ?? err.message);
      else Alert.alert("Order Failed", String(err));
    } finally {
      setIsPlacingOrder(false);
    }
  };

  const selectedChannel = channelsQuery.data?.items.find(
    (sc) => sc.id === salesChannelId,
  );
  const requiresTable = selectedChannel ? !selectedChannel.isTakeout : false;
  const isMissingTable = requiresTable && tableId === null;

  const loyaltyBalance = balanceQuery.data?.balance ?? 0;
  const maxRedeemablePoints = Math.min(loyaltyBalance, Math.floor(totalGross * 0.5));
  const pointsEntered = parseInt(loyaltyPointsUsed, 10) || 0;
  const loyaltyExceeded = pointsEntered > maxRedeemablePoints;

  return (
    <SafeAreaView edges={["bottom"]} className="flex-1 bg-bg">
      <Stack.Screen options={{ headerShown: true, title: "Checkout" }} />
      <ScrollView contentContainerStyle={{ padding: 16, paddingBottom: 40, gap: 16 }}>
        <View>
          <Text className="text-sm text-muted mb-2">Sales Channel</Text>
          <View className="gap-2">
            {channelsQuery.data?.items.map((sc) => (
              <Selectable
                key={sc.id}
                label={sc.name + (sc.isTakeout ? " (takeout)" : "")}
                selected={salesChannelId === sc.id}
                onPress={() => setSalesChannelId(sc.id)}
              />
            ))}
          </View>
        </View>

        <View>
          <Text className="text-sm text-muted mb-2">
            {requiresTable ? "Table" : "Table (Optional)"}
          </Text>
          <View className="gap-2">
            {!requiresTable && (
              <Selectable
                label="No Table"
                selected={tableId === null}
                onPress={() => setTableId(null)}
              />
            )}
            {tablesQuery.data?.items.map((t) => (
              <Selectable
                key={t.id}
                label={t.name}
                selected={tableId === t.id}
                onPress={() => setTableId(t.id)}
              />
            ))}
          </View>
          {isMissingTable && (
            <Text className="text-sm text-danger mt-2">
              Dine-in orders need a table.
            </Text>
          )}
        </View>

        <View>
          <Text className="text-sm text-muted mb-2">Promotion Code</Text>
          <View className="flex-row gap-2">
            <TextInput
              value={promotionCode}
              onChangeText={setPromotionCode}
              autoCapitalize="characters"
              placeholder="WELCOME10"
              className="flex-1 border border-border rounded-md px-3 py-2 text-ink"
            />
            <Button
              label="Check"
              variant="secondary"
              onPress={validatePromo}
              loading={isValidatingPromo}
              disabled={!promotionCode}
            />
          </View>
          {promoValidation && (
            <Text
              className={`text-sm mt-2 ${
                promoValidation.valid ? "text-accent" : "text-danger"
              }`}
            >
              {promoValidation.valid
                ? `${promoValidation.discountPercentage}% off applied`
                : (promoValidation.reason ?? "Invalid code")}
            </Text>
          )}
        </View>

        <View>
          <Text className="text-sm text-muted mb-2">
            Loyalty Points to Redeem
            {balanceQuery.data
              ? ` (balance ${balanceQuery.data.balance})`
              : ""}
          </Text>
          <TextInput
            value={loyaltyPointsUsed}
            onChangeText={setLoyaltyPointsUsed}
            keyboardType="number-pad"
            className="border border-border rounded-md px-3 py-2 text-ink"
          />
          <Text className="text-xs text-muted mt-1">
            Up to {maxRedeemablePoints} pts — points can cover at most half the order.
          </Text>
          {loyaltyExceeded && (
            <Text className="text-sm text-danger mt-1">
              Too many points — max {maxRedeemablePoints} for this order.
            </Text>
          )}
        </View>

        <View className="border-t border-border pt-4 gap-2">
          <View className="flex-row justify-between">
            <Text className="text-muted">Total</Text>
            <Text className="text-ink font-semibold">
              {totalGross.toFixed(2)} PLN
            </Text>
          </View>
        </View>

        <Button
          label="Place Order"
          onPress={placeOrder}
          loading={isPlacingOrder}
          disabled={
            lines.length === 0 ||
            !meQuery.data?.outletId ||
            isMissingTable ||
            loyaltyExceeded
          }
        />
      </ScrollView>
    </SafeAreaView>
  );
}

function Selectable({
  label,
  selected,
  onPress,
}: {
  label: string;
  selected: boolean;
  onPress: () => void;
}) {
  return (
    <View
      className={`border rounded-md px-3 py-2 ${
        selected ? "border-accent bg-accentSoft" : "border-border"
      }`}
      onTouchEnd={onPress}
    >
      <Text className={selected ? "text-accent font-semibold" : "text-ink"}>
        {label}
      </Text>
    </View>
  );
}
