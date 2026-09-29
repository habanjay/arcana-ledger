import type { ValuationInputs, ValuationStock } from '../types/valuation';

export const valuationStocks: Record<string, ValuationStock> = {
  AAPL: { name: 'Apple Inc.', sector: 'Consumer technology', revenue: 124423, netIncome: 6105, shares: 2765.4, growth: 8, margin: 5, multiple: 21, price: 47, y1Revenue: 112589, y1Income: 5526 },
  MSFT: { name: 'Microsoft Corp.', sector: 'Software & cloud', revenue: 245122, netIncome: 88100, shares: 7430, growth: 11, margin: 36, multiple: 29, price: 430, y1Revenue: 211915, y1Income: 72361 },
  NVDA: { name: 'NVIDIA Corp.', sector: 'Semiconductors', revenue: 130497, netIncome: 72880, shares: 24500, growth: 28, margin: 45, multiple: 35, price: 177, y1Revenue: 60922, y1Income: 29760 },
  AMZN: { name: 'Amazon.com Inc.', sector: 'Commerce & cloud', revenue: 637959, netIncome: 59248, shares: 10700, growth: 12, margin: 10, multiple: 28, price: 225, y1Revenue: 574785, y1Income: 30425 },
};

export const valuationDefaults: ValuationInputs = {
  revenue: valuationStocks.AAPL.revenue,
  netIncome: valuationStocks.AAPL.netIncome,
  shares: valuationStocks.AAPL.shares,
  growth: valuationStocks.AAPL.growth,
  margin: valuationStocks.AAPL.margin,
  multiple: valuationStocks.AAPL.multiple,
  price: valuationStocks.AAPL.price,
};