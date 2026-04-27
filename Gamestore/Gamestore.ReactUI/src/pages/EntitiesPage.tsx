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
  const [editGenreId, setEditGenreId] = useState('');
  const [editPlatformId, setEditPlatformId] = useState('');
  const [editPublisherId, setEditPublisherId] = useState('');

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
      if (editGenreId) {
        await api.updateGenre({ genre: { id: editGenreId, name: genreName } });
        setEditGenreId('');
        setMessage('Genre updated.');
      } else {
        await api.createGenre({ genre: { name: genreName } });
        setMessage('Genre created.');
      }
      setGenreName('');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Create genre failed');
    }
  };

  const createPlatform = async () => {
    try {
      if (editPlatformId) {
        await api.updatePlatform({ platform: { id: editPlatformId, type: platformType } });
        setEditPlatformId('');
        setMessage('Platform updated.');
      } else {
        await api.createPlatform({ platform: { type: platformType } });
        setMessage('Platform created.');
      }
      setPlatformType('');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Create platform failed');
    }
  };

  const createPublisher = async () => {
    try {
      if (editPublisherId) {
        await api.updatePublisher({ publisher: { id: editPublisherId, ...publisher } });
        setEditPublisherId('');
        setMessage('Publisher updated.');
      } else {
        await api.createPublisher({ publisher });
        setMessage('Publisher created.');
      }
      setPublisher({ companyName: '', homePage: '', description: '' });
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Create publisher failed');
    }
  };

  const selectGenre = (item: Genre) => {
    setGenreName(item.name);
    setEditGenreId(item.id);
  };

  const selectPlatform = (item: Platform) => {
    setPlatformType(item.type);
    setEditPlatformId(item.id);
  };

  const selectPublisher = (item: Publisher) => {
    setPublisher({ companyName: item.companyName, homePage: item.homePage ?? '', description: item.description ?? '' });
    setEditPublisherId(item.id);
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
                <div className="row-actions">
                  <button type="button" className="btn-small" onClick={() => selectGenre(item)}>Edit</button>
                  <button type="button" className="btn-small danger" onClick={() => void api.deleteGenre(item.id).then(load)}>
                    Delete
                  </button>
                </div>
              </li>
            ))}
          </ul>
          <div className="form">
            <input value={genreName} onChange={(e: ChangeEvent<HTMLInputElement>) => setGenreName(e.target.value)} placeholder="Genre name" />
            <div className="row-actions">
              <button type="button" className="btn" onClick={() => void createGenre()}>{editGenreId ? 'Update' : 'Add'}</button>
              {editGenreId ? (
                <button type="button" className="btn-small" onClick={() => { setEditGenreId(''); setGenreName(''); }}>Cancel</button>
              ) : null}
            </div>
          </div>
        </article>

        <article className="card">
          <h3>Platforms</h3>
          <ul className="list compact">
            {platforms.map((item: Platform) => (
              <li key={item.id}>
                <span>{item.type}</span>
                <div className="row-actions">
                  <button type="button" className="btn-small" onClick={() => selectPlatform(item)}>Edit</button>
                  <button type="button" className="btn-small danger" onClick={() => void api.deletePlatform(item.id).then(load)}>
                    Delete
                  </button>
                </div>
              </li>
            ))}
          </ul>
          <div className="form">
            <input value={platformType} onChange={(e: ChangeEvent<HTMLInputElement>) => setPlatformType(e.target.value)} placeholder="Platform type" />
            <div className="row-actions">
              <button type="button" className="btn" onClick={() => void createPlatform()}>{editPlatformId ? 'Update' : 'Add'}</button>
              {editPlatformId ? (
                <button type="button" className="btn-small" onClick={() => { setEditPlatformId(''); setPlatformType(''); }}>Cancel</button>
              ) : null}
            </div>
          </div>
        </article>

        <article className="card">
          <h3>Publishers</h3>
          <ul className="list compact">
            {publishers.map((item: Publisher) => (
              <li key={item.id}>
                <span>{item.companyName}</span>
                <div className="row-actions">
                  <button type="button" className="btn-small" onClick={() => selectPublisher(item)}>Edit</button>
                  <button type="button" className="btn-small danger" onClick={() => void api.deletePublisher(item.id).then(load)}>
                    Delete
                  </button>
                </div>
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
            <div className="row-actions">
              <button type="button" className="btn" onClick={() => void createPublisher()}>{editPublisherId ? 'Update' : 'Add'}</button>
              {editPublisherId ? (
                <button type="button" className="btn-small" onClick={() => { setEditPublisherId(''); setPublisher({ companyName: '', homePage: '', description: '' }); }}>Cancel</button>
              ) : null}
            </div>
          </div>
        </article>
      </div>

      {message ? <p>{message}</p> : null}
      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}
