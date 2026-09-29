export type TradeResult = 'up' | 'down' | 'neutral';

export interface Trade {
  id: string;
  symbol: string;
  setup: string;
  shares: number;
  date: string;
  entryPrice: string;
  cutLoss: string;
  targetPrice: string;
  result: TradeResult;
  quote: string;
  change: string;
}

export interface NewTrade {
  date: string;
  orderType: 'Long' | 'Short';
  symbol: string;
  setup: string;
  entryPrice: string;
  cutLoss: string;
  targetPrice: string;
  shares: string;
  comments: string;
}