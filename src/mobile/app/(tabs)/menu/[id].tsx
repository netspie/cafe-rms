import { Ionicons } from "@expo/vector-icons";
import { Stack, useLocalSearchParams, useRouter } from "expo-router";
import { useState } from "react";
import {
  ActivityIndicator,
  Alert,
  Image,
  Pressable,
  ScrollView,
  Text,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { Button } from "@/components/Button";
import { ApiError, api, imageUrl } from "@/lib/api";
import type { Allergen, Favorite, MenuItemDetail } from "@/lib/types";
import { useFetch } from "@/lib/useFetch";
import { useOrderStore } from "@/stores/orderStore";

export default function ProductDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const [quantity, setQuantity] = useState(1);
  const add = useOrderStore((s) => s.add);

  const productQuery = useFetch(
    () => api.get<MenuItemDetail>(`/api/menu/${id}`),
    [id],
  );

  const allergensQuery = useFetch(() =>
    api.get<{ items: Allergen[] }>("/api/allergens?pageSize=100"),
  );

  const favoritesQuery = useFetch(() => api.get<Favorite[]>("/api/my/favorites"));

  const isFavorited =
    favoritesQuery.data?.some((f) => f.productId === id) ?? false;

  const toggleFavorite = async () => {
    try {
      if (isFavorited) await api.del(`/api/my/favorites/${id}`);
      else await api.post("/api/my/favorites", { productId: id });
      await favoritesQuery.refetch();
    } catch (err) {
      if (err instanceof ApiError) Alert.alert("Nie udało się", err.message);
    }
  };

  if (productQuery.isPending || !productQuery.data) {
    return (
      <View className="flex-1 bg-bg">
        <Stack.Screen options={{ headerShown: true, title: "" }} />
        <ActivityIndicator className="mt-8" />
      </View>
    );
  }

  const product = productQuery.data;
  const allergens = allergensQuery.data?.items ?? [];
  const productAllergens = allergens.filter((a) =>
    product.allergenIds.includes(a.id),
  );

  return (
    <SafeAreaView edges={["bottom"]} className="flex-1 bg-bg">
      <Stack.Screen options={{ headerShown: true, title: product.name }} />
      <ScrollView contentContainerStyle={{ padding: 16, paddingBottom: 120 }}>
        {product.images[0] && (
          <Image
            source={{ uri: imageUrl(product.images[0].url)! }}
            className="w-full h-56 rounded-xl mb-4 bg-accentSoft"
            resizeMode="cover"
          />
        )}

        <View className="flex-row justify-between items-start mb-3">
          <View className="flex-1 pr-3">
            <Text className="text-2xl font-bold text-ink">{product.name}</Text>
            {product.description && (
              <Text className="text-muted mt-2">{product.description}</Text>
            )}
          </View>
          <Pressable onPress={toggleFavorite} className="p-2">
            <Ionicons
              name={isFavorited ? "heart" : "heart-outline"}
              size={28}
              color={isFavorited ? "#B0202E" : "#6B6B6B"}
            />
          </Pressable>
        </View>

        <View className="flex-row items-center gap-2 mb-4">
          {product.originalPrice != null && product.originalPrice > product.price && (
            <Text className="text-lg text-muted line-through">
              {product.originalPrice.toFixed(2)}
            </Text>
          )}
          <Text className="text-xl font-bold text-accent">
            {product.price.toFixed(2)} PLN
          </Text>
          {product.isEventPrice && (
            <Text className="text-sm text-accent">Cena wydarzenia</Text>
          )}
        </View>

        {productAllergens.length > 0 && (
          <View className="mb-4">
            <Text className="text-sm text-muted mb-2">Alergeny</Text>
            <View className="flex-row flex-wrap gap-2">
              {productAllergens.map((a) => (
                <View
                  key={a.id}
                  className="bg-accentSoft rounded-full px-3 py-1"
                >
                  <Text className="text-ink text-sm">{a.name}</Text>
                </View>
              ))}
            </View>
          </View>
        )}

        <View className="flex-row items-center gap-4 mb-4">
          <Text className="text-ink">Ilość</Text>
          <Pressable
            onPress={() => setQuantity(Math.max(1, quantity - 1))}
            className="w-9 h-9 rounded-lg bg-accentSoft items-center justify-center"
          >
            <Text className="text-ink text-lg">−</Text>
          </Pressable>
          <Text className="text-ink text-lg w-6 text-center">{quantity}</Text>
          <Pressable
            onPress={() => setQuantity(quantity + 1)}
            className="w-9 h-9 rounded-lg bg-accentSoft items-center justify-center"
          >
            <Text className="text-ink text-lg">+</Text>
          </Pressable>
        </View>

        <Button
          label="Dodaj do zamówienia"
          onPress={() => {
            add({
              productId: product.id,
              productName: product.name,
              unitPrice: product.price,
              quantity,
            });
            if (router.canGoBack()) router.back();
            else router.replace("/(tabs)/menu");
          }}
        />
      </ScrollView>
    </SafeAreaView>
  );
}
