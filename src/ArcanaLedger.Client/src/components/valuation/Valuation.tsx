import { useState } from 'react';
import { valuationDefaults, valuationStocks } from '../../data/valuationData';
import type { ValuationInputKey, ValuationInputs } from '../../types/valuation';
import { NavigationRail } from '../layout/NavigationRail';
import { TopBar } from '../layout/TopBar';

interface ValuationProps { onNavigate: (label: string) => void; }

const inputFields: Array<{ key: ValuationInputKey; label: string; step: string }> = [
  { key: 'revenue', label: 'Estimated revenue (Y0), in millions', step: '1' },
  { key: 'netIncome', label: 'Estimated net income (Y0), in millions', step: '1' },
  { key: 'shares', label: 'Outstanding shares, in millions', step: '0.01' },
  { key: 'growth', label: 'Revenue growth (5-year CAGR)', step: '0.1' },
  { key: 'margin', label: 'Terminal net margin', step: '0.1' },
  { key: 'multiple', label: 'Comparable P/E', step: '0.1' },
  { key: 'price', label: 'Price today', step: '0.01' },
];

const formatMoney = (value: number, decimals = 2) => `$${value.toFixed(decimals)}`;
const formatMillions = (value: number) => Math.round(value).toLocaleString();

function buildForecast(stock: typeof valuationStocks.AAPL, inputs: ValuationInputs) {
  const revenue = [stock.y1Revenue, inputs.revenue];
  const income = [stock.y1Income, inputs.netIncome];
  for (let year = 1; year <= 5; year += 1) {
    revenue.push(revenue[year] * (1 + inputs.growth / 100));
    income.push(revenue[year + 1] * inputs.margin / 100);
  }
  return { revenue, income };
}

