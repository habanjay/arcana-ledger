import { NavigationRail } from '../layout/NavigationRail';
import { TopBar } from '../layout/TopBar';

interface AnalyticsProps { onNavigate: (label: string) => void; }

const performance = {
  allocation: 'PHP 226,307.49',
  runningBalance: 'PHP 227,359.82',
  returnValue: 'PHP 1,052.33',
  returnPercent: '0.47%',
  totalTrades: '0',
  wins: '2',
  losses: '0',
  totalGain: 'PHP 1,052.33',
  averageGain: 'PHP 526.17',
  totalLoss: 'PHP 0.00',
  averageLoss: 'PHP 0.00',
  hitRatio: 'NA',
  edgeRatio: 'NA',
};

function MetricRow({ label, value, emphasized = false }: { label: string; value: string; emphasized?: boolean }) {
  return <div className={emphasized ? 'analytics-metric emphasized' : 'analytics-metric'}><span>{label}</span><strong>{value}</strong></div>;
}

function PieCard({ title, description, centerLabel, centerDetail, className, legend }: { title: string; description: string; centerLabel: string; centerDetail: string; className: string; legend: Array<{ label: string; color: string }> }) {
  return <figure className="analytics-pie-card"><figcaption><strong>{title}</strong><span>{description}</span></figcaption><div className={`analytics-pie ${className}`} role="img" aria-label={`${title}: ${centerLabel} ${centerDetail}`}><div><strong>{centerLabel}</strong><span>{centerDetail}</span></div></div><div className="analytics-legend">{legend.map((item) => <span key={item.label}><i style={{ backgroundColor: item.color }} />{item.label}</span>)}</div></figure>;
}

export function Analytics({ onNavigate }: AnalyticsProps) {
  return <div className="app"><NavigationRail activeLabel="Analytics" onNavigate={onNavigate} /><main className="workspace analytics-workspace"><TopBar title="Analytics" onLogout={() => onNavigate('Login')} /><section className="analytics-content" aria-label="Trade performance analytics"><div className="page-heading"><div><h1>Trade Performance</h1><p>Review returns, win rate, and the results of your configured trade setups.</p></div></div><div className="analytics-layout"><section className="panel analytics-summary"><MetricRow label="Starting allocation" value={performance.allocation} /><MetricRow label="Running balance" value={performance.runningBalance} emphasized /><MetricRow label="Net return" value={performance.returnValue} /><MetricRow label="Return rate" value={performance.returnPercent} /><div className="analytics-divider" /><MetricRow label="Total trades" value={performance.totalTrades} /><MetricRow label="Winning trades" value={performance.wins} /><MetricRow label="Losing trades" value={performance.losses} /><div className="analytics-divider" /><MetricRow label="Total gain" value={performance.totalGain} /><MetricRow label="Average gain" value={performance.averageGain} /><MetricRow label="Total loss" value={performance.totalLoss} /><MetricRow label="Average loss" value={performance.averageLoss} /><div className="analytics-ratios"><MetricRow label="Hit ratio" value={performance.hitRatio} /><MetricRow label="Edge ratio" value={performance.edgeRatio} /></div></section><div className="analytics-charts"><PieCard title="Trade outcomes" description="Winning vs. losing trades" centerLabel={performance.wins} centerDetail="wins" className="trades-pie" legend={[{ label: 'Winning trades', color: '#43b8ff' }, { label: 'Losing trades', color: '#695e7c' }]} /><PieCard title="Return contribution" description="Allocation vs. generated return" centerLabel={performance.returnPercent} centerDetail="net return" className="return-pie" legend={[{ label: 'Allocation', color: '#a96be9' }, { label: 'Return', color: '#43b8ff' }]} /></div></div></section></main></div>;
}
