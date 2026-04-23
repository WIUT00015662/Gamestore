import { useEffect, useState } from 'react';
import { api } from '../api/client';

export function ModeratorPage() {
  const [durations, setDurations] = useState<string[]>([]);
  const [query, setQuery] = useState('');
  const [matches, setMatches] = useState<string[]>([]);
  const [user, setUser] = useState('');
  const [duration, setDuration] = useState('permanent');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    const load = async () => {
      try {
        const response = await api.getBanDurations();
        setDurations(response);
        if (response.length > 0) {
          setDuration(response[0]);
        }
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load durations');
      }
    };

    void load();
  }, []);

  useEffect(() => {
    const id = setTimeout(() => {
      const search = async () => {
        try {
          const response = await api.searchUsers(query, 20);
          setMatches(response);
        } catch {
          setMatches([]);
        }
      };

      void search();
    }, 250);

    return () => clearTimeout(id);
  }, [query]);

  const ban = async () => {
    try {
      await api.banUser(user, duration);
      setMessage(`User ${user} banned for ${duration}.`);
      setError('');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ban operation failed');
    }
  };

  return (
    <section className="section">
      <h2>Moderator panel</h2>
      <p className="muted">Manage comment moderation and user bans.</p>

      <div className="form-wrap">
        <div className="form">
          <label>
            Search user name
            <input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Type to search users"
            />
          </label>
          {matches.length > 0 ? (
            <ul className="list compact search-results">
              {matches.map((match) => (
                <li key={match}>
                  <button type="button" className="btn-small" onClick={() => setUser(match)}>{match}</button>
                </li>
              ))}
            </ul>
          ) : null}
          <label>
            Selected user
            <input value={user} onChange={(e) => setUser(e.target.value)} placeholder="User name" />
          </label>
          <label>
            Duration
            <select value={duration} onChange={(e) => setDuration(e.target.value)}>
              {durations.map((item) => (
                <option key={item} value={item}>
                  {item}
                </option>
              ))}
            </select>
          </label>
          <button type="button" className="btn" onClick={() => void ban()}>
            Ban user
          </button>
        </div>
      </div>

      {message ? <p>{message}</p> : null}
      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}
