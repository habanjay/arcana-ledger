import { useState } from 'react';
import { trades } from '../../data/tradesData';
import type { Trade } from '../../types/trades';
import { NavigationRail } from '../layout/NavigationRail';
import { TopBar } from '../layout/TopBar';

type ReviewOutcome = 'Win' | 'Loss' | 'Break-even' | 'Open';

interface TradeReviewEntry {
  outcome: ReviewOutcome;
  whatWentWrong: string;
  mistake: string;
  learning: string;
  improvements: string;
}

const blankReview: TradeReviewEntry = {
  outcome: 'Open',
  whatWentWrong: '',
  mistake: '',
  learning: '',
  improvements: '',
};

const starterReviews: Record<string, TradeReviewEntry> = {
  'iart-03-may': { outcome: 'Win', whatWentWrong: 'Entry was slightly late after the breakout candle.', mistake: 'I waited for confirmation instead of planning the trigger in advance.', learning: 'A prepared trigger keeps the entry closer to the intended risk.', improvements: 'Set the alert at the breakout level and place the plan before the session opens.' },
  'cmg-03-may': { outcome: 'Loss', whatWentWrong: 'Price rejected the level and momentum faded quickly.', mistake: 'I entered without a clean volume confirmation.', learning: 'A familiar setup is not enough when participation is missing.', improvements: 'Require above-average volume and define the invalidation level before entry.' },
  'dct-07-may': { outcome: 'Loss', whatWentWrong: 'The position moved against the setup immediately.', mistake: 'I sized from conviction instead of the planned VAR limit.', learning: 'Position size must stay tied to risk, especially on uncertain breakouts.', improvements: 'Calculate shares from the stop distance before sending the order.' },
  'vicr-07-may': { outcome: 'Win', whatWentWrong: 'The first partial exit was taken too early.', mistake: 'I protected the gain before the setup reached its target zone.', learning: 'Let the trade work when price and volume still confirm the thesis.', improvements: 'Use the planned target and trail only after the first objective is reached.' },
  'vicr-08-may': { outcome: 'Win', whatWentWrong: 'No major issue; execution followed the plan.', mistake: '', learning: 'A simple breakout plan works when entry, risk, and target are defined.', improvements: 'Keep the same checklist and document the setup before entry.' },
  'sfly-08-may': { outcome: 'Loss', whatWentWrong: 'The breakout failed back into the base.', mistake: 'I ignored the weakening market context because the chart looked attractive.', learning: 'Context can invalidate an otherwise clean pattern.', improvements: 'Check sector strength and the broader market trend before entering.' },
};

function defaultReview(trade: Trade): TradeReviewEntry {
  return starterReviews[trade.id] ?? { ...blankReview, outcome: trade.result === 'up' ? 'Win' : trade.result === 'down' ? 'Loss' : 'Open' };
}

function outcomeClass(outcome: ReviewOutcome) {
  return outcome === 'Win' ? 'review-win' : outcome === 'Loss' ? 'review-loss' : 'review-neutral';
}

