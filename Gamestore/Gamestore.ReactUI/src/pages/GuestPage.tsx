import { Link } from 'react-router-dom';

export function GuestPage() {
  return (
    <section className="section">
      <h2>Guest area</h2>
      <p className="muted">Guests can browse games and featured discounts.</p>
      <div className="toolbar">
        <Link to="/games" className="btn-link">Browse games</Link>
        <Link to="/login" className="btn-link">Sign in for more features</Link>
      </div>
    </section>
  );
}
