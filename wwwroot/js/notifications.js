/**
 * Toast notification system + floating feedback messages.
 */

let container = null;

function ensureContainer() {
  if (container) return container;
  container = document.createElement('div');
  container.className = 'toast-container';
  document.body.appendChild(container);
  return container;
}

export function toast(message, type = 'info', duration = 3000) {
  const el = document.createElement('div');
  el.className = `toast toast-${type}`;
  el.innerHTML = `<span class="toast-icon">${getIcon(type)}</span><span class="toast-msg">${message}</span>`;
  const c = ensureContainer();
  c.appendChild(el);
  requestAnimationFrame(() => el.classList.add('toast-visible'));
  setTimeout(() => {
    el.classList.remove('toast-visible');
    el.addEventListener('transitionend', () => el.remove());
  }, duration);
}

export function xpPopup(amount) {
  const el = document.createElement('div');
  el.className = 'xp-popup';
  el.textContent = `+${amount} XP`;
  document.body.appendChild(el);
  requestAnimationFrame(() => el.classList.add('xp-popup-visible'));
  setTimeout(() => {
    el.classList.remove('xp-popup-visible');
    el.addEventListener('transitionend', () => el.remove());
  }, 1800);
}

export function coinPopup(amount) {
  const el = document.createElement('div');
  el.className = 'coin-popup';
  el.textContent = `+${amount} coins`;
  document.body.appendChild(el);
  requestAnimationFrame(() => el.classList.add('coin-popup-visible'));
  setTimeout(() => {
    el.classList.remove('coin-popup-visible');
    el.addEventListener('transitionend', () => el.remove());
  }, 1800);
}

export function achievementPopup(title, desc, icon) {
  const el = document.createElement('div');
  el.className = 'achievement-popup';
  el.innerHTML = `
    <div class="achievement-popup-inner">
      <div class="achievement-popup-icon">${iconSvg(icon)}</div>
      <div class="achievement-popup-text">
        <p class="achievement-popup-label">Achievement Unlocked!</p>
        <h4>${title}</h4>
        <p>${desc}</p>
      </div>
    </div>`;
  document.body.appendChild(el);
  requestAnimationFrame(() => el.classList.add('achievement-popup-visible'));
  setTimeout(() => {
    el.classList.remove('achievement-popup-visible');
    el.addEventListener('transitionend', () => el.remove());
  }, 4000);
}

export function levelUpPopup(level) {
  const el = document.createElement('div');
  el.className = 'levelup-overlay';
  el.innerHTML = `
    <div class="levelup-content">
      <div class="levelup-glow"></div>
      <h2>LEVEL UP!</h2>
      <p class="levelup-level">Level ${level}</p>
      <p class="levelup-sub">Your power grows stronger</p>
    </div>`;
  document.body.appendChild(el);
  requestAnimationFrame(() => el.classList.add('levelup-visible'));
  setTimeout(() => {
    el.classList.remove('levelup-visible');
    el.addEventListener('transitionend', () => el.remove());
  }, 3000);
}

function getIcon(type) {
  switch (type) {
    case 'success': return '✓';
    case 'error': return '✕';
    case 'warning': return '⚠';
    default: return 'ℹ';
  }
}

function iconSvg(icon) {
  const icons = {
    trophy: '<svg viewBox="0 0 24 24" width="32" height="32"><path fill="currentColor" d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/></svg>',
    flame: '<svg viewBox="0 0 24 24" width="32" height="32"><path fill="currentColor" d="M12 23c-3.9 0-7-3.1-7-7 0-3.3 2.1-5.8 3.5-7.8.5-.6 1.5-.4 1.7.3.4 1.5 1.2 3.1 2.3 4.3.2-2.5 1.5-4.8 3-6.8.5-.6 1.5-.4 1.7.3C18.3 9.5 19 12 19 15c0 3.9-3.1 7-7 8z"/></svg>',
    star: '<svg viewBox="0 0 24 24" width="32" height="32"><path fill="currentColor" d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z"/></svg>',
    shield: '<svg viewBox="0 0 24 24" width="32" height="32"><path fill="currentColor" d="M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4z"/></svg>',
    crown: '<svg viewBox="0 0 24 24" width="32" height="32"><path fill="currentColor" d="M5 16L3 5l5.5 5L12 4l3.5 6L21 5l-2 11H5z"/></svg>',
    gem: '<svg viewBox="0 0 24 24" width="32" height="32"><path fill="currentColor" d="M6 2l-4 8 10 13L22 10l-4-8H6zm1.5 2h9L19 9.5 12 20 5 9.5 7.5 4z"/></svg>',
    coins: '<svg viewBox="0 0 24 24" width="32" height="32"><path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67V7z"/></svg>',
    zap: '<svg viewBox="0 0 24 24" width="32" height="32"><path fill="currentColor" d="M11 21h-1l1-7H7.5c-.58 0-.57-.32-.38-.66.19-.34.05-.08.05-.08L13 3h1l-1 7h3.5c.49 0 .59.32.38.66L11 21z"/></svg>',
    'book-open': '<svg viewBox="0 0 24 24" width="32" height="32"><path fill="currentColor" d="M13 21l-2-1-2 1V7h4v14zm6-18h-4v14l2-1 2 1V3zM7 3H3v14l2-1 2 1V3z"/></svg>',
    'arrow-up': '<svg viewBox="0 0 24 24" width="32" height="32"><path fill="currentColor" d="M4 12l1.41 1.41L11 7.83V20h2V7.83l5.58 5.59L20 12l-8-8-8 8z"/></svg>',
  };
  return icons[icon] || icons.star;
}
