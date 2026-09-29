export interface ValuationStock {
  name: string;
  sector: string;
  revenue: number;
  netIncome: number;
  shares: number;
  growth: number;
  margin: number;
  multiple: number;
  price: number;
  y1Revenue: number;
  y1Income: number;
}

export interface ValuationInputs {
  revenue: number;
  netIncome: number;
  shares: number;
  growth: number;
  margin: number;
  multiple: number;
  price: number;
}

export type ValuationInputKey = keyof ValuationInputs;