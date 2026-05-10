import { ActivityIndicator, Pressable, Text } from "react-native";

type Props = {
  label: string;
  onPress: () => void;
  loading?: boolean;
  disabled?: boolean;
  variant?: "primary" | "secondary" | "danger";
};

export function Button({ label, onPress, loading, disabled, variant = "primary" }: Props) {
  const isDisabled = disabled || loading;
  const base = "py-3 px-4 rounded-md items-center";
  const styles = {
    primary: "bg-accent",
    secondary: "bg-bgSoft border border-border",
    danger: "bg-danger",
  };
  const textStyles = {
    primary: "text-white font-semibold",
    secondary: "text-ink font-semibold",
    danger: "text-white font-semibold",
  };
  return (
    <Pressable
      onPress={onPress}
      disabled={isDisabled}
      className={`${base} ${styles[variant]} ${isDisabled ? "opacity-50" : ""}`}
    >
      {loading ? (
        <ActivityIndicator color={variant === "secondary" ? "#111" : "#fff"} />
      ) : (
        <Text className={textStyles[variant]}>{label}</Text>
      )}
    </Pressable>
  );
}
