import type { WatchlistItem } from '../../types/dashboard';
import { SectionHeading } from '../layout/SectionHeading';

interface WatchlistProps { items: WatchlistItem[]; selectedSymbol: string; onSelect: (symbol: string) => void; }

export function Watchlist({ items, selectedSymbol, onSelect }: WatchlistProps) {
  return <section className="panel panel-body">
    <SectionHeading title="Watchlist" subtitle="Markets worth watching today" action="Manage" />
    <div className="watchlist">{items.map((item) => <button className={selectedSymbol === item.symbol ? 'watch selected' : 'watch'} key={item.symbol} type="button" onClick={() => onSelect(item.symbol)}><span className="symbol">{item.symbol}</span><span><span className="name">{item.name}</span><span className="sector">{item.sector}</span></span><span className="quote">{item.price}<small className={item.tone}>{item.change}</small></span></button>)}</div>
  </section>;
}
