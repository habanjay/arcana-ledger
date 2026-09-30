import { useEffect, useState, type FormEvent } from 'react';
import { trades as initialTrades } from '../../data/tradesData';
import type { NewTrade, Trade, TradeResult } from '../../types/trades';
import { NavigationRail } from '../layout/NavigationRail';
import { TopBar } from '../layout/TopBar';

interface TradesProps { onNavigate: (label: string) => void; }

const emptyTrade: NewTrade = { date: '', orderType: 'Long', symbol: '', setup: 'M - Boomer', entryPrice: '', cutLoss: '', targetPrice: '', shares: '', comments: '' };

function resultClass(result: TradeResult) { return result === 'up' ? 'trade-quote up' : result === 'down' ? 'trade-quote down' : 'trade-quote'; }

export function Trades({ onNavigate }: TradesProps) {
  const [tradeList, setTradeList] = useState<Trade[]>(initialTrades);
  const [resultFilter, setResultFilter] = useState<'all' | 'up' | 'down'>('all');
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState<NewTrade>(emptyTrade);
  const [exported, setExported] = useState(false);
  const visibleTrades = tradeList.filter((trade) => resultFilter === 'all' || trade.result === resultFilter);

  useEffect(() => {
    if (!modalOpen) return undefined;
    const closeOnEscape = (event: KeyboardEvent) => { if (event.key === 'Escape') setModalOpen(false); };
    document.addEventListener('keydown', closeOnEscape);
    return () => document.removeEventListener('keydown', closeOnEscape);
  }, [modalOpen]);

  const exportCsv = () => {
    const header = 'Symbol,Setup,Shares,Date,Entry price,Result,Quote,Change';
    const rows = tradeList.map((trade) => [trade.symbol, trade.setup, trade.shares, trade.date, trade.entryPrice, trade.result, trade.quote, trade.change].join(','));
    const url = URL.createObjectURL(new Blob([[header, ...rows].join('\n')], { type: 'text/csv' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = 'arcana-trades.csv';
    link.click();
    URL.revokeObjectURL(url);
    setExported(true);
    window.setTimeout(() => setExported(false), 1200);
  };

  const saveTrade = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const nextTrade: Trade = { id: `${form.symbol}-${Date.now()}`, symbol: form.symbol.toUpperCase(), setup: form.setup, shares: Number(form.shares), date: form.date, entryPrice: `$${Number(form.entryPrice).toFixed(2)}`, cutLoss: form.cutLoss ? `$${Number(form.cutLoss).toFixed(2)}` : '—', targetPrice: form.targetPrice ? `$${Number(form.targetPrice).toFixed(2)}` : '—', result: 'neutral', quote: 'Open', change: 'Pending' };
    setTradeList((current) => [nextTrade, ...current]);
    setForm(emptyTrade);
    setModalOpen(false);
  };

  return <div className="app">
    <NavigationRail activeLabel="Trades" onNavigate={onNavigate} />
    <main className="workspace trades-workspace">
      <TopBar title="Trades" onLogout={() => onNavigate('Login')} />
      <section className="trade-content" aria-label="Trade ledger">
        <div className="page-heading"><div><h1>Trade Ledger</h1><p>Review open and completed positions, execution costs, and realized performance.</p></div><div className="heading-actions"><button className="ghost" type="button" onClick={exportCsv}>{exported ? 'Exported' : 'Export CSV'}</button><button className="primary" type="button" onClick={() => setModalOpen(true)}>Log trade</button></div></div>
        <section className="summary-grid" aria-label="Trade summary"><article className="panel stat"><span>Total trades</span><strong>{tradeList.length}</strong><small>Across 5 symbols</small></article><article className="panel stat"><span>Winning trades</span><strong className="positive">3</strong><small>50.00% win rate</small></article><article className="panel stat"><span>Realized gain / loss</span><strong className="positive">$1,152.41</strong><small>Net of commissions</small></article><article className="panel stat"><span>Capital deployed</span><strong>$42,196.20</strong><small>Peak exposure</small></article></section>
        <section className="panel ledger-panel"><div className="panel-head"><div><h2>Recent trades</h2><p>Entry, close, and target price points for each setup</p></div><div className="filters"><select className="filter" value={resultFilter} onChange={(event) => setResultFilter(event.target.value as 'all' | 'up' | 'down')} aria-label="Filter trades"><option value="all">All trades</option><option value="up">Winners</option><option value="down">Losers</option></select><select className="filter" aria-label="Filter period" defaultValue="May 2026"><option>May 2026</option><option>April 2026</option></select></div></div>
          <div className="trade-list" aria-label="Recent trades">{visibleTrades.map((trade) => <button className="trade-row" key={trade.id} type="button"><span className="trade-symbol">{trade.symbol}</span><span><span className="trade-name">{trade.symbol}</span><span className="trade-meta">{trade.setup} · {trade.shares} shares · {trade.date}</span><span className="trade-points"><b>Entry</b> {trade.entryPrice} · <b>Cutloss</b> {trade.cutLoss} · <b>Target</b> {trade.targetPrice}</span></span><span className={resultClass(trade.result)}>{trade.quote}<small>{trade.result === 'up' ? '↗ ' : trade.result === 'down' ? '↘ ' : ''}{trade.change}</small></span></button>)}</div>
          <div className="table-note"><span aria-live="polite">Showing <strong>{visibleTrades.length} trades</strong> from the current period</span><span>Last updated today at 9:42 AM</span></div>
        </section>
      </section>
    </main>
    {modalOpen && <div className="modal-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) setModalOpen(false); }}><form className="trade-form" onSubmit={saveTrade}><div className="form-head"><div><h2>Log a trade</h2><p>Capture the setup and execution details for a new position.</p></div><button className="close-form" type="button" onClick={() => setModalOpen(false)} aria-label="Close trade form">×</button></div><div className="form-grid">{([['date', 'Entry date', 'e.g. 12-May', 'text'], ['symbol', 'Stock symbol', 'e.g. AAPL', 'text'], ['entryPrice', 'Entry price', '0.00', 'number'], ['cutLoss', 'Cut loss', '0.00', 'number'], ['targetPrice', 'Target price', '0.00', 'number'], ['shares', 'Shares', '0', 'number']] as const).map(([name, label, placeholder, type]) => <div className="field" key={name}><label htmlFor={`trade-${name}`}>{label}</label><input id={`trade-${name}`} type={type} min={type === 'number' ? '0' : undefined} step={type === 'number' ? '0.01' : undefined} placeholder={placeholder} required={name === 'date' || name === 'symbol' || name === 'entryPrice' || name === 'shares'} value={form[name]} onChange={(event) => setForm({ ...form, [name]: event.target.value })} /></div>)}<div className="field"><label htmlFor="order-type">Order type</label><select id="order-type" value={form.orderType} onChange={(event) => setForm({ ...form, orderType: event.target.value as NewTrade['orderType'] })}><option>Long</option><option>Short</option></select></div><div className="field"><label htmlFor="trade-setup">Setup</label><select id="trade-setup" value={form.setup} onChange={(event) => setForm({ ...form, setup: event.target.value })}><option>M - Boomer</option><option>M - Slingshot</option><option>S - Blue Sky Breakout</option><option>Other</option></select></div><div className="field full"><label htmlFor="trade-comments">Comments</label><textarea id="trade-comments" placeholder="Add an execution note or setup context" value={form.comments} onChange={(event) => setForm({ ...form, comments: event.target.value })} /></div></div><div className="form-actions"><button className="ghost" type="button" onClick={() => setModalOpen(false)}>Cancel</button><button className="primary" type="submit">Save trade</button></div></form></div>}
  </div>;
}