import { useState, type FormEvent } from 'react';
import { api } from '../api/client';
import { saveToken } from '../auth';

type LoginPageProps = {
  onLogin: () => void;
};

export function LoginPage({ onLogin }: LoginPageProps) {
  const [login, setLogin] = useState('admin');
  const [password, setPassword] = useState('admin');
  const [error, setError] = useState('');

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();

    try {
      const response = await api.login({
        model: {
          login,
          password,
          internalAuth: true,
        },
      });

      saveToken(response.token);
      onLogin();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed');
    }
  };

  return (
    <section className="section form-wrap">
      <h2>Sign in</h2>
      <form onSubmit={handleSubmit} className="form">
        <label>
          Login
          <input value={login} onChange={(e) => setLogin(e.target.value)} />
        </label>
        <label>
          Password
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </label>
        <button type="submit" className="btn">
          Login
        </button>
      </form>
      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}
