import { useEffect, useMemo, useState } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../api/client';
import { getUserName, hasPermission } from '../auth';
import type { Comment, Game, Offer } from '../types';

type CommentTreeProps = {
  gameKey: string;
  items: Comment[];
  canComment: boolean;
  canModerate: boolean;
  currentUser: string;
  onChanged: () => Promise<void>;
};

function CommentTree({ gameKey, items, canComment, canModerate, currentUser, onChanged }: CommentTreeProps): JSX.Element {
  const [replyToId, setReplyToId] = useState('');
  const [quoteToId, setQuoteToId] = useState('');
  const [editId, setEditId] = useState('');
  const [text, setText] = useState('');

  const resetAction = () => {
    setReplyToId('');
    setQuoteToId('');
    setEditId('');
    setText('');
  };

  const submit = async () => {
    if (!text.trim()) {
      return;
    }

    if (editId) {
      await api.updateComment(gameKey, editId, text.trim());
    } else if (quoteToId) {
      await api.addComment(gameKey, text.trim(), quoteToId, 'quote');
    } else if (replyToId) {
      await api.addComment(gameKey, text.trim(), replyToId, 'reply');
    }

    resetAction();
    await onChanged();
  };

  const remove = async (id: string) => {
    await api.deleteComment(gameKey, id);
    await onChanged();
  };

  return (
    <ul className="list nested">
      {items.map((comment) => {
        const isOwner = comment.name.toLowerCase() === currentUser.toLowerCase();
        const canDelete = canModerate || isOwner;
        const canEdit = isOwner && !comment.isDeleted;

        return (
          <li key={comment.id}>
            <p>
              <strong>{comment.name}:</strong> {comment.body}
            </p>
            <div className="row-actions">
              {canComment && !comment.isDeleted ? (
                <>
                  <button type="button" className="btn-small" onClick={() => { setReplyToId(comment.id); setQuoteToId(''); setEditId(''); setText(''); }}>
                    Reply
                  </button>
                  <button type="button" className="btn-small" onClick={() => { setQuoteToId(comment.id); setReplyToId(''); setEditId(''); setText(''); }}>
                    Quote
                  </button>
                </>
              ) : null}
              {canEdit ? (
                <button type="button" className="btn-small" onClick={() => { setEditId(comment.id); setReplyToId(''); setQuoteToId(''); setText(comment.body); }}>
                  Edit
                </button>
              ) : null}
              {canDelete ? (
                <button type="button" className="btn-small danger" onClick={() => void remove(comment.id)}>
                  Delete
                </button>
              ) : null}
            </div>

            {(replyToId === comment.id || quoteToId === comment.id || editId === comment.id) ? (
              <div className="form comment-action-form">
                <textarea
                  placeholder={editId === comment.id ? 'Edit your comment' : 'Write your comment'}
                  value={text}
                  onChange={(e) => setText(e.target.value)}
                />
                <div className="row-actions">
                  <button className="btn-small" type="button" onClick={() => void submit()}>Save</button>
                  <button className="btn-small" type="button" onClick={resetAction}>Cancel</button>
                </div>
              </div>
            ) : null}

            {comment.childComments.length > 0 ? (
              <CommentTree
                gameKey={gameKey}
                items={comment.childComments}
                canComment={canComment}
                canModerate={canModerate}
                currentUser={currentUser}
                onChanged={onChanged}
              />
            ) : null}
          </li>
        );
      })}
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
    if (!commentBody.trim()) {
      return;
    }

    try {
      await api.addComment(key, commentBody.trim());
      setCommentBody('');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Cannot add comment');
    }
  };

  const canBuy = hasPermission('BuyGame');
  const canComment = hasPermission('CommentGame');
  const canModerate = hasPermission('ManageComments');
  const currentUser = useMemo(() => getUserName(), []);
  const bestPrice = offers.length > 0
    ? Math.min(...offers.map((offer) => offer.price))
    : game?.bestOfferPrice;

  if (!game) {
    return <section className="section">Loading...</section>;
  }

  return (
    <section className="section">
      <h2>{game.name}</h2>
      <p className="muted">{game.key}</p>
      <p>{game.description}</p>
      {typeof bestPrice === 'number' ? (
        <p>
          <strong>${bestPrice.toFixed(2)}</strong>
        </p>
      ) : null}

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
      <CommentTree
        gameKey={key}
        items={comments}
        canComment={canComment}
        canModerate={canModerate}
        currentUser={currentUser}
        onChanged={load}
      />

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
