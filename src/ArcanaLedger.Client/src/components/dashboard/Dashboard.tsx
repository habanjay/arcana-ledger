import { useState } from 'react';
import { activity, navigationItems, watchlist } from '../../data/dashboardData';
import { ActivityFeed } from './ActivityFeed';
import { NavigationRail } from '../layout/NavigationRail';
import { PerformanceChart } from './PerformanceChart';
import { QuickActions } from './QuickActions';
import { SectionHeading } from '../layout/SectionHeading';
import { TopBar } from '../layout/TopBar';
import { Watchlist } from './Watchlist';
import type { AuthenticatedUser } from '../../auth/authentication';

export function Dashboard({ user, onNavigate }: { user: AuthenticatedUser | null; onNavigate?: (label: string) => void }) {
  const [railExpanded, setRailExpanded] = useState(false);
  const [selectedSymbol, setSelectedSymbol] = useState('AAPL');

  return <div className={railExpanded ? 'app nav-expanded' : 'app'}>
    <NavigationRail items={navigationItems} activeLabel="Home" expanded={railExpanded} onToggle={() => setRailExpanded((current) => !current)} onNavigate={onNavigate} />
    <main className="workspace">
      <TopBar onLogout={() => onNavigate?.('Login')} />
      <section className="home" aria-label="Trading home overview">
        <section className="welcome"><div className="welcome-copy"><p className="eyebrow">SUNDAY, SEPTEMBER 27, 2026</p><h1>Good morning, {user?.name ?? 'Alex'}</h1><p>Your portfolio is steady today. Review active positions, check market signals, or refine your next setup.</p><button className="primary-action" type="button" onClick={() => document.querySelector('.chart-panel')?.scrollIntoView({ behavior: 'smooth', block: 'center' })}>Review portfolio</button></div></section>
        <section className="stats" aria-label="Portfolio highlights"><article className="panel stat"><span>Portfolio value</span><strong>C$256,104.41</strong><small className="positive">+2.84% this month</small></article><article className="panel stat"><span>Available cash</span><strong>C$84,514.46</strong><small>33% of portfolio</small></article><article className="panel stat"><span>Open positions</span><strong>12</strong><small className="positive">3 need attention</small></article><article className="panel stat"><span>Today's result</span><strong className="negative">(C$803.48)</strong><small className="negative">-0.31% today</small></article></section>
        <div className="columns">
          <div className="column"><section className="panel chart-panel"><SectionHeading title="Portfolio performance" subtitle="Value across the last 30 trading days" action="30D ⌄" /><PerformanceChart /></section><ActivityFeed items={activity} /></div>
          <div className="column"><Watchlist items={watchlist} selectedSymbol={selectedSymbol} onSelect={setSelectedSymbol} /><QuickActions onNavigate={onNavigate ?? (() => undefined)} /></div>
        </div>
      </section>
    </main>
  </div>;
}
