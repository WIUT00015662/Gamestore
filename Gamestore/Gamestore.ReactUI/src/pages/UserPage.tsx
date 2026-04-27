import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client';
import { hasPermission } from '../auth';
import type { Order } from '../types';

export function UserPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [error, setError] = useState('');
  const canViewHistory = hasPermission('ViewOrderHistory');

  useEffect(() => {
    const load = async () => {
      if (!canViewHistory) {
        return;
      }

      try {
        setOrders(await api.getMyOrders());
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load orders');
      }
    };

    void load();
  }, [canViewHistory]);

  return (
    <section className="section">
      <h2>User dashboard</h2>
      <p className="muted">Personal actions for regular customers.</p>
      <div className="toolbar">
        <Link to="/games" className="btn-link">Browse games</Link>
        <Link to="/cart" className="btn-link">Open cart</Link>
      </div>

      {canViewHistory ? (
        <div>
          <h3>Order history</h3>
          <ul className="list compact">
            {orders.map((order: Order) => (
              <li key={order.id}>
                <span>{order.id}</span>
                <span>{order.date ?? 'N/A'}</span>
              </li>
            ))}
          </ul>
        </div>
      ) : (
        <p className="muted">Order history is not available for this role.</p>
      )}

      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}
