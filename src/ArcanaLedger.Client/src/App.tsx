import { useState } from 'react';
import { Dashboard } from './components/dashboard/Dashboard';
import { Trades } from './components/trades/Trades';
import './styles/dashboard.css';
import './styles/trades.css';

function App() {
  const [page, setPage] = useState('Home');
  return page === 'Trades' ? <Trades onNavigate={setPage} /> : <Dashboard onNavigate={setPage} />;
}

export default App;
