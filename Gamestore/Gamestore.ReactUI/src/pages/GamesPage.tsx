import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client';
import type { Game } from '../types';

export function GamesPage() {
  const [games, setGames] = useState<Game[]>([]);
  const [search, setSearch] = useState('');
  const [error, setError] = useState('');

  const load = async (name: string) => {
    try {
      const response = await api.getGames(name, 1);
      setGames(response.games);
      setError('');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load games');
    }
  };

  useEffect(() => {
    void load('');
  }, []);

  return (
    <section className="section">
      <h2>Games catalog</h2>
      <div className="toolbar">
        <input
          placeholder="Search games"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <button className="btn" type="button" onClick={() => void load(search)}>
          Search
        </button>
      </div>

      <div className="card-grid">
        {games.map((game) => (
          <article className="card" key={game.id}>
            <h3>{game.name}</h3>
            <p className="muted">{game.key}</p>
            <p>${game.price.toFixed(2)}</p>
            <Link to={`/games/${game.key}`}>Open</Link>
          </article>
        ))}
      </div>
      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}