export function TradeReview({ onNavigate }: { onNavigate: (label: string) => void }) {
  const [selectedTradeId, setSelectedTradeId] = useState(trades[0]?.id ?? '');
  const [reviews, setReviews] = useState<Record<string, TradeReviewEntry>>(starterReviews);
  const [saved, setSaved] = useState(false);
  const selectedTrade = trades.find((trade) => trade.id === selectedTradeId) ?? trades[0];
  const review = selectedTrade ? reviews[selectedTrade.id] ?? defaultReview(selectedTrade) : blankReview;
  const reviewedCount = trades.filter((trade) => reviews[trade.id]?.learning || reviews[trade.id]?.improvements).length;

  const updateReview = (key: keyof TradeReviewEntry, value: string) => {
    if (!selectedTrade) return;
    setReviews((current) => ({ ...current, [selectedTrade.id]: { ...review, [key]: value } }));
    setSaved(false);
  };

  const saveReview = () => {
    if (!selectedTrade) return;
    setReviews((current) => ({ ...current, [selectedTrade.id]: review }));
    setSaved(true);
    window.setTimeout(() => setSaved(false), 1400);
  };

  const resetReview = () => {
    if (!selectedTrade) return;
    setReviews((current) => ({ ...current, [selectedTrade.id]: { ...blankReview, outcome: selectedTrade.result === 'up' ? 'Win' : selectedTrade.result === 'down' ? 'Loss' : 'Open' } }));
    setSaved(false);
  };

  return <div className="app">
    <NavigationRail activeLabel="Trade review" onNavigate={onNavigate} />
    <main className="workspace review-workspace">
      <TopBar title="Trade Review" onLogout={() => onNavigate('Login')} />
      <section className="review-content" aria-label="Trade review journal">
        <div className="page-heading"><div><h1>Trade Review</h1><p>Turn every result into a repeatable lesson for the next trade.</p></div><div className="heading-actions"><button className="ghost" type="button" onClick={resetReview}>Reset Review</button><button className="primary" type="button" onClick={saveReview}>{saved ? 'Saved' : 'Save Review'}</button></div></div>
        <section className="review-summary" aria-label="Review progress"><article className="panel review-stat"><span>Trades reviewed</span><strong>{reviewedCount}<small> / {trades.length}</small></strong><em>{reviewedCount === trades.length ? 'Complete' : 'Keep building the journal'}</em></article><article className="panel review-stat"><span>Wins reviewed</span><strong className="positive">{Object.values(reviews).filter((item) => item.outcome === 'Win').length}</strong><em>What should be repeated?</em></article><article className="panel review-stat"><span>Losses reviewed</span><strong className="negative">{Object.values(reviews).filter((item) => item.outcome === 'Loss').length}</strong><em>What should change?</em></article></section>
        <div className="review-layout">
          <section className="panel review-trade-list"><div className="panel-head"><div><h2>Recent trades</h2><p>Select a trade to review its decisions and result.</p></div><button className="review-link" type="button" onClick={() => onNavigate('Trades')}>View all trades</button></div><div className="review-list">{trades.map((trade) => { const tradeReview = reviews[trade.id] ?? defaultReview(trade); return <button className={trade.id === selectedTradeId ? 'review-trade-row selected' : 'review-trade-row'} key={trade.id} type="button" onClick={() => { setSelectedTradeId(trade.id); setSaved(false); }}><span className="review-symbol">{trade.symbol.slice(0, 2)}</span><span><strong>{trade.symbol}</strong><small>{trade.setup} · {trade.date}</small><small>{trade.entryPrice} · {trade.quote}</small></span><span className={outcomeClass(tradeReview.outcome)}>{tradeReview.outcome}</span></button>; })}</div></section>
          {selectedTrade && <section className="panel review-editor"><div className="review-editor-head"><div><span className="control-label">Reviewing {selectedTrade.date}</span><h2>{selectedTrade.symbol} <small>{selectedTrade.setup}</small></h2><p>Entry {selectedTrade.entryPrice} · Result {selectedTrade.quote} · {selectedTrade.change}</p></div><span className={`review-badge ${outcomeClass(review.outcome)}`}>{review.outcome}</span></div><div className="review-form"><div className="field"><label htmlFor="review-outcome">Outcome</label><select id="review-outcome" value={review.outcome} onChange={(event) => updateReview('outcome', event.target.value)}><option>Win</option><option>Loss</option><option>Break-even</option><option>Open</option></select></div><div className="field"><label htmlFor="review-wrong">What went wrong?</label><textarea id="review-wrong" rows={3} value={review.whatWentWrong} onChange={(event) => updateReview('whatWentWrong', event.target.value)} placeholder="Describe the setup, execution, or context that did not go as planned." /></div><div className="field"><label htmlFor="review-mistake">What was my mistake?</label><textarea id="review-mistake" rows={3} value={review.mistake} onChange={(event) => updateReview('mistake', event.target.value)} placeholder="Name the decision or behavior you would change." /></div><div className="field"><label htmlFor="review-learning">What did I learn?</label><textarea id="review-learning" rows={3} value={review.learning} onChange={(event) => updateReview('learning', event.target.value)} placeholder="Write the principle you want to remember." /></div><div className="field"><label htmlFor="review-improvements">What needs to improve?</label><textarea id="review-improvements" rows={3} value={review.improvements} onChange={(event) => updateReview('improvements', event.target.value)} placeholder="Turn the lesson into a specific checklist action." /></div></div></section>}
        </div>
      </section>
    </main>
  </div>;
}
