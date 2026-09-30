import { useState } from 'react';
import type { NavigationItem } from '../../types/dashboard';

interface NavigationRailProps {
  items?: NavigationItem[];
  expanded?: boolean;
  onToggle?: () => void;
  activeLabel?: string;
  onNavigate?: (label: string) => void;
}

const defaultItems: NavigationItem[] = [{ label: 'Home', icon: '⌂' }, { label: 'Trades', icon: '↗' }, { label: 'Trade review', icon: '✓' }, { label: 'Trade setups', icon: '◈' }, { label: 'Analytics', icon: '◔' }, { label: 'Valuation', icon: '▣' }, { label: 'Configuration', icon: '⚙' }];

export function NavigationRail({ items = defaultItems, expanded, onToggle, activeLabel = 'Home', onNavigate = () => undefined }: NavigationRailProps) {
  const [internalExpanded, setInternalExpanded] = useState(false);
  const isControlled = expanded !== undefined || onToggle !== undefined;
  const isExpanded = isControlled ? expanded ?? false : internalExpanded;
  const toggle = () => {
    if (isControlled) onToggle?.();
    else setInternalExpanded((current) => !current);
  };

  return (
    <aside className={isExpanded ? 'rail expanded' : 'rail'}>
      <div className="brand" aria-label="Arcana Ledger" />
      <nav className="nav" aria-label="Primary navigation">
        {items.map((item) => <button className={item.label === activeLabel ? 'nav-button active' : 'nav-button'} key={item.label} type="button" onClick={() => onNavigate(item.label)}><span aria-hidden="true">{item.icon}</span><span className="nav-label">{item.label}</span></button>)}
      </nav>
      <button className="rail-toggle" type="button" onClick={toggle} aria-label={isExpanded ? 'Collapse navigation' : 'Expand navigation'} aria-expanded={isExpanded}>‹</button>
    </aside>
  );
}
