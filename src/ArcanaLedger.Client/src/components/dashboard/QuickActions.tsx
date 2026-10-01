import { quickActions } from '../../data/dashboardData';
import { SectionHeading } from '../layout/SectionHeading';

interface QuickActionsProps { onNavigate: (label: string) => void; }

const actionRoutes: Record<string, string> = { 'Run valuation': 'Valuation', 'Configure risk': 'Configuration', 'Log a trade': 'Trades', 'View analytics': 'Analytics', 'Open setup playbook': 'Setup playbook' };

export function QuickActions({ onNavigate }: QuickActionsProps) {
  return <section className="panel quick"><SectionHeading title="Quick actions" subtitle="Jump into your daily workflow" /><div className="quick-grid">{quickActions.map(([icon, label]) => <button className="quick-button" key={label} type="button" onClick={() => onNavigate(actionRoutes[label] ?? 'Dashboard')}><b>{icon}</b><span>{label}</span></button>)}</div></section>;
}
