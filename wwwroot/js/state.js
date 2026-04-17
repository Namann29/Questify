/**
 * Simple reactive state manager for the SPA.
 * Modules subscribe to key paths and get notified on changes.
 */

const listeners = new Map();
const state = {
  user: null,
  skills: [],
  tasks: [],
  activity: [],
  difficulty: null,
  achievements: [],
  questChains: [],
  intelligence: null,
  theme: 'darkNeon',
  coins: 0,
  gems: 0,
  onboardingDone: false,
  currentPage: 'dashboard',
};

export function getState() {
  return state;
}

export function setState(updates) {
  Object.assign(state, updates);
  notify(Object.keys(updates));
}

export function subscribe(keys, fn) {
  const keyList = Array.isArray(keys) ? keys : [keys];
  keyList.forEach(k => {
    if (!listeners.has(k)) listeners.set(k, new Set());
    listeners.get(k).add(fn);
  });
}

function notify(keys) {
  const seen = new Set();
  keys.forEach(k => {
    const fns = listeners.get(k);
    if (fns) fns.forEach(fn => { if (!seen.has(fn)) { seen.add(fn); fn(state); } });
  });
}
