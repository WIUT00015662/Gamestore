import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client';
import { getPrimaryRole, hasPermission } from '../auth';
import type { Deal, Game } from '../types';

export function HomePage() {
  const [deals, setDeals] = useState<Deal[]>([]);
  const [games, setGames] = useState<Game[]>([]);
  const [error, setError] = useState('');

  useEffect(() => {
    const load = async () => {
      try {
        const [featuredDeals, gamesResponse] = await Promise.all([api.getFeaturedDeals(), api.getGames('', 1)]);
        setDeals(featuredDeals);
        setGames(gamesResponse.games);
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load home data');
      }
    };

    void load();
  }, []);

  const canPoll = hasPermission('ManageEntities');

  const handlePoll = async () => {
    try {
      await api.triggerPolling();
      const featuredDeals = await api.getFeaturedDeals();
      setDeals(featuredDeals);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Polling failed');
    }
  };

  const role = getPrimaryRole();
  const capabilities = useMemo(
    () => [
      { title: 'Browse games', enabled: true },
      { title: 'Buy games', enabled: hasPermission('BuyGame') },
      { title: 'Comment', enabled: hasPermission('CommentGame') },
      { title: 'Moderate comments', enabled: hasPermission('ManageComments') },
      { title: 'Manage entities', enabled: hasPermission('ManageEntities') },
      { title: 'Manage users', enabled: hasPermission('ManageUsers') },
      { title: 'Manage roles', enabled: hasPermission('ManageRoles') },
    ],
    [],
  );

  return (
    <div>
      <section className="section">
        <h2>Current access</h2>
        <p className="muted">Role: {role}</p>
        <ul className="capabilities">
          {capabilities.map((item) => (
            <li key={item.title} className={item.enabled ? 'enabled' : 'disabled'}>
              {item.title}
            </li>
          ))}
        </ul>
      </section>

      <section className="section">
        <div className="section-header">
          <h2>Top discounted deals</h2>
          {canPoll && (
            <button type="button" className="btn" onClick={handlePoll}>
              Poll deals now
            </button>
          )}
        </div>
        {deals.length === 0 ? <p className="muted">No featured deals yet.</p> : null}
        <div className="card-grid">
          {deals.map((deal) => (
            <article className="card" key={`${deal.gameId}-${deal.vendor}`}>
              <h3>{deal.gameName}</h3>
              <p className="muted">{deal.vendor}</p>
              <p>
                <strong>{deal.discountPercent}% off</strong>
              </p>
              <p>
                ${deal.discountedPrice.toFixed(2)} <span className="muted">(was ${deal.originalPrice.toFixed(2)})</span>
              </p>
              <a href={deal.purchaseUrl} target="_blank" rel="noreferrer">
                Open store
              </a>
            </article>
          ))}
        </div>
      </section>

      <section className="section">
        <h2>Games</h2>
        <div className="card-grid">
          {games.map((game) => (
            <article className="card" key={game.id}>
              <h3>{game.name}</h3>
              <p className="muted">{game.key}</p>
              <p>${game.price.toFixed(2)}</p>
              <Link to={`/games/${game.key}`}>Details</Link>
            </article>
          ))}
        </div>
      </section>

      {error ? <p className="error">{error}</p> : null}
    </div>
  );
}
