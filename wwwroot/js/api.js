const TOKEN_KEY = 'lag_token';

/** @param {string} path */
export function apiUrl(path) {
  return path.startsWith('http') ? path : path;
}

export function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token) {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
}

/**
 * @param {string} path
 * @param {RequestInit} [opts]
 */
export async function api(path, opts = {}) {
  const headers = { ...opts.headers };
  if (!(opts.body instanceof FormData)) {
    headers['Content-Type'] = 'application/json';
  }
  const t = getToken();
  if (t) headers.Authorization = `Bearer ${t}`;

  const res = await fetch(apiUrl(path), { ...opts, headers });
  if (res.status === 401) {
    clearToken();
    throw new Error('UNAUTHORIZED');
  }
  if (!res.ok) {
    let msg = res.statusText;
    try {
      const text = await res.text();
      if (text) msg = text;
    } catch { /* ignore */ }
    throw new Error(msg);
  }
  if (res.status === 204) return null;
  const ct = res.headers.get('content-type');
  if (ct && ct.includes('application/json')) return res.json();
  return res.text();
}
