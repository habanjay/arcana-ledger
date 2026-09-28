export type ConfigurationFieldKey =
  | 'capital' | 'varRate' | 'cashValue' | 'portfolioShare' | 'reserve' | 'reserveShare'
  | 'positionEntry' | 'positionExit' | 'positionShares' | 'positionCash'
  | 'trancheEntry' | 'trancheExit' | 'trancheShares' | 'trancheCash'
  | 'swingEntry' | 'swingExit' | 'swingShares' | 'swingCash'
  | 'breakout' | 'patternLow' | 'targetPrice' | 'momentum';

export type ConfigurationValues = Record<ConfigurationFieldKey, string>;

export interface ConfigurationField { key: ConfigurationFieldKey; label: string; tone?: 'positive' | 'negative'; }
export interface ConfigurationSection { title: string; fields: ConfigurationField[]; }
export interface ConfigurationOptionGroup { label: string; options: string[]; initial: string; }