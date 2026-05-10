/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./app/**/*.{js,jsx,ts,tsx}", "./components/**/*.{js,jsx,ts,tsx}"],
  presets: [require("nativewind/preset")],
  theme: {
    extend: {
      colors: {
        accent: "#2A6F4D",
        accentSoft: "#E8F1EB",
        ink: "#111111",
        muted: "#6B6B6B",
        border: "#E5E5E5",
        bg: "#FFFFFF",
        bgSoft: "#F7F7F7",
        danger: "#B0202E",
      },
    },
  },
  plugins: [],
};
