import type { ActivityItem, NavigationItem, WatchlistItem } from '../types/dashboard';

export const watchlist: WatchlistItem[] = [
  { symbol: 'AAPL', name: 'Apple Inc.', sector: 'Consumer technology', price: 'C$47.00', change: '+1.82%', tone: 'positive' },
  { symbol: 'MSFT', name: 'Microsoft Corp.', sector: 'Software & cloud', price: 'C$430.00', change: '+0.74%', tone: 'positive' },
  { symbol: 'NVDA', name: 'NVIDIA Corp.', sector: 'Semiconductors', price: 'C$177.00', change: '-1.36%', tone: 'negative' },
  { symbol: 'AMZN', name: 'Amazon.com Inc.', sector: 'Commerce & cloud', price: 'C$225.00', change: '+2.10%', tone: 'positive' },
];

export const activity: ActivityItem[] = [
  { title: 'NVDA position updated', detail: 'Added 40 shares at C$177.20', time: '12 min ago' },
  { title: 'Valuation saved', detail: 'AAPL target price set to C$69.41', time: '1 hr ago' },
  { title: 'Risk threshold reached', detail: 'Daily loss is nearing your limit', time: '3 hrs ago' },
];

export const navigationItems: NavigationItem[] = [
  { label: 'Home', icon: '⌂' },
  { label: 'Trades', icon: '↗' },
  { label: 'Trade setups', icon: '◈' },
  { label: 'Analytics', icon: '◔' },
  { label: 'Valuation', icon: '▣' },
  { label: 'Configuration', icon: '⚙' },
];

export const quickActions = [
  ['▣', 'Run valuation'],
  ['⚙', 'Configure risk'],
  ['↗', 'Log a trade'],
  ['◔', 'View analytics'],
] as const;
