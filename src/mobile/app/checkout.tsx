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
          modifierIds: l.modifiers.map((m) => m.id),
        })),
      });
      clear();
      router.replace(`/(tabs)/orders/${res.orderId}`);
    } catch (err) {
      if (err instanceof ApiError)
        Alert.alert("Nie udało się złożyć zamówienia", err.detail ?? err.message);
      else Alert.alert("Nie udało się złożyć zamówienia", String(err));
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
  const pointsApplied = Math.min(Math.max(pointsEntered, 0), maxRedeemablePoints);
  const promoDiscount = promoValidation?.valid
    ? Math.round(totalGross * ((promoValidation.discountPercentage ?? 0) / 100) * 100) / 100
    : 0;
  const finalTotal = Math.max(0, totalGross - promoDiscount - pointsApplied);

  return (
    <SafeAreaView edges={["bottom"]} className="flex-1 bg-bg">
      <Stack.Screen options={{ headerShown: true, title: "Podsumowanie" }} />
      <ScrollView contentContainerStyle={{ padding: 16, paddingBottom: 40, gap: 16 }}>
        <View>
          <Text className="text-sm text-muted mb-2">Kanał sprzedaży</Text>
          <View className="gap-2">
            {channelsQuery.data?.items.map((sc) => (
              <Selectable
                key={sc.id}
                label={sc.name}
                selected={salesChannelId === sc.id}
                onPress={() => setSalesChannelId(sc.id)}
              />
            ))}
          </View>
        </View>

        <View>
          <Text className="text-sm text-muted mb-2">
            {requiresTable ? "Stolik" : "Stolik (opcjonalnie)"}
          </Text>
          <View className="gap-2">
            {!requiresTable && (
              <Selectable
                label="Bez stolika"
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
              Zamówienia na miejscu wymagają stolika.
            </Text>
          )}
        </View>

        <View>
          <Text className="text-sm text-muted mb-2">Kod promocyjny</Text>
          <View className="flex-row gap-2">
            <TextInput
              value={promotionCode}
              onChangeText={(text) => {
                setPromotionCode(text);
                setPromoValidation(null);
              }}
              autoCapitalize="characters"
              placeholder="WELCOME10"
              placeholderTextColor="#6B6B6B"
              className="flex-1 bg-white border border-ink rounded-xl px-3 py-2.5 text-ink"
            />
            <Button
              label="Sprawdź"
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
                ? `Zniżka ${promoValidation.discountPercentage}% zastosowana`
                : (promoValidation.reason ?? "Nieprawidłowy kod")}
            </Text>
          )}
        </View>

        <View>
          <Text className="text-sm text-muted mb-2">
            Punkty do wykorzystania
            {balanceQuery.data
              ? ` (saldo ${balanceQuery.data.balance})`
              : ""}
          </Text>
          <TextInput
            value={loyaltyPointsUsed}
            onChangeText={setLoyaltyPointsUsed}
            keyboardType="number-pad"
            className="bg-white border border-ink rounded-xl px-3 py-2.5 text-ink"
          />
          <Text className="text-xs text-muted mt-1">
            Do {maxRedeemablePoints} pkt — punkty mogą pokryć maksymalnie połowę zamówienia.
          </Text>
          {loyaltyExceeded && (
            <Text className="text-sm text-danger mt-1">
              Za dużo punktów — maksymalnie {maxRedeemablePoints} dla tego zamówienia.
            </Text>
          )}
        </View>

        <View className="border-t border-accentSoft pt-4 gap-2">
          <View className="flex-row justify-between">
            <Text className="text-muted">Suma częściowa</Text>
            <Text className="text-ink">{totalGross.toFixed(2)} PLN</Text>
          </View>
          {promoDiscount > 0 && (
            <View className="flex-row justify-between">
              <Text className="text-muted">Rabat ({promotionCode})</Text>
              <Text className="text-accent">−{promoDiscount.toFixed(2)} PLN</Text>
            </View>
          )}
          {pointsApplied > 0 && (
            <View className="flex-row justify-between">
              <Text className="text-muted">Lojalność ({pointsApplied} pkt)</Text>
              <Text className="text-accent">−{pointsApplied.toFixed(2)} PLN</Text>
            </View>
          )}
          <View className="flex-row justify-between border-t border-accentSoft pt-2">
            <Text className="text-ink font-semibold">Do zapłaty</Text>
            <Text className="text-ink font-bold text-lg">
              {finalTotal.toFixed(2)} PLN
            </Text>
          </View>
        </View>

        <Button
          label="Złóż zamówienie"
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
      className={`rounded-xl px-3 py-2.5 ${selected ? "bg-accent" : "bg-accentSoft"}`}
      onTouchEnd={onPress}
    >
      <Text className={selected ? "text-white font-semibold" : "text-ink"}>
        {label}
      </Text>
    </View>
  );
}
