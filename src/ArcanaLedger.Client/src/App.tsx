import { useState } from 'react';
import { Dashboard } from './components/dashboard/Dashboard';
import { Trades } from './components/trades/Trades';
import { Valuation } from './components/valuation/Valuation';
import { Configuration } from './components/configuration/Configuration';
import './styles/dashboard.css';
import './styles/trades.css';
import './styles/valuation.css';
import './styles/configuration.css';

function App() {
  const [page, setPage] = useState('Home');
  if (page === 'Trades') return <Trades onNavigate={setPage} />;
  if (page === 'Valuation') return <Valuation onNavigate={setPage} />;
  if (page === 'Configuration') return <Configuration onNavigate={setPage} />;
  return <Dashboard onNavigate={setPage} />;
}

export default App;
