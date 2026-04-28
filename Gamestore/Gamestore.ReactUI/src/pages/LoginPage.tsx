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
  const [registerName, setRegisterName] = useState('');
  const [registerEmail, setRegisterEmail] = useState('');
  const [registerPassword, setRegisterPassword] = useState('');
  const [registerError, setRegisterError] = useState('');

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

  const handleRegister = async (e: FormEvent) => {
    e.preventDefault();

    try {
      const response = await api.register({
        userName: registerName,
        email: registerEmail,
        password: registerPassword,
      });

      saveToken(response.token);
      onLogin();
    } catch (err) {
      setRegisterError(err instanceof Error ? err.message : 'Registration failed');
    }
  };

  return (
    <section className="section">
      <div className="auth-grid">
        <div className="auth-card">
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
        </div>

        <div className="auth-card">
          <h2>Create account</h2>
          <form onSubmit={handleRegister} className="form">
            <label>
              User name
              <input value={registerName} onChange={(e) => setRegisterName(e.target.value)} />
            </label>
            <label>
              Email
              <input type="email" value={registerEmail} onChange={(e) => setRegisterEmail(e.target.value)} />
            </label>
            <label>
              Password
              <input type="password" value={registerPassword} onChange={(e) => setRegisterPassword(e.target.value)} />
            </label>
            <button type="submit" className="btn">
              Register
            </button>
          </form>
          {registerError ? <p className="error">{registerError}</p> : null}
        </div>
      </div>
    </section>
  );
}
