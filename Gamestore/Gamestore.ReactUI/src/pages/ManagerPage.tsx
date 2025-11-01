import { useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client';

export function ManagerPage() {
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const runPolling = async () => {
    try {
      const result = await api.triggerPolling();
      setMessage(`Polling complete. Discounted games: ${result.totalDiscountedGames}`);
      setError('');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Polling failed');
    }
  };

  return (
    <section className="section">
      <h2>Manager panel</h2>
      <p className="muted">Manage catalog and operational workflows.</p>

      <div className="toolbar">
        <button type="button" className="btn" onClick={() => void runPolling()}>
          Run discount polling
        </button>
        <Link to="/games" className="btn-link">
          Open games catalog
        </Link>
        <Link to="/manager/games" className="btn-link">
          Manage games
        </Link>
        <Link to="/manager/entities" className="btn-link">
          Manage entities
        </Link>
        <Link to="/manager/orders" className="btn-link">
          Manage orders
        </Link>
      </div>

      {message ? <p>{message}</p> : null}
      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}
