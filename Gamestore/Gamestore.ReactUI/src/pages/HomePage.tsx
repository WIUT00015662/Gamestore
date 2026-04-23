import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../api/client';
import { hasPermission } from '../auth';
import type { Deal, Game } from '../types';

export function HomePage() {
  const navigate = useNavigate();
  const [deals, setDeals] = useState<Deal[]>([]);
  const [games, setGames] = useState<Game[]>([]);
  const [error, setError] = useState('');
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
        const [featuredDeals, sorting, publishDates, pagination] = await Promise.all([
          api.getFeaturedDeals(),
          api.getSortingOptions(),
          api.getPublishDateOptions(),
          api.getPaginationOptions(),
        ]);
        setDeals(featuredDeals);
        setSortingOptions(sorting);
        setPublishDateOptions(publishDates);
        setPaginationOptions(pagination);
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load home data');
      }

      await loadGames(1, '');
    };

    void load();
  }, []);

  useEffect(() => {
    void loadGames(1, search);
  }, [sort, datePublishing, pageCount]);

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
          {canPoll && (
            <button type="button" className="btn" onClick={handlePoll}>
              Poll deals now
            </button>
          )}
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
