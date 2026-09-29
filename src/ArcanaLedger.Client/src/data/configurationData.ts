import type { ConfigurationField, ConfigurationOptionGroup, ConfigurationSection, ConfigurationValues } from '../types/configuration';

export const configurationDefaults: ConfigurationValues = {
  capital: '$256,104.41', varRate: '0.25%', cashValue: '$84,514.46', portfolioShare: '33% of Portfolio', reserve: '$25,610.44', reserveShare: '10% of Portfolio',
  positionEntry: '$62.90', positionExit: '$60.00', positionShares: '110', positionCash: '$6,943.52', trancheEntry: '$70.80', trancheExit: '$66.60', trancheShares: '76', trancheCash: '$5,396.49',
  swingEntry: '$41.80', swingExit: '$39.90', swingShares: '337', swingCash: '$14,085.74', breakout: '$63.00', patternLow: '$60.00', targetPrice: '$66.00', momentum: '5.50% of Portfolio',
};

export const portfolioFields: ConfigurationField[] = [
  { key: 'capital', label: 'Capital' }, { key: 'varRate', label: 'Portfolio VAR' }, { key: 'cashValue', label: 'Cash Value', tone: 'negative' },
  { key: 'portfolioShare', label: 'Cash Allocation' }, { key: 'reserve', label: 'Trading Reserve', tone: 'negative' }, { key: 'reserveShare', label: 'Reserve Allocation' },
];

export const positionSections: ConfigurationSection[] = [
  { title: 'Pre-trade - 1st tranche', fields: [{ key: 'positionEntry', label: 'Entry', tone: 'positive' }, { key: 'positionExit', label: 'Exit', tone: 'negative' }, { key: 'positionShares', label: 'Number of shares' }, { key: 'positionCash', label: 'Cash value' }] },
  { title: 'Pre-trade - 2nd tranche', fields: [{ key: 'trancheEntry', label: 'Entry', tone: 'positive' }, { key: 'trancheExit', label: 'Exit', tone: 'negative' }, { key: 'trancheShares', label: 'Number of shares' }, { key: 'trancheCash', label: 'Cash value' }] },
];

export const swingSections: ConfigurationSection[] = [
  { title: 'Momentum setup', fields: [{ key: 'swingEntry', label: 'Entry', tone: 'positive' }, { key: 'swingExit', label: 'Exit', tone: 'negative' }, { key: 'swingShares', label: 'Total number of shares' }, { key: 'swingCash', label: 'Cash value' }] },
  { title: 'Target price', fields: [{ key: 'breakout', label: 'Breakout', tone: 'positive' }, { key: 'patternLow', label: 'Pattern low', tone: 'negative' }, { key: 'targetPrice', label: 'Target price' }, { key: 'momentum', label: 'Momentum' }] },
];

export const optionGroups: ConfigurationOptionGroup[] = [
  { label: 'Order type', options: ['Long', 'Short', 'Sell', 'Cover'], initial: 'Long' },
  { label: 'Trend', options: ['Up', 'Down'], initial: 'Up' },
  { label: 'Setup', options: ['P - Coil', 'P - Base 0 Duration', 'S - Pullback', 'S - Blue Sky Breakout', 'M - Expansion Breakout', 'M - Slingshot', 'M - Jack in the Box', 'M - 180 Degrees', 'M - Boomer', 'M - 1,2,3 Pullback', 'C - Capitulation', 'C - Square Root', 'Others'], initial: 'P - Coil' },
];