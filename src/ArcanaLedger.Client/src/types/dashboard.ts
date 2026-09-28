export type Tone = 'positive' | 'negative' | 'neutral';

export interface WatchlistItem {
  symbol: string;
  name: string;
  sector: string;
  price: string;
  change: string;
  tone: Tone;
}

export interface ActivityItem {
  title: string;
  detail: string;
  time: string;
}

export interface NavigationItem {
  label: string;
  icon: string;
}
