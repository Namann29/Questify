import { api } from './api.js';
import { getState, setState, subscribe } from './state.js';

const THEME_KEY = 'lag_theme';

const THEMES = {
  darkNeon: {
    label: 'Dark Neon',
    vars: {
      '--bg0': '#07080f',
      '--bg1': '#0c0e18',
      '--glass': 'rgba(255,255,255,0.06)',
      '--glass2': 'rgba(255,255,255,0.1)',
      '--stroke': 'rgba(255,255,255,0.12)',
      '--text': '#f4f6ff',
      '--muted': 'rgba(236,242,255,0.62)',
      '--accent': '#7c5cff',
      '--accent2': '#2dd4bf',
      '--sun': '#fbbf24',
      '--rose': '#fb7185',
      '--chart-text': 'rgba(236,242,255,0.78)',
      '--chart-grid': 'rgba(255,255,255,0.06)',
      '--input-bg': 'rgba(0,0,0,0.35)',
      '--card-bg': 'rgba(0,0,0,0.25)',
      '--body-bg': 'radial-gradient(1200px 800px at 10% -10%, rgba(124,92,255,0.35), transparent 55%), radial-gradient(900px 600px at 90% 10%, rgba(45,212,191,0.22), transparent 50%), radial-gradient(700px 500px at 50% 100%, rgba(251,113,133,0.15), transparent 45%), #07080f',
      '--aurora-bg': 'conic-gradient(from 200deg at 50% 120%, rgba(124,92,255,0.15), transparent 40%, rgba(45,212,191,0.12), transparent 55%, rgba(251,191,36,0.1), transparent 70%)',
    }
  },
  minimalLight: {
    label: 'Minimal Light',
    vars: {
      '--bg0': '#f8f9fc',
      '--bg1': '#ffffff',
      '--glass': 'rgba(255,255,255,0.85)',
      '--glass2': 'rgba(255,255,255,0.95)',
      '--stroke': 'rgba(0,0,0,0.08)',
      '--text': '#1a1c2e',
      '--muted': 'rgba(30,34,56,0.55)',
      '--accent': '#6c47ff',
      '--accent2': '#0d9488',
      '--sun': '#d97706',
      '--rose': '#e11d48',
      '--chart-text': 'rgba(30,34,56,0.7)',
      '--chart-grid': 'rgba(0,0,0,0.06)',
      '--input-bg': 'rgba(0,0,0,0.04)',
      '--card-bg': 'rgba(0,0,0,0.03)',
      '--body-bg': 'linear-gradient(135deg, #f0f2ff 0%, #fefefe 50%, #f0fdf4 100%)',
      '--aurora-bg': 'none',
    }
  },
  cyberpunk: {
    label: 'Cyberpunk',
    vars: {
      '--bg0': '#0a0a12',
      '--bg1': '#12121f',
      '--glass': 'rgba(255,0,110,0.06)',
      '--glass2': 'rgba(0,255,200,0.08)',
      '--stroke': 'rgba(0,255,200,0.15)',
      '--text': '#e0ffe8',
      '--muted': 'rgba(0,255,200,0.5)',
      '--accent': '#ff006e',
      '--accent2': '#00ffc8',
      '--sun': '#ffbe0b',
      '--rose': '#ff006e',
      '--chart-text': 'rgba(0,255,200,0.7)',
      '--chart-grid': 'rgba(0,255,200,0.08)',
      '--input-bg': 'rgba(0,255,200,0.05)',
      '--card-bg': 'rgba(255,0,110,0.05)',
      '--body-bg': 'radial-gradient(1200px 800px at 20% -10%, rgba(255,0,110,0.25), transparent 50%), radial-gradient(900px 600px at 80% 10%, rgba(0,255,200,0.18), transparent 45%), #0a0a12',
      '--aurora-bg': 'conic-gradient(from 180deg at 50% 120%, rgba(255,0,110,0.2), transparent 35%, rgba(0,255,200,0.15), transparent 55%, rgba(255,190,11,0.1), transparent 70%)',
    }
  }
};

export function getThemes() {
  return Object.entries(THEMES).map(([key, t]) => ({ key, label: t.label }));
}

export function applyTheme(themeKey) {
  const theme = THEMES[themeKey] || THEMES.darkNeon;
  const root = document.documentElement;
  Object.entries(theme.vars).forEach(([k, v]) => root.style.setProperty(k, v));

  // Handle color-scheme for light theme
  root.style.setProperty('color-scheme', themeKey === 'minimalLight' ? 'light' : 'dark');

  // Body background
  document.body.style.background = theme.vars['--body-bg'];

  // Aurora
  const aurora = document.querySelector('.aurora');
  if (aurora) aurora.style.background = theme.vars['--aurora-bg'];

  localStorage.setItem(THEME_KEY, themeKey);
  setState({ theme: themeKey });
}

export function initTheme() {
  const saved = localStorage.getItem(THEME_KEY) || getState().theme || 'darkNeon';
  applyTheme(saved);
}

export async function switchTheme(themeKey) {
  applyTheme(themeKey);
  try {
    await api('/api/user/theme', {
      method: 'POST',
      body: JSON.stringify({ theme: themeKey })
    });
  } catch (e) {
    console.warn('Theme sync failed:', e);
  }
}
