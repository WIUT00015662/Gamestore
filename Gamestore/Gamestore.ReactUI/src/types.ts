export enum OrderStatus {
  Open = 0,
  Checkout = 1,
  Paid = 2,
  Cancelled = 3,
  Shipped = 4,
}

export enum CommentActionType {
  None = 0,
  Reply = 1,
  Quote = 2,
}

export type LoginRequest = {
  model: {
    login: string;
    password: string;
    internalAuth: boolean;
  };
};

export type TokenResponse = { token: string };

export type Game = {
  id: string;
  name: string;
  key: string;
  description?: string;
  price: number;
  unitInStock: number;
  discount: number;
};

export type GetGamesResponse = {
  games: Game[];
  totalPages: number;
  currentPage: number;
};

export type Deal = {
  gameId: string;
  gameName: string;
  vendor: string;
  purchaseUrl: string;
  originalPrice: number;
  discountedPrice: number;
  discountPercent: number;
};

export type Offer = {
  id: string;
  vendor: string;
  purchaseUrl: string;
  price: number;
  lastPolledPrice?: number;
  lastPolledAt?: string;
};

export type Comment = {
  id: string;
  authorUserId?: string;
  name: string;
  body: string;
  isDeleted?: boolean;
  childComments: Comment[];
};

export type UserLookup = {
  id: string;
  name: string;
};

export type BanDurationType = 'OneHour' | 'OneDay' | 'OneWeek' | 'OneMonth' | 'Permanent';

export type CartItem = {
  productId: string;
  price: number;
  quantity: number;
  discount?: number;
};

export type PaymentMethod = {
  imageUrl: string;
  title: string;
  description: string;
};

export type PaymentMethodsResponse = {
  paymentMethods: PaymentMethod[];
};

export type BasicUser = {
  id: string;
  name: string;
};

export type BasicRole = {
  id: string;
  name: string;
};

export type Genre = {
  id: string;
  name: string;
  parentGenreId?: string;
};

export type Platform = {
  id: string;
  type: string;
};

export type Publisher = {
  id: string;
  companyName: string;
  homePage?: string;
  description?: string;
};

export type Order = {
  id: string;
  customerId: string;
  date?: string;
  status: OrderStatus;
};

export type IBoxPaymentResponse = {
  userId: string;
  orderId: string;
  paymentDate: string;
  sum: number;
};

export type DiscountPollingResult = {
  pollingRunId: string;
  polledAt: string;
  totalDiscountedGames: number;
  featuredGamesCount: number;
};

export type RegisterRequest = {
  userName: string;
  email: string;
  password: string;
};

export type GameFilters = {
  name?: string;
  page?: number;
  pageCount?: string;
  sort?: string;
  datePublishing?: string;
  minPrice?: number;
  maxPrice?: number;
};
