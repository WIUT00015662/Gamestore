import { useEffect, useState, type ChangeEvent } from 'react';
import { api } from '../api/client';
import type { Genre, Platform, Publisher } from '../types';

export function EntitiesPage() {
  const [genres, setGenres] = useState<Genre[]>([]);
  const [platforms, setPlatforms] = useState<Platform[]>([]);
  const [publishers, setPublishers] = useState<Publisher[]>([]);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const [genreName, setGenreName] = useState('');
  const [platformType, setPlatformType] = useState('');
  const [publisher, setPublisher] = useState({ companyName: '', homePage: '', description: '' });

  const load = async () => {
    try {
      const [g, p, pub] = await Promise.all([api.getGenres(), api.getPlatforms(), api.getPublishers()]);
      setGenres(g);
      setPlatforms(p);
      setPublishers(pub);
      setError('');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load entities');
    }
  };

  useEffect(() => {
    void load();
  }, []);

  const createGenre = async () => {
    try {
      await api.createGenre({ genre: { name: genreName } });
      setGenreName('');
      setMessage('Genre created.');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Create genre failed');
    }
  };

  const createPlatform = async () => {
    try {
      await api.createPlatform({ platform: { type: platformType } });
      setPlatformType('');
      setMessage('Platform created.');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Create platform failed');
    }
  };

  const createPublisher = async () => {
    try {
      await api.createPublisher({ publisher });
      setPublisher({ companyName: '', homePage: '', description: '' });
      setMessage('Publisher created.');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Create publisher failed');
    }
  };

  return (
    <section className="section">
      <h2>Entities management</h2>
      <p className="muted">Manage genres, platforms and publishers.</p>

      <div className="card-grid">
        <article className="card">
          <h3>Genres</h3>
          <ul className="list compact">
            {genres.map((item: Genre) => (
              <li key={item.id}>
                <span>{item.name}</span>
                <button type="button" className="btn-small danger" onClick={() => void api.deleteGenre(item.id).then(load)}>
                  Delete
                </button>
              </li>
            ))}
          </ul>
          <div className="form">
            <input value={genreName} onChange={(e: ChangeEvent<HTMLInputElement>) => setGenreName(e.target.value)} placeholder="New genre" />
            <button type="button" className="btn" onClick={() => void createGenre()}>Add</button>
          </div>
        </article>

        <article className="card">
          <h3>Platforms</h3>
          <ul className="list compact">
            {platforms.map((item: Platform) => (
              <li key={item.id}>
                <span>{item.type}</span>
                <button type="button" className="btn-small danger" onClick={() => void api.deletePlatform(item.id).then(load)}>
                  Delete
                </button>
              </li>
            ))}
          </ul>
          <div className="form">
            <input value={platformType} onChange={(e: ChangeEvent<HTMLInputElement>) => setPlatformType(e.target.value)} placeholder="New platform" />
            <button type="button" className="btn" onClick={() => void createPlatform()}>Add</button>
          </div>
        </article>

        <article className="card">
          <h3>Publishers</h3>
          <ul className="list compact">
            {publishers.map((item: Publisher) => (
              <li key={item.id}>
                <span>{item.companyName}</span>
                <button type="button" className="btn-small danger" onClick={() => void api.deletePublisher(item.id).then(load)}>
                  Delete
                </button>
              </li>
            ))}
          </ul>
          <div className="form">
            <input
              value={publisher.companyName}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setPublisher((old) => ({ ...old, companyName: e.target.value }))}
              placeholder="Company"
            />
            <input
              value={publisher.homePage}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setPublisher((old) => ({ ...old, homePage: e.target.value }))}
              placeholder="Home page"
            />
            <textarea
              value={publisher.description}
              onChange={(e: ChangeEvent<HTMLTextAreaElement>) => setPublisher((old) => ({ ...old, description: e.target.value }))}
              placeholder="Description"
            />
            <button type="button" className="btn" onClick={() => void createPublisher()}>Add</button>
          </div>
        </article>
      </div>

      {message ? <p>{message}</p> : null}
      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}
