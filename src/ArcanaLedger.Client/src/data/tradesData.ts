import type { Trade } from '../types/trades';

export const trades: Trade[] = [
  { id: 'iart-03-may', symbol: 'IART', setup: 'M - Boomer', shares: 300, date: '3-May', entryPrice: '$62.41', cutLoss: '—', targetPrice: '—', result: 'up', quote: '$548.52', change: '2.93%' },
  { id: 'cmg-03-may', symbol: 'CMG', setup: 'M - Boomer', shares: 100, date: '3-May', entryPrice: '$422.65', cutLoss: '—', targetPrice: '—', result: 'down', quote: '($505.98)', change: '-1.20%' },
  { id: 'dct-07-may', symbol: 'DCT', setup: 'M - Boomer', shares: 600, date: '7-May', entryPrice: '$66.40', cutLoss: '—', targetPrice: '—', result: 'down', quote: '($258.99)', change: '-0.65%' },
  { id: 'vicr-07-may', symbol: 'VICR', setup: 'S - Blue Sky Breakout', shares: 600, date: '7-May', entryPrice: '$35.00', cutLoss: '—', targetPrice: '—', result: 'up', quote: '$1,109.42', change: '5.28%' },
  { id: 'vicr-08-may', symbol: 'VICR', setup: 'S - Blue Sky Breakout', shares: 600, date: '8-May', entryPrice: '$35.83', cutLoss: '—', targetPrice: '—', result: 'up', quote: '$611.42', change: '2.84%' },
  { id: 'sfly-08-may', symbol: 'SFLY', setup: 'M - Slingshot', shares: 900, date: '8-May', entryPrice: '$94.10', cutLoss: '—', targetPrice: '—', result: 'down', quote: '($353.06)', change: '-0.42%' },
];