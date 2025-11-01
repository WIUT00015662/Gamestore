import { useEffect, useState, type ChangeEvent } from 'react';
import { api } from '../api/client';
import type { Game, Genre, Platform, Publisher } from '../types';

type GameForm = {
  id?: string;
  name: string;
  key: string;
  description: string;
  price: number;
  unitInStock: number;
  discount: number;
  publisher: string;
  genres: string[];
  platforms: string[];
};

const emptyForm: GameForm = {
  name: '',
  key: '',
  description: '',
  price: 0,
  unitInStock: 0,
  discount: 0,
  publisher: '',
  genres: [],
  platforms: [],
};

function toggle(values: string[], value: string) {
  return values.includes(value)
    ? values.filter((x) => x !== value)
    : [...values, value];
}

export function GamesManagementPage() {
  const [games, setGames] = useState<Game[]>([]);
  const [genres, setGenres] = useState<Genre[]>([]);
  const [platforms, setPlatforms] = useState<Platform[]>([]);
  const [publishers, setPublishers] = useState<Publisher[]>([]);
  const [form, setForm] = useState<GameForm>(emptyForm);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const load = async () => {
    try {
      const [gamesResponse, genresResponse, platformsResponse, publishersResponse] = await Promise.all([
        api.getAllGamesRaw(),
        api.getGenres(),
        api.getPlatforms(),
        api.getPublishers(),
      ]);

      setGames(gamesResponse);
      setGenres(genresResponse);
      setPlatforms(platformsResponse);
      setPublishers(publishersResponse);
      setError('');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load games management data');
    }
  };

  useEffect(() => {
    void load();
  }, []);

  const clearForm = () => {
    setForm(emptyForm);
  };

  const createGame = async () => {
    try {
      await api.createGame({
        game: {
          name: form.name,
          key: form.key,
          description: form.description,
          price: form.price,
          unitInStock: form.unitInStock,
          discount: form.discount,
        },
        genres: form.genres,
        platforms: form.platforms,
        publisher: form.publisher,
      });

      setMessage('Game created.');
      clearForm();
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Create game failed');
    }
  };

  const updateGame = async () => {
    if (!form.id) {
      return;
    }

    try {
      await api.updateGame({
        game: {
          id: form.id,
          name: form.name,
          key: form.key,
          description: form.description,
          price: form.price,
          unitInStock: form.unitInStock,
          discount: form.discount,
        },
        genres: form.genres,
        platforms: form.platforms,
        publisher: form.publisher,
      });

      setMessage('Game updated.');
      clearForm();
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Update game failed');
    }
  };

  const selectGame = (game: Game) => {
    setForm((old) => ({
      ...old,
      id: game.id,
      name: game.name,
      key: game.key,
      description: game.description ?? '',
      price: game.price,
      unitInStock: game.unitInStock,
      discount: game.discount,
    }));
  };

  const deleteGame = async (key: string) => {
    try {
      await api.deleteGame(key);
      setMessage('Game deleted.');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Delete game failed');
    }
  };

  return (
    <section className="section">
      <h2>Games management</h2>
      <p className="muted">Create, update and delete games.</p>

      <div className="card-grid">
        <article className="card">
          <h3>Existing games</h3>
          <ul className="list compact">
            {games.map((game: Game) => (
              <li key={game.id}>
                <span>{game.name}</span>
                <div className="row-actions">
                  <button type="button" className="btn-small" onClick={() => selectGame(game)}>Edit</button>
                  <button type="button" className="btn-small danger" onClick={() => void deleteGame(game.key)}>Delete</button>
                </div>
              </li>
            ))}
          </ul>
        </article>

        <article className="card">
          <h3>{form.id ? 'Edit game' : 'Create game'}</h3>
          <div className="form">
            <input value={form.name} onChange={(e: ChangeEvent<HTMLInputElement>) => setForm((old) => ({ ...old, name: e.target.value }))} placeholder="Name" />
            <input value={form.key} onChange={(e: ChangeEvent<HTMLInputElement>) => setForm((old) => ({ ...old, key: e.target.value }))} placeholder="Key" />
            <textarea value={form.description} onChange={(e: ChangeEvent<HTMLTextAreaElement>) => setForm((old) => ({ ...old, description: e.target.value }))} placeholder="Description" />
            <input type="number" value={form.price} onChange={(e: ChangeEvent<HTMLInputElement>) => setForm((old) => ({ ...old, price: Number(e.target.value) }))} placeholder="Price" />
            <input type="number" value={form.unitInStock} onChange={(e: ChangeEvent<HTMLInputElement>) => setForm((old) => ({ ...old, unitInStock: Number(e.target.value) }))} placeholder="Stock" />
            <input type="number" value={form.discount} onChange={(e: ChangeEvent<HTMLInputElement>) => setForm((old) => ({ ...old, discount: Number(e.target.value) }))} placeholder="Discount" />

            <select value={form.publisher} onChange={(e: ChangeEvent<HTMLSelectElement>) => setForm((old) => ({ ...old, publisher: e.target.value }))}>
              <option value="">Select publisher</option>
              {publishers.map((publisher: Publisher) => (
                <option key={publisher.id} value={publisher.id}>{publisher.companyName}</option>
              ))}
            </select>

            <div className="check-grid">
              {genres.map((genre: Genre) => (
                <label key={genre.id}>
                  <input
                    type="checkbox"
                    checked={form.genres.includes(genre.id)}
                    onChange={() => setForm((old) => ({ ...old, genres: toggle(old.genres, genre.id) }))}
                  />
                  {genre.name}
                </label>
              ))}
            </div>

            <div className="check-grid">
              {platforms.map((platform: Platform) => (
                <label key={platform.id}>
                  <input
                    type="checkbox"
                    checked={form.platforms.includes(platform.id)}
                    onChange={() => setForm((old) => ({ ...old, platforms: toggle(old.platforms, platform.id) }))}
                  />
                  {platform.type}
                </label>
              ))}
            </div>

            {form.id ? (
              <button type="button" className="btn" onClick={() => void updateGame()}>Update game</button>
            ) : (
              <button type="button" className="btn" onClick={() => void createGame()}>Create game</button>
            )}
            <button type="button" className="btn-small" onClick={clearForm}>Clear</button>
          </div>
        </article>
      </div>

      {message ? <p>{message}</p> : null}
      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}
