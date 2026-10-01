import { useState } from 'react';
import { setupCategoryClass, setupPlaybooks, type SetupPlaybook as SetupPlaybookRecord } from '../../data/setupPlaybookData';
import { NavigationRail } from '../layout/NavigationRail';
import { TopBar } from '../layout/TopBar';

interface SetupPlaybookProps { onNavigate: (label: string) => void; }

function Chart({ chart }: { chart: SetupPlaybookRecord['chart'] }) {
  return <div className="playbook-chart-wrap"><svg viewBox="0 0 600 230" preserveAspectRatio="none" role="img" aria-label="Sample price chart showing entry, cutloss, and target">
    <g className="playbook-chart-grid">{[30, 80, 130, 180, 220].map((y) => <line key={`h-${y}`} x1="34" y1={y} x2="586" y2={y} />)}{[100, 200, 300, 400, 500].map((x) => <line key={`v-${x}`} x1={x} y1="4" x2={x} y2="226" />)}</g>
    <line className="playbook-target-line" x1="34" y1={chart.targetY} x2="586" y2={chart.targetY} /><line className="playbook-cutloss-line" x1="34" y1={chart.cutlossY} x2="586" y2={chart.cutlossY} /><line className="playbook-entry-line" x1={chart.entry.x} y1="4" x2={chart.entry.x} y2="226" />
    <polyline className="playbook-ema-line" points={chart.ema} /><polyline className="playbook-price-line" points={chart.price} />
      <circle cx={chart.entry.x} cy={chart.entry.y} r="5" fill="#ffb632" /><circle cx={chart.entry.x} cy={chart.entry.y} r="10" fill="#ffb632" opacity=".2" />
    <text className="playbook-target-label" x="40" y={chart.targetY - 6} fill="#31d7c4">{chart.targetLabel}</text><text className="playbook-cutloss-label" x="40" y={chart.cutlossY - 6} fill="#ff8095">{chart.cutlossLabel}</text><text className="playbook-entry-label" x={chart.entry.x + 7} y={chart.entry.y - 7} fill="#fff1c2">{chart.entry.label}</text>
  </svg><div className="playbook-chart-legend"><span><i className="price" />Price</span><span><i className="ema" />{chart.emaLabel}</span><span><b />Entry trigger</span><span><i className="cutloss" />Cutloss</span><span><i className="target" />Target</span></div></div>;
}

function PanelHeader({ title, subtitle, badge, badgeClass = '' }: { title: string; subtitle?: string; badge?: string; badgeClass?: string }) {
  return <div className="panel-head"><div><h2>{title}</h2>{subtitle && <p>{subtitle}</p>}</div>{badge && <span className={`panel-badge ${badgeClass}`}>{badge}</span>}</div>;
}

