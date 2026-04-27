import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../api/client';
import { hasPermission } from '../auth';
import type { Deal, Game, Genre } from '../types';

export function HomePage() {
  const navigate = useNavigate();
  const [deals, setDeals] = useState<Deal[]>([]);
  const [games, setGames] = useState<Game[]>([]);
  const [error, setError] = useState('');
  const [subscriptionEmail, setSubscriptionEmail] = useState('');
  const [subscriptionMessage, setSubscriptionMessage] = useState('');
  const [subscriptionError, setSubscriptionError] = useState('');
  const [unsubscribeEmail, setUnsubscribeEmail] = useState('');
  const [unsubscribeMessage, setUnsubscribeMessage] = useState('');
  const [unsubscribeError, setUnsubscribeError] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const [searchInput, setSearchInput] = useState('');
  const [search, setSearch] = useState('');
  const [sort, setSort] = useState('');
  const [datePublishing, setDatePublishing] = useState('');
  const [pageCount, setPageCount] = useState('10');
  const [minPrice, setMinPrice] = useState('');
  const [maxPrice, setMaxPrice] = useState('');

  const [sortingOptions, setSortingOptions] = useState<string[]>([]);
  const [publishDateOptions, setPublishDateOptions] = useState<string[]>([]);
  const [paginationOptions, setPaginationOptions] = useState<string[]>([]);
  const [genres, setGenres] = useState<Genre[]>([]);
  const [selectedGenre, setSelectedGenre] = useState('');

  const loadGames = async (nextPage: number, nextSearch = search) => {
    try {
      const response = await api.getGamesFiltered({
        name: nextSearch,
        page: nextPage,
        pageCount,
        sort: sort || undefined,
        datePublishing: datePublishing || undefined,
        minPrice: minPrice ? Number(minPrice) : undefined,
        maxPrice: maxPrice ? Number(maxPrice) : undefined,
      });
      setGames(response.games);
      setTotalPages(response.totalPages || 1);
      setPage(response.currentPage || nextPage);
      setError('');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load games');
    }
  };

  useEffect(() => {
    const load = async () => {
      try {
        const [featuredDeals, sorting, publishDates, pagination, genresResponse] = await Promise.all([
          api.getFeaturedDeals(),
          api.getSortingOptions(),
          api.getPublishDateOptions(),
          api.getPaginationOptions(),
          api.getGenres(),
        ]);
        setDeals(featuredDeals);
        setSortingOptions(sorting);
        setPublishDateOptions(publishDates);
        setPaginationOptions(pagination);
        setGenres(genresResponse);
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load home data');
      }

      await loadGames(1, '');
    };

    void load();
  }, []);

  useEffect(() => {
    if (selectedGenre) {
      void (async () => {
        try {
          const response = await api.getGamesByGenreId(selectedGenre);
          setGames(response);
          setTotalPages(1);
          setPage(1);
          setError('');
        } catch (e) {
          setError(e instanceof Error ? e.message : 'Failed to load games');
        }
      })();
      return;
    }

    void loadGames(1, search);
  }, [sort, datePublishing, pageCount, selectedGenre]);

  const canPoll = hasPermission('ManageEntities');

  const handlePoll = async () => {
    try {
      await api.triggerPolling();
      const featuredDeals = await api.getFeaturedDeals();
      setDeals(featuredDeals);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Polling failed');
    }
  };

  const handleSubscribe = async (e: FormEvent) => {
    e.preventDefault();
    const email = subscriptionEmail.trim();
    if (!email) {
      setSubscriptionError('Please enter a valid email address.');
      return;
    }

    try {
      await api.subscribeToDiscounts(email);
      setSubscriptionMessage('Thanks! You are subscribed for discount alerts.');
      setSubscriptionError('');
      setSubscriptionEmail('');
      setUnsubscribeMessage('');
      setUnsubscribeError('');
    } catch (e) {
      setSubscriptionError(e instanceof Error ? e.message : 'Subscription failed');
      setSubscriptionMessage('');
    }
  };

  const handleUnsubscribe = async (e: FormEvent) => {
    e.preventDefault();
    const email = unsubscribeEmail.trim();
    if (!email) {
      setUnsubscribeError('Please enter a valid email address.');
      return;
    }

    try {
      await api.unsubscribeFromDiscounts(email);
      setUnsubscribeMessage('You have been unsubscribed.');
      setUnsubscribeError('');
      setUnsubscribeEmail('');
      setSubscriptionMessage('');
      setSubscriptionError('');
    } catch (e) {
      setUnsubscribeError(e instanceof Error ? e.message : 'Unsubscribe failed');
      setUnsubscribeMessage('');
    }
  };

  const runSearch = async (e: FormEvent) => {
    e.preventDefault();
    const trimmed = searchInput.trim();
    setSearch(trimmed);
    await loadGames(1, trimmed);
  };

  return (
    <div>
      <section className="section">
        <div className="section-header">
          <h2>Top discounted deals</h2>
          <div className="toolbar">
            <button type="button" className="btn" onClick={() => navigate('/deals')}>
              See all discounts
            </button>
            {canPoll && (
              <button type="button" className="btn" onClick={handlePoll}>
                Poll deals now
              </button>
            )}
          </div>
        </div>
        {deals.length === 0 ? <p className="muted">No featured deals yet.</p> : null}
        <div className="card-grid">
          {deals.map((deal) => (
            <article className="card card-clickable" key={`${deal.gameId}-${deal.vendor}`} onClick={() => window.open(deal.purchaseUrl, '_blank', 'noreferrer')}>
              <p className="muted">Game</p>
              <h3>{deal.gameName}</h3>
              <p className="muted">Publisher / Vendor</p>
              <p>{deal.vendor}</p>
              <p className="muted">Discount</p>
              <p>
                <strong>{deal.discountPercent}% off</strong>
              </p>
              <p className="muted">Price</p>
              <p>
                ${deal.discountedPrice.toFixed(2)} <span className="muted">(was ${deal.originalPrice.toFixed(2)})</span>
              </p>
            </article>
          ))}
        </div>
      </section>

      <section className="section">
        <h2>Get discount emails</h2>
        <p className="muted">Subscribe as a guest or signed-in user to receive new discounts every interval.</p>
        <form className="toolbar" onSubmit={(e) => void handleSubscribe(e)}>
          <input
            type="email"
            placeholder="you@example.com"
            value={subscriptionEmail}
            onChange={(e) => setSubscriptionEmail(e.target.value)}
          />
          <button className="btn" type="submit">Subscribe</button>
        </form>
        {subscriptionMessage ? <p>{subscriptionMessage}</p> : null}
        {subscriptionError ? <p className="error">{subscriptionError}</p> : null}
        <div className="spacer" />
        <form className="toolbar" onSubmit={(e) => void handleUnsubscribe(e)}>
          <input
            type="email"
            placeholder="you@example.com"
            value={unsubscribeEmail}
            onChange={(e) => setUnsubscribeEmail(e.target.value)}
          />
          <button className="btn" type="submit">Unsubscribe</button>
        </form>
        {unsubscribeMessage ? <p>{unsubscribeMessage}</p> : null}
        {unsubscribeError ? <p className="error">{unsubscribeError}</p> : null}
      </section>

      <section className="section">
        <h2>Games catalog</h2>
        <form className="toolbar" onSubmit={(e) => void runSearch(e)}>
          <input
            placeholder="Search games"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />
          <select value={sort} onChange={(e) => setSort(e.target.value)}>
            <option value="">Sort by</option>
            {sortingOptions.map((option) => (
              <option key={option} value={option}>{option}</option>
            ))}
          </select>
          <select value={datePublishing} onChange={(e) => setDatePublishing(e.target.value)}>
            <option value="">Publish date</option>
            {publishDateOptions.map((option) => (
              <option key={option} value={option}>{option}</option>
            ))}
          </select>
          <select value={pageCount} onChange={(e) => setPageCount(e.target.value)}>
            {paginationOptions.map((option) => (
              <option key={option} value={option}>{option} / page</option>
            ))}
          </select>
          <select value={selectedGenre} onChange={(e) => setSelectedGenre(e.target.value)}>
            <option value="">All genres</option>
            {genres.map((genre) => (
              <option key={genre.id} value={genre.id}>{genre.name}</option>
            ))}
          </select>
          <input type="number" min="0" step="0.01" placeholder="Min price" value={minPrice} onChange={(e) => setMinPrice(e.target.value)} />
          <input type="number" min="0" step="0.01" placeholder="Max price" value={maxPrice} onChange={(e) => setMaxPrice(e.target.value)} />
          <button className="btn" type="submit">Search</button>
          <button
            className="btn"
            type="button"
            onClick={() => {
              setSearchInput('');
              setSearch('');
              setSort('');
              setDatePublishing('');
              setMinPrice('');
              setMaxPrice('');
              setSelectedGenre('');
              setPage(1);
              void loadGames(1, '');
            }}
          >
            Reset
          </button>
        </form>

        {games.length === 0 ? <p className="muted">No games found for current criteria.</p> : null}

        <div className="card-grid">
          {games.map((game) => (
            <article className="card card-clickable" key={game.id} onClick={() => navigate(`/games/${game.key}`)}>
              <p className="muted">Game</p>
              <h3>{game.name}</h3>
              <p className="muted">Key</p>
              <p>{game.key}</p>
              <p className="muted">Price</p>
              <p>${game.price.toFixed(2)}</p>
              <p className="muted">Stock</p>
              <p>{game.unitInStock}</p>
            </article>
          ))}
        </div>

        <div className="toolbar">
          <button className="btn" type="button" disabled={page <= 1} onClick={() => void loadGames(page - 1)}>
            Previous
          </button>
          <span className="muted">Page {page} of {Math.max(totalPages, 1)}</span>
          <button className="btn" type="button" disabled={page >= totalPages} onClick={() => void loadGames(page + 1)}>
            Next
          </button>
        </div>
      </section>

      {error ? <p className="error">{error}</p> : null}
    </div>
  );
}
