import { useEffect, useState } from 'react';
import { api } from '../api/client';
import type { BanDurationType, UserLookup } from '../types';

const durationLabels: Record<BanDurationType, string> = {
  OneHour: '1 hour',
  OneDay: '1 day',
  OneWeek: '1 week',
  OneMonth: '1 month',
  Permanent: 'permanent',
};

export function ModeratorPage() {
  const [durations, setDurations] = useState<BanDurationType[]>([]);
  const [query, setQuery] = useState('');
  const [matches, setMatches] = useState<UserLookup[]>([]);
  const [selectedUserId, setSelectedUserId] = useState('');
  const [selectedUserName, setSelectedUserName] = useState('');
  const [duration, setDuration] = useState<BanDurationType>('Permanent');
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
      await api.banUser(selectedUserId, duration);
      setMessage(`User ${selectedUserName} banned for ${durationLabels[duration]}.`);
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
            <span>Search user name</span>
            <input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Type to search users"
            />
          </label>
          {matches.length > 0 ? (
            <ul className="list compact search-results">
              {matches.map((match) => (
                <li key={match.id}>
                  <button
                    type="button"
                    className="btn-small"
                    onClick={() => {
                      setSelectedUserId(match.id);
                      setSelectedUserName(match.name);
                    }}>
                    {match.name} ({match.id})
                  </button>
                </li>
              ))}
            </ul>
          ) : null}
          <label>
            <span>Selected user name</span>
            <input
              value={selectedUserName}
              onChange={(e) => setSelectedUserName(e.target.value)}
              placeholder="User name"
            />
          </label>
          <label>
            <span>Selected user id</span>
            <input
              value={selectedUserId}
              onChange={(e) => setSelectedUserId(e.target.value)}
              placeholder="User id"
            />
          </label>
          <label>
            <span>Duration</span>
            <select value={duration} onChange={(e) => setDuration(e.target.value as BanDurationType)}>
              {durations.map((item) => (
                <option key={item} value={item}>
                  {durationLabels[item]}
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