export function SetupPlaybook({ onNavigate }: SetupPlaybookProps) {
  const setupKeys = Object.keys(setupPlaybooks);
  const [selectedKey, setSelectedKey] = useState(setupKeys[0]);
  const [checked, setChecked] = useState<Record<string, boolean>>({});
  const [notice, setNotice] = useState('');
  const setup = setupPlaybooks[selectedKey];
  const related = setupKeys.filter((key) => setupPlaybooks[key].category === setup.category);
  const flash = (message: string) => { setNotice(message); window.setTimeout(() => setNotice(''), 1200); };

  return <div className="app"><NavigationRail activeLabel="Trade setups" onNavigate={onNavigate} /><main className="workspace playbook-workspace"><TopBar title="Setup Playbook" onLogout={() => onNavigate('Login')} /><section className="playbook-content" aria-label="Setup execution playbook">
    <div className="page-heading"><div><h1>Setup Execution Playbook</h1><p>When, what, and how to execute this setup with entry, cutloss, and target reference points.</p></div><div className="heading-actions"><select className="playbook-select" aria-label="Select setup" value={selectedKey} onChange={(event) => { setSelectedKey(event.target.value); setChecked({}); }}><optgroup label="Swing">{setupKeys.filter((key) => setupPlaybooks[key].category === 'Swing').map((key) => <option key={key} value={key}>{setupPlaybooks[key].name}</option>)}</optgroup><optgroup label="Momentum">{setupKeys.filter((key) => setupPlaybooks[key].category === 'Momentum').map((key) => <option key={key} value={key}>{setupPlaybooks[key].name}</option>)}</optgroup><optgroup label="Position">{setupKeys.filter((key) => setupPlaybooks[key].category === 'Position').map((key) => <option key={key} value={key}>{setupPlaybooks[key].name}</option>)}</optgroup><optgroup label="Capitulation">{setupKeys.filter((key) => setupPlaybooks[key].category === 'Capitulation').map((key) => <option key={key} value={key}>{setupPlaybooks[key].name}</option>)}</optgroup></select><button className="ghost" type="button" onClick={() => flash('Print view ready')}>Print Playbook</button><button className="primary" type="button" onClick={() => onNavigate('Configuration')}>Edit in Configuration</button></div></div>
    {notice && <p className="playbook-notice" role="status">{notice}</p>}
    <div className="playbook-grid"><div className="column">
      <section className="panel"><PanelHeader title={setup.name} subtitle={`${setup.category} · ${setup.order} · ${setup.derived ? 'Framework-derived entry' : 'Course-documented entry'}`} badge={setup.category} badgeClass={setupCategoryClass[setup.category]} /><div className="panel-body"><p className={setup.derived ? 'source-note derived' : 'source-note'}><span aria-hidden="true">ⓘ</span>{setup.sourceNote}</p><p className="setup-desc">{setup.description}</p><h3 className="section-title">Setup conditions</h3><div className="condition-list">{setup.conditions.map(([title, detail]) => <article className="condition" key={title}><i>✓</i><div><strong>{title}</strong><small>{detail}</small></div></article>)}</div><div className="divider" /><h3 className="section-title">Reference chart</h3><Chart chart={setup.chart} /></div></section>
      <section className="panel"><PanelHeader title="Execution sequence" subtitle="Follow the sequence; do not anticipate the trigger." /><div className="panel-body"><ol className="step-list">{setup.steps.map(([title, detail], index) => <li className="step" key={title}><span className="step-num">{index + 1}</span><div><strong>{title}</strong><p>{detail}</p></div></li>)}</ol></div></section>
      <section className="panel"><PanelHeader title="Execution parameters" subtitle="Reference values for the selected setup." /><div className="panel-body"><div className="form-grid">{Object.entries({ 'Entry trigger': setup.params.entry, Cutloss: setup.params.cutloss, Target: setup.params.target, 'Risk / reward': setup.params.rr, Timeframe: setup.params.timeframe, 'Order type': setup.params.order, 'Position sizing': setup.params.sizing, VAR: setup.params.varPct }).map(([label, value]) => <div className="field" key={label}><label htmlFor={`playbook-${label}`}>{label}</label><input id={`playbook-${label}`} value={value} readOnly className={label === 'Entry trigger' ? 'input-positive' : label === 'Cutloss' ? 'input-negative' : undefined} /></div>)}</div></div></section>
    </div><div className="column">
      <section className="panel playbook-summary"><h2>Setup performance</h2><p>Historical results for {setup.name}</p><div className="summary-grid-mini">{setup.stats.map(([label, value]) => <div className="summary-metric" key={label}><span>{label}</span><strong>{value}</strong></div>)}</div></section>
      <section className="panel control-panel"><h2>Pre-trade checklist</h2><p>Mark each condition before placing the order.</p><div className="checklist">{setup.checklist.map((item) => { const id = `${selectedKey}-${item}`; return <button className={checked[id] ? 'check-item checked' : 'check-item'} type="button" key={item} onClick={() => setChecked((current) => ({ ...current, [id]: !current[id] }))}><span className="box">✓</span><span>{item}</span></button>; })}</div></section>
      <section className="panel control-panel"><h2>Do / do not</h2><div className="doable-columns"><div><h3 className="list-title positive">Do</h3><ul className="do-list">{setup.dos.map((item) => <li key={item}>{item}</li>)}</ul></div><div><h3 className="list-title negative">Do not</h3><ul className="dont-list">{setup.donts.map((item) => <li key={item}>{item}</li>)}</ul></div></div></section>
      <section className="panel control-panel"><h2>Related setups</h2><div className="related-chips">{related.map((key) => <button className={key === selectedKey ? 'chip active' : 'chip'} type="button" key={key} onClick={() => { setSelectedKey(key); setChecked({}); }}>{setupPlaybooks[key].name}</button>)}</div></section>
    </div></div>
  </section></main></div>;
}