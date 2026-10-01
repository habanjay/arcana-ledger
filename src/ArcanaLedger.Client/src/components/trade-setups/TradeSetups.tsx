import { useState } from 'react';
import { NavigationRail } from '../layout/NavigationRail';
import { TopBar } from '../layout/TopBar';
import {
  defaultTradeSetup,
  SetupReferencePanel,
  TradeSetupPanel,
  type TradeSetup,
} from '../configuration/Configuration';

interface TradeSetupsProps { onNavigate: (label: string) => void; }

export function TradeSetups({ onNavigate }: TradeSetupsProps) {
  const [setup, setSetup] = useState<TradeSetup>(defaultTradeSetup);
  const [saved, setSaved] = useState(false);
  const reset = () => { setSetup(defaultTradeSetup); setSaved(false); };
  const save = () => { setSaved(true); window.setTimeout(() => setSaved(false), 1400); };

  return <div className="app"><NavigationRail activeLabel="Trade setups" onNavigate={onNavigate} /><main className="workspace configuration-workspace"><TopBar title="Trade Setups" onLogout={() => onNavigate('Login')} /><section className="configuration-content" aria-label="Trade setup library"><div className="page-heading"><div><h1>Trade Setup Library</h1><p>Define repeatable position and momentum setups before logging a trade.</p></div><div className="heading-actions"><button className="ghost" type="button" onClick={() => onNavigate('Setup playbook')}>Open Execution Playbook</button><button className="ghost" type="button" onClick={reset}>Reset Setup</button><button className="primary" type="button" onClick={save}>{saved ? 'Saved' : 'Save Setup'}</button></div></div><div className="trade-setups-layout"><TradeSetupPanel setup={setup} onChange={(key, value) => { setSetup((current) => ({ ...current, [key]: value })); setSaved(false); }} /><SetupReferencePanel /></div></section></main></div>;
  return <div className="app"><NavigationRail activeLabel="Trade setups" onNavigate={onNavigate} /><main className="workspace configuration-workspace"><TopBar title="Trade Setups" onLogout={() => onNavigate('Login')} /><section className="configuration-content" aria-label="Trade setup library"><div className="page-heading"><div><h1>Trade Setup Library</h1><p>Define repeatable position and momentum setups before logging a trade.</p></div><div className="heading-actions"><button className="ghost" type="button" onClick={() => onNavigate('Setup playbook')}>Open Execution Playbook</button><button className="ghost" type="button" onClick={reset}>Reset Setup</button><button className="primary" type="button" onClick={save}>{saved ? 'Saved' : 'Save Setup'}</button></div></div><div className="trade-setups-layout"><TradeSetupPanel setup={setup} onChange={(key, value) => { setSetup((current) => ({ ...current, [key]: value })); setSaved(false); }} /><SetupReferencePanel onNavigate={onNavigate} /></div></section></main></div>;
}
