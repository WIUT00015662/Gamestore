import { getToken } from '../auth';
import type {
  BasicRole,
  BasicUser,
  CartItem,
  Comment,
  Deal,
  DiscountPollingResult,
  Game,
  Genre,
  GetGamesResponse,
  IBoxPaymentResponse,
  LoginRequest,
  Offer,
  Order,
  PaymentMethodsResponse,
  Platform,
  Publisher,
  TokenResponse,
} from '../types';

const apiBase = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7135';

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const token = getToken();
  const response = await fetch(`${apiBase}${url}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(init?.headers ?? {}),
    },
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `Request failed: ${response.status}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export const api = {
  login: (body: LoginRequest) => request<TokenResponse>('/users/login', { method: 'POST', body: JSON.stringify(body) }),
  getGames: (name = '', page = 1) => request<GetGamesResponse>(`/games?name=${encodeURIComponent(name)}&page=${page}&pageCount=10`),
  getAllGamesRaw: () => request<Game[]>('/games/all'),
  getGame: (key: string) => request<Game>(`/games/${key}`),
  createGame: (payload: { game: { name: string; key: string; description: string; price: number; unitInStock: number; discount: number }; genres: string[]; platforms: string[]; publisher: string }) =>
    request<Game>('/games', { method: 'POST', body: JSON.stringify(payload) }),
  updateGame: (payload: { game: { id: string; name: string; key: string; description: string; price: number; unitInStock: number; discount: number }; genres: string[]; platforms: string[]; publisher: string }) =>
    request<void>('/games', { method: 'PUT', body: JSON.stringify(payload) }),
  deleteGame: (key: string) => request<void>(`/games/${key}`, { method: 'DELETE' }),
  buyGame: (key: string) => request<void>(`/games/${key}/buy`, { method: 'POST' }),
  getComments: (key: string) => request<Comment[]>(`/games/${key}/comments`),
  addComment: (key: string, name: string, body: string) =>
    request<void>(`/games/${key}/comments`, {
      method: 'POST',
      body: JSON.stringify({ comment: { name, body } }),
    }),
  getOffers: (key: string) => request<Offer[]>(`/deals/games/${key}/offers`),
  getFeaturedDeals: () => request<Deal[]>('/deals/featured?take=5'),
  triggerPolling: () => request<DiscountPollingResult>('/deals/poll', { method: 'POST' }),
  getCart: () => request<CartItem[]>('/orders/cart'),
  removeFromCart: (key: string) => request<void>(`/orders/cart/${key}`, { method: 'DELETE' }),
  getPaymentMethods: () => request<PaymentMethodsResponse>('/orders/payment-methods'),
  pay: async (method: string, model?: unknown) => {
    if (method === 'IBox terminal') {
      return request<IBoxPaymentResponse>('/orders/payment', { method: 'POST', body: JSON.stringify({ method, model }) });
    }

    if (method === 'Bank') {
      const token = getToken();
      const response = await fetch(`${apiBase}/orders/payment`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
        },
        body: JSON.stringify({ method, model }),
      });

      if (!response.ok) {
        throw new Error(await response.text());
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'invoice.pdf';
      link.click();
      window.URL.revokeObjectURL(url);
      return;
    }

    return request<void>('/orders/payment', { method: 'POST', body: JSON.stringify({ method, model }) });
  },
  getUsers: () => request<BasicUser[]>('/users'),
  getUserRoles: (id: string) => request<BasicRole[]>(`/users/${id}/roles`),
  createUser: (payload: { user: { id: string; name: string }; roles: string[]; password: string }) =>
    request<BasicUser>('/users', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),
  updateUser: (payload: { user: { id: string; name: string }; roles: string[]; password: string }) =>
    request<void>('/users', {
      method: 'PUT',
      body: JSON.stringify(payload),
    }),
  deleteUser: (id: string) => request<void>(`/users/${id}`, { method: 'DELETE' }),

  getRoles: () => request<BasicRole[]>('/roles'),
  getPermissions: () => request<string[]>('/roles/permissions'),
  getRolePermissions: (id: string) => request<string[]>(`/roles/${id}/permissions`),
  createRole: (payload: { role: { id: string; name: string }; permissions: string[] }) =>
    request<BasicRole>('/roles', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),
  updateRole: (payload: { role: { id: string; name: string }; permissions: string[] }) =>
    request<void>('/roles', {
      method: 'PUT',
      body: JSON.stringify(payload),
    }),
  deleteRole: (id: string) => request<void>(`/roles/${id}`, { method: 'DELETE' }),

  getBanDurations: () => request<string[]>('/comments/ban/durations'),
  banUser: (user: string, duration: string) =>
    request<void>('/comments/ban', {
      method: 'POST',
      body: JSON.stringify({ user, duration }),
    }),
  getGamesByGenreId: (id: string) => request<Game[]>(`/genres/${id}/games`),
  getGamesByPlatformId: (id: string) => request<Game[]>(`/platforms/${id}/games`),
  getGamesByPublisherName: (name: string) => request<Game[]>(`/publishers/${encodeURIComponent(name)}/games`),

  getGenres: () => request<Genre[]>('/genres'),
  createGenre: (payload: { genre: { name: string; parentGenreId?: string } }) =>
    request<Genre>('/genres', { method: 'POST', body: JSON.stringify(payload) }),
  updateGenre: (payload: { genre: { id: string; name: string; parentGenreId?: string } }) =>
    request<void>('/genres', { method: 'PUT', body: JSON.stringify(payload) }),
  deleteGenre: (id: string) => request<void>(`/genres/${id}`, { method: 'DELETE' }),

  getPlatforms: () => request<Platform[]>('/platforms'),
  createPlatform: (payload: { platform: { type: string } }) =>
    request<Platform>('/platforms', { method: 'POST', body: JSON.stringify(payload) }),
  updatePlatform: (payload: { platform: { id: string; type: string } }) =>
    request<void>('/platforms', { method: 'PUT', body: JSON.stringify(payload) }),
  deletePlatform: (id: string) => request<void>(`/platforms/${id}`, { method: 'DELETE' }),

  getPublishers: () => request<Publisher[]>('/publishers'),
  createPublisher: (payload: { publisher: { companyName: string; homePage: string; description: string } }) =>
    request<Publisher>('/publishers', { method: 'POST', body: JSON.stringify(payload) }),
  updatePublisher: (payload: { publisher: { id: string; companyName: string; homePage: string; description: string } }) =>
    request<void>('/publishers', { method: 'PUT', body: JSON.stringify(payload) }),
  deletePublisher: (id: string) => request<void>(`/publishers/${id}`, { method: 'DELETE' }),

  getOrders: () => request<Order[]>('/orders'),
  getOrderById: (id: string) => request<Order>(`/orders/${id}`),
  getOrderDetails: (id: string) => request<CartItem[]>(`/orders/${id}/details`),
  addGameToOrder: (orderId: string, key: string) => request<void>(`/orders/${orderId}/details/${key}`, { method: 'POST' }),
  updateOrderDetailQuantity: (id: string, count: number) =>
    request<void>(`/orders/details/${id}/quantity`, { method: 'PATCH', body: JSON.stringify({ count }) }),
  deleteOrderDetail: (id: string) => request<void>(`/orders/details/${id}`, { method: 'DELETE' }),
  shipOrder: (id: string) => request<void>(`/orders/${id}/ship`, { method: 'POST' }),
};
