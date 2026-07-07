// Sakura + gold family — pinks, cherry reds, rose, warm gold/amber.
const PALETTE = [
  "#E23A53", // sakura red
  "#D14D74", // rose
  "#B84A7E", // plum rose
  "#E8748E", // soft sakura pink
  "#E0803C", // gold
  "#C98A2E", // honey amber
  "#E0685A", // coral
];

const MatchaGreen = "#5E8C3E";

export function eventColor(seed: string): string {
  if (seed.toLowerCase().includes("matcha")) return MatchaGreen;
  let hash = 0;
  for (let i = 0; i < seed.length; i++) hash = (hash * 31 + seed.charCodeAt(i)) >>> 0;
  return PALETTE[hash % PALETTE.length];
}
