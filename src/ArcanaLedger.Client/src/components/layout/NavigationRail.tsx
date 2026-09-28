import type { NavigationItem } from '../../types/dashboard';

interface NavigationRailProps {
  items?: NavigationItem[];
  expanded?: boolean;
  onToggle?: () => void;
  activeLabel?: string;
  onNavigate?: (label: string) => void;
}

const defaultItems: NavigationItem[] = [{ label: 'Home', icon: '⌂' }, { label: 'Trades', icon: '↗' }, { label: 'Valuation', icon: '▣' }, { label: 'Configuration', icon: '⚙' }];

export function NavigationRail({ items = defaultItems, expanded = false, onToggle = () => undefined, activeLabel = 'Home', onNavigate = () => undefined }: NavigationRailProps) {
  return (
    <aside className="rail">
      <div className="brand" aria-label="Arcana Ledger" />
      <nav className="nav" aria-label="Primary navigation">
        {items.map((item) => <button className={item.label === activeLabel ? 'nav-button active' : 'nav-button'} key={item.label} type="button" onClick={() => onNavigate(item.label)}><span aria-hidden="true">{item.icon}</span><span className="nav-label">{item.label}</span></button>)}
      </nav>
      <button className="nav-button logout" type="button"><span aria-hidden="true">↪</span><span className="nav-label">Log out</span></button>
      <button className="rail-toggle" type="button" onClick={onToggle} aria-label={expanded ? 'Collapse navigation' : 'Expand navigation'} aria-expanded={expanded}>‹</button>
    </aside>
  );
}
