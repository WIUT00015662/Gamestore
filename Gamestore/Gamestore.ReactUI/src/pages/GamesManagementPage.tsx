import { useEffect, useState, type ChangeEvent } from 'react';
import { api } from '../api/client';
import type { Game, GameVendorOfferInput, Genre, Platform, Publisher } from '../types';

type GameForm = {
  id?: string;
  name: string;
  key: string;
  description: string;
  unitInStock: number;
  publisher: string;
  genres: string[];
  platforms: string[];
  vendorOffers: GameVendorOfferInput[];
};

const emptyForm: GameForm = {
  name: '',
  key: '',
  description: '',
  unitInStock: 0,
  publisher: '',
  genres: [],
  platforms: [],
  vendorOffers: [],
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
        api.getAllGamesForManagement(),
        api.getGenres(),
        api.getPlatforms(),
        api.getPublishers(),
      ]);

      setGames(gamesResponse.games);
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
      if (form.vendorOffers.length === 0) {
        setError('At least one seller offer is required.');
        return;
      }

      await api.createGame({
        game: {
          name: form.name,
          key: form.key,
          description: form.description,
          unitInStock: form.unitInStock,
        },
        vendorOffers: form.vendorOffers.map(({ vendor, purchaseUrl, price, referencePrice }) => ({
          vendor,
          purchaseUrl,
          price,
          referencePrice,
        })),
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
      if (form.vendorOffers.length === 0) {
        setError('At least one seller offer is required.');
        return;
      }

      await api.updateGame({
        game: {
          id: form.id,
          name: form.name,
          key: form.key,
          description: form.description,
          unitInStock: form.unitInStock,
        },
        vendorOffers: form.vendorOffers.map(({ vendor, purchaseUrl, price, referencePrice }) => ({
          vendor,
          purchaseUrl,
          price,
          referencePrice,
        })),
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

  const selectGame = async (game: Game) => {
    try {
      const [offers, gameGenres, gamePlatforms, gamePublisher] = await Promise.all([
        api.getOffers(game.key),
        api.getGameGenres(game.key),
        api.getGamePlatforms(game.key),
        api.getGamePublisher(game.key),
      ]);
      setForm((old) => ({
        ...old,
        id: game.id,
        name: game.name,
        key: game.key,
        description: game.description ?? '',
        unitInStock: game.unitInStock,
        publisher: gamePublisher.id,
        genres: gameGenres.map((genre) => genre.id),
        platforms: gamePlatforms.map((platform) => platform.id),
        vendorOffers: offers.map((offer) => ({
          clientId: crypto.randomUUID(),
          vendor: offer.vendor,
          purchaseUrl: offer.purchaseUrl,
          price: offer.price,
          referencePrice: offer.price,
        })),
      }));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load game offers');
    }
  };

  const updateOffer = (index: number, next: Partial<GameVendorOfferInput>) => {
    setForm((old) => ({
      ...old,
      vendorOffers: old.vendorOffers.map((offer, i) => (i === index ? { ...offer, ...next } : offer)),
    }));
  };

  const addOffer = () => {
    setForm((old) => ({
      ...old,
      vendorOffers: [
        ...old.vendorOffers,
        { clientId: crypto.randomUUID(), vendor: '', purchaseUrl: '', price: 0, referencePrice: 0 },
      ],
    }));
  };

  const removeOffer = (index: number) => {
    setForm((old) => ({
      ...old,
      vendorOffers: old.vendorOffers.filter((_, i) => i !== index),
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
            <label>
              Name
              <input value={form.name} onChange={(e: ChangeEvent<HTMLInputElement>) => setForm((old) => ({ ...old, name: e.target.value }))} placeholder="Name" />
            </label>
            <label>
              Key
              <input value={form.key} onChange={(e: ChangeEvent<HTMLInputElement>) => setForm((old) => ({ ...old, key: e.target.value }))} placeholder="Key" />
            </label>
            <label>
              Description
              <textarea value={form.description} onChange={(e: ChangeEvent<HTMLTextAreaElement>) => setForm((old) => ({ ...old, description: e.target.value }))} placeholder="Description" />
            </label>
            <label>
              Stock
              <input type="number" value={form.unitInStock} onChange={(e: ChangeEvent<HTMLInputElement>) => setForm((old) => ({ ...old, unitInStock: Number(e.target.value) }))} placeholder="Stock" />
            </label>

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

            <div className="form">
              <h4>Seller offers</h4>
              {form.vendorOffers.map((offer, index) => (
                <div key={offer.clientId ?? `${offer.vendor}-${index}`} className="card form">
                  <label>
                    Seller
                    <input
                      value={offer.vendor}
                      onChange={(e: ChangeEvent<HTMLInputElement>) => updateOffer(index, { vendor: e.target.value })}
                      placeholder="Vendor name"
                    />
                  </label>
                  <label>
                    Purchase URL
                    <input
                      value={offer.purchaseUrl}
                      onChange={(e: ChangeEvent<HTMLInputElement>) => updateOffer(index, { purchaseUrl: e.target.value })}
                      placeholder="https://example.com"
                    />
                  </label>
                  <label>
                    Current price
                    <input
                      type="number"
                      value={offer.price}
                      onChange={(e: ChangeEvent<HTMLInputElement>) => updateOffer(index, { price: Number(e.target.value) })}
                      placeholder="Price"
                    />
                  </label>
                  <label>
                    Reference price
                    <input
                      type="number"
                      value={offer.referencePrice}
                      onChange={(e: ChangeEvent<HTMLInputElement>) => updateOffer(index, { referencePrice: Number(e.target.value) })}
                      placeholder="Reference price"
                    />
                  </label>
                  <button type="button" className="btn-small danger" onClick={() => removeOffer(index)}>Remove offer</button>
                </div>
              ))}
              <button type="button" className="btn-small" onClick={addOffer}>Add offer</button>
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
