const tokenKey = 'gamestore_token';
const roleClaimKey = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const nameClaimKey = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';

type TokenPayload = {
  exp?: number;
  permission?: string | string[];
  [roleClaimKey]?: string | string[];
  [nameClaimKey]?: string;
};

export const getToken = () => localStorage.getItem(tokenKey);

export const saveToken = (token: string) => localStorage.setItem(tokenKey, token);

export const clearToken = () => localStorage.removeItem(tokenKey);

function parsePayload(): TokenPayload | null {
  const token = getToken();
  if (!token) {
    return null;
  }

  const payload = token.split('.')[1];
  if (!payload) {
    return null;
  }

  try {
    return JSON.parse(atob(payload)) as TokenPayload;
  } catch {
    return null;
  }
}

function isExpired(payload: TokenPayload | null): boolean {
  const exp = payload?.exp;
  if (!exp) {
    return false;
  }

  const nowInSeconds = Math.floor(Date.now() / 1000);
  return exp <= nowInSeconds;
}

export const hasToken = () => {
  const payload = parsePayload();
  if (!payload) {
    return false;
  }

  if (isExpired(payload)) {
    clearToken();
    return false;
  }

  return true;
};

function toArray(value: string | string[] | undefined): string[] {
  if (!value) {
    return [];
  }

  return Array.isArray(value) ? value : [value];
}

export const getRoles = () => toArray(parsePayload()?.[roleClaimKey]);

export const hasRole = (role: string) => getRoles().includes(role);

export const getPrimaryRole = () => getRoles()[0] ?? 'Guest';

export const getUserName = () => parsePayload()?.[nameClaimKey] ?? 'Guest';

export const hasPermission = (permission: string) => {
  if (!hasToken()) {
    return false;
  }

  const permissions = toArray(parsePayload()?.permission);
  return permissions.includes(permission);
};
