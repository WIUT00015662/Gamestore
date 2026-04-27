import { useEffect, useState } from 'react';
import { api } from '../api/client';
import type { Deal } from '../types';

export function DealsPage() {
  const [deals, setDeals] = useState<Deal[]>([]);
  const [error, setError] = useState('');

  useEffect(() => {
    const load = async () => {
      try {
        setDeals(await api.getAllDeals());
        setError('');
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load discounts');
      }
    };

    void load();
  }, []);

  return (
    <section className="section">
      <div className="section-header">
        <h2>All current discounts</h2>
      </div>

      {deals.length === 0 ? <p className="muted">No discounted games yet.</p> : null}

      <div className="card-grid">
        {deals.map((deal) => (
          <article className="card card-clickable" key={`${deal.gameId}-${deal.vendor}`} onClick={() => window.open(deal.purchaseUrl, '_blank', 'noreferrer')}>
            <p className="muted">Game</p>
            <h3>{deal.gameName}</h3>
            <p className="muted">Vendor</p>
            <p>{deal.vendor}</p>
            <p className="muted">Discount</p>
            <p>
              <strong>{deal.discountPercent}% off</strong>
            </p>
            <p className="muted">Price</p>
            <p>
              ${deal.discountedPrice.toFixed(2)} <span className="muted">(was ${deal.originalPrice.toFixed(2)})</span>
            </p>
          </article>
        ))}
      </div>

      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}
