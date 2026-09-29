import { useState } from 'react';
import { Dashboard } from './components/dashboard/Dashboard';
import { Trades } from './components/trades/Trades';
import { Valuation } from './components/valuation/Valuation';
import { Configuration } from './components/configuration/Configuration';
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

function App() {
  const [page, setPage] = useState('Login');
  const [user, setUser] = useState<AuthenticatedUser | null>(null);
  if (page === 'Login') return <Login authProvider={mockAuthProvider} onAuthenticated={(authenticatedUser) => { setUser(authenticatedUser); setPage('Dashboard'); }} onNavigate={setPage} />;
  if (page === 'Create account') return <CreateAccount onNavigate={setPage} />;
  if (page === 'Trades') return <Trades onNavigate={setPage} />;
  if (page === 'Valuation') return <Valuation onNavigate={setPage} />;
  if (page === 'Configuration') return <Configuration onNavigate={setPage} />;
  return <Dashboard user={user} onNavigate={setPage} />;
}

export default App;
