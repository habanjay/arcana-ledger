import { useState } from 'react';
import { Dashboard } from './components/dashboard/Dashboard';
import { Trades } from './components/trades/Trades';
import { Valuation } from './components/valuation/Valuation';
import { Configuration } from './components/configuration/Configuration';
import { TradeSetups } from './components/trade-setups/TradeSetups';
import { Analytics } from './components/analytics/Analytics';
import { TradeReview } from './components/trade-review/TradeReview';
import { SetupPlaybook } from './components/setup-playbook/SetupPlaybook';
import { Login } from './components/login/Login';
import { CreateAccount } from './components/create-account/CreateAccount';
import { mockAuthProvider } from './auth/mockAuthProvider';
import type { AuthenticatedUser } from './auth/authentication';
import './styles/dashboard.css';
import './styles/trades.css';
import './styles/valuation.css';
import './styles/configuration.css';
import './styles/login.css';
import './styles/create-account.css';
import './styles/typography.css';
import './styles/analytics.css';
import './styles/trade-review.css';
import './styles/setup-playbook.css';

function App() {
  const [page, setPage] = useState('Login');
  const [user, setUser] = useState<AuthenticatedUser | null>(null);
  const navigate = (label: string) => {
    if (label === 'Login') setUser(null);
    setPage(label);
  };
  if (page === 'Login') return <Login authProvider={mockAuthProvider} onAuthenticated={(authenticatedUser) => { setUser(authenticatedUser); setPage('Dashboard'); }} onNavigate={navigate} />;
  if (page === 'Create account') return <CreateAccount onNavigate={navigate} />;
  if (page === 'Trades') return <Trades onNavigate={navigate} />;
  if (page === 'Trade review') return <TradeReview onNavigate={navigate} />;
  if (page === 'Trade setups') return <TradeSetups onNavigate={navigate} />;
  if (page === 'Setup playbook') return <SetupPlaybook onNavigate={navigate} />;
  if (page === 'Analytics') return <Analytics onNavigate={navigate} />;
  if (page === 'Valuation') return <Valuation onNavigate={navigate} />;
  if (page === 'Configuration') return <Configuration onNavigate={navigate} />;
  return <Dashboard user={user} onNavigate={navigate} />;
}

export default App;
