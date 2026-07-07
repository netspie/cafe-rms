/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./app/**/*.{js,jsx,ts,tsx}", "./components/**/*.{js,jsx,ts,tsx}"],
  presets: [require("nativewind/preset")],
  theme: {
    extend: {
      colors: {
        accent: "#E23A53",
        accentSoft: "#FCE7EC",
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