export function Valuation({ onNavigate }: ValuationProps) {
  const [selectedSymbol, setSelectedSymbol] = useState('AAPL');
  const [inputs, setInputs] = useState<ValuationInputs>(valuationDefaults);
  const [saved, setSaved] = useState(false);
  const stock = valuationStocks[selectedSymbol];
  const forecast = buildForecast(stock, inputs);
  const futureEps = inputs.shares ? forecast.income[6] / inputs.shares : 0;
  const target = futureEps * inputs.multiple;
  const upside = inputs.price ? (target / inputs.price - 1) * 100 : 0;
  const currentMargin = inputs.revenue ? inputs.netIncome / inputs.revenue * 100 : 0;

  const selectStock = (symbol: string) => {
    const nextStock = valuationStocks[symbol];
    setSelectedSymbol(symbol);
    setInputs({ revenue: nextStock.revenue, netIncome: nextStock.netIncome, shares: nextStock.shares, growth: nextStock.growth, margin: nextStock.margin, multiple: nextStock.multiple, price: nextStock.price });
  };

  const updateInput = (key: ValuationInputKey, value: string) => {
    setInputs((current) => ({ ...current, [key]: Number(value) || 0 }));
    setSaved(false);
  };

  const saveValuation = () => {
    setSaved(true);
    window.setTimeout(() => setSaved(false), 1400);
  };

  return <div className="app">
    <NavigationRail activeLabel="Valuation" onNavigate={onNavigate} />
    <main className="workspace valuation-workspace">
      <TopBar title="Valuation" onLogout={() => onNavigate('Login')} />
      <section className="valuation-content" aria-label="Company valuation model">
        <div className="page-heading"><div><h1>Company Valuation</h1><p>Estimate fair value from earnings growth, margins, and comparable multiples.</p></div><div className="heading-actions"><button className="ghost" type="button" onClick={() => { setInputs(valuationDefaults); setSelectedSymbol('AAPL'); setSaved(false); }}>Reset Model</button><button className="primary" type="button" onClick={saveValuation}>{saved ? 'Saved' : 'Save Valuation'}</button></div></div>
        <div className="valuation-layout">
          <div className="column">
            <section className="panel"><div className="panel-head"><div><h2>Operating Forecast - {selectedSymbol}</h2><p>Historical results and projected earnings, in millions</p></div><span className="badge">5-Year Model</span></div><div className="panel-body forecast-wrap"><table className="forecast"><caption className="sr-only">Operating forecast for {selectedSymbol}</caption><thead><tr><th>Fiscal year</th>{['Y-1', 'Y0', 'Y+1E', 'Y+2E', 'Y+3E', 'Y+4E', 'Y+5E'].map((year) => <th key={year}>{year}</th>)}</tr></thead><tbody><tr><td>Revenue</td>{forecast.revenue.map((value, index) => <td key={`revenue-${index}`}>{formatMillions(value)}</td>)}</tr><tr><td>Net income</td>{forecast.income.map((value, index) => <td key={`income-${index}`}>{formatMillions(value)}</td>)}</tr><tr><td>Revenue growth</td><td>—</td><td>{((inputs.revenue / stock.y1Revenue - 1) * 100).toFixed(1)}%</td>{forecast.revenue.slice(2).map((_, index) => <td key={`growth-${index}`}>{inputs.growth.toFixed(1)}%</td>)}</tr><tr><td>Net margin</td>{forecast.income.map((value, index) => <td key={`margin-${index}`}>{(value / forecast.revenue[index] * 100).toFixed(1)}%</td>)}</tr><tr><td>EPS</td>{forecast.income.map((value, index) => <td key={`eps-${index}`}>{(value / inputs.shares || 0).toFixed(2)}</td>)}</tr></tbody></table></div></section>
            <section className="panel"><div className="panel-head"><div><h2>Valuation Summary</h2><p>Current market price compared with modeled fair value</p></div><div className="status"><i />Model ready</div></div><div className="panel-body"><div className="metric-grid"><div className="metric"><span>Price today</span><strong>{formatMoney(inputs.price)}</strong></div><div className="metric"><span>Target price (Year +5)</span><strong className="positive">{formatMoney(target)}</strong></div><div className="metric"><span>Upside</span><strong className={upside >= 0 ? 'positive' : 'negative'}>{upside.toFixed(1)}%</strong></div><div className="metric"><span>Implied EPS</span><strong>{formatMoney(futureEps)}</strong></div></div><p className="note">Target price = estimated Year +5 EPS x comparable P/E.</p></div></section>
          </div>
          <div className="column">
            <section className="panel summary"><h2>Valuation Snapshot</h2><p>Estimated fair value based on current assumptions</p><div className="summary-value">{formatMoney(target)}<small className={upside >= 0 ? 'positive' : 'negative'}>{upside >= 0 ? '+' : ''}{upside.toFixed(1)}% upside to price today</small></div><div className="summary-line"><span>Comparable P/E</span><strong>{inputs.multiple.toFixed(2)}x</strong></div><div className="summary-line"><span>Discount rate</span><strong>8.00%</strong></div></section>
            <section className="panel control"><h2>Stocks</h2><div className="stock-list" aria-label="Select a stock for valuation">{Object.entries(valuationStocks).map(([symbol, item]) => <button className={symbol === selectedSymbol ? 'stock-button selected' : 'stock-button'} key={symbol} type="button" onClick={() => selectStock(symbol)}><span className="stock-symbol">{symbol}</span><span><span className="stock-name">{item.name}</span><span className="stock-sector">{item.sector}</span></span><span className="stock-price">{formatMoney(item.price)}</span></button>)}</div></section>
            <section className="panel control"><h2>Model Inputs</h2><div className="assumption-list">{inputFields.map((field) => <div className="assumption" key={field.key}><label htmlFor={`valuation-${field.key}`}>{field.label}</label><input id={`valuation-${field.key}`} type="number" min="0" step={field.step} value={inputs[field.key]} onChange={(event) => updateInput(field.key, event.target.value)} /></div>)}</div></section>
            <section className="panel control"><h2>Input Assumptions</h2><div className="metric-grid"><div className="metric"><span>Actual revenue</span><strong>{formatMoney(stock.y1Revenue, 0)}M</strong></div><div className="metric"><span>Actual net income</span><strong>{formatMoney(stock.y1Income, 0)}M</strong></div><div className="metric"><span>Current margin</span><strong>{currentMargin.toFixed(1)}%</strong></div><div className="metric"><span>Forecast period</span><strong>5 years</strong></div></div><p className="note">Last updated today at 9:42 AM</p></section>
          </div>
        </div>
      </section>
    </main>
  </div>;
}