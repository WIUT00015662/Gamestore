import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../api/client';
import { hasPermission } from '../auth';
import type { Comment, Game, Offer } from '../types';

function renderComments(items: Comment[]): JSX.Element {
  return (
    <ul className="list nested">
      {items.map((comment) => (
        <li key={comment.id}>
          <p>
            <strong>{comment.name}:</strong> {comment.body}
          </p>
          {comment.childComments.length > 0 ? renderComments(comment.childComments) : null}
        </li>
      ))}
    </ul>
  );
}

export function GameDetailsPage() {
  const { key = '' } = useParams();
  const [game, setGame] = useState<Game | null>(null);
  const [offers, setOffers] = useState<Offer[]>([]);
  const [comments, setComments] = useState<Comment[]>([]);
  const [commentBody, setCommentBody] = useState('');
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const load = async () => {
    try {
      const [gameResponse, offerResponse, commentsResponse] = await Promise.all([
        api.getGame(key),
        api.getOffers(key),
        api.getComments(key),
      ]);

      setGame(gameResponse);
      setOffers(offerResponse);
      setComments(commentsResponse);
      setError('');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load details');
    }
  };

  useEffect(() => {
    void load();
  }, [key]);

  const buy = async () => {
    try {
      await api.buyGame(key);
      setMessage('Added to cart');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Cannot buy game');
    }
  };

  const submitComment = async () => {
    try {
      await api.addComment(key, 'Guest', commentBody);
      setCommentBody('');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Cannot add comment');
    }
  };

  const canBuy = hasPermission('BuyGame');
  const canComment = hasPermission('CommentGame');

  if (!game) {
    return <section className="section">Loading...</section>;
  }

  return (
    <section className="section">
      <h2>{game.name}</h2>
      <p className="muted">{game.key}</p>
      <p>{game.description}</p>
      <p>
        <strong>${game.price.toFixed(2)}</strong>
      </p>

      {canBuy ? (
        <button className="btn" type="button" onClick={() => void buy()}>
          Add to cart
        </button>
      ) : (
        <p className="muted">Your role cannot buy games.</p>
      )}

      <h3>Where to buy</h3>
      <div className="card-grid">
        {offers.map((offer) => (
          <article key={offer.id} className="card">
            <h4>{offer.vendor}</h4>
            <p>${offer.price.toFixed(2)}</p>
            {offer.lastPolledPrice ? (
              <p className="muted">Last polled: ${offer.lastPolledPrice.toFixed(2)}</p>
            ) : null}
            <a href={offer.purchaseUrl} target="_blank" rel="noreferrer">
              Open offer
            </a>
          </article>
        ))}
      </div>

      <h3>Comments</h3>
      {renderComments(comments)}

      {canComment ? (
        <div className="form">
          <textarea
            placeholder="Write comment"
            value={commentBody}
            onChange={(e) => setCommentBody(e.target.value)}
          />
          <button className="btn" type="button" onClick={() => void submitComment()}>
            Add comment
          </button>
        </div>
      ) : (
        <p className="muted">Your role cannot add comments.</p>
      )}

      {error ? <p className="error">{error}</p> : null}
      {message ? <p>{message}</p> : null}
    </section>
  );
}
