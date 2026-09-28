import { quickActions } from '../../data/dashboardData';
import { SectionHeading } from '../layout/SectionHeading';

interface QuickActionsProps { actionMessage: string; onAction: (label: string) => void; }

export function QuickActions({ actionMessage, onAction }: QuickActionsProps) {
  return <section className="panel quick"><SectionHeading title="Quick actions" subtitle="Jump into your daily workflow" /><div className="quick-grid">{quickActions.map(([icon, label]) => <button className="quick-button" key={label} type="button" onClick={() => onAction(label)}><b>{icon}</b><span>{label}</span></button>)}</div>{actionMessage && <p className="action-status" role="status">{actionMessage}</p>}</section>;
}
