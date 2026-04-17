import { getState, subscribe } from './state.js';

const ALL_ACHIEVEMENTS = [
  { key: 'first_task', title: 'First Task Completed', icon: 'trophy' },
  { key: 'seven_day_streak', title: '7-Day Streak', icon: 'flame' },
  { key: 'xp_master', title: 'XP Master', icon: 'star' },
  { key: 'hard_quest', title: 'Hard Mode Hero', icon: 'shield' },
  { key: 'all_skills', title: 'Well Rounded', icon: 'crown' },
  { key: 'level_5', title: 'Rising Star', icon: 'arrow-up' },
  { key: 'level_10', title: 'Veteran Player', icon: 'gem' },
  { key: 'coin_collector', title: 'Coin Collector', icon: 'coins' },
  { key: 'quest_chain_complete', title: 'Story Mode Champion', icon: 'book-open' },
  { key: 'ten_tasks_day', title: 'Productivity Beast', icon: 'zap' },
];

export function renderAchievements() {
  const el = document.getElementById('achievements-grid');
  if (!el) return;

  const unlocked = getState().achievements || [];
  const unlockedKeys = new Set(unlocked.map(a => a.key));

  el.innerHTML = ALL_ACHIEVEMENTS.map(a => {
    const isUnlocked = unlockedKeys.has(a.key);
    const unlockedData = unlocked.find(u => u.key === a.key);
    return `
      <div class="achievement-card ${isUnlocked ? 'unlocked' : 'locked'}" data-key="${a.key}">
        <div class="achievement-icon ${isUnlocked ? '' : 'locked-icon'}">${iconSvg(a.icon)}</div>
        <div class="achievement-info">
          <h4>${a.title}</h4>
          <p class="muted small">${isUnlocked ? 'Unlocked!' : 'Locked'}</p>
          ${unlockedData ? `<time class="achievement-time">${new Date(unlockedData.unlockedAtUtc).toLocaleDateString()}</time>` : ''}
        </div>
        ${isUnlocked ? '<div class="achievement-check">&#10003;</div>' : '<div class="achievement-lock">&#128274;</div>'}
      </div>`;
  }).join('');
}

function iconSvg(icon) {
  const icons = {
    trophy: '<svg viewBox="0 0 24 24" width="24" height="24"><path fill="currentColor" d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/></svg>',
    flame: '<svg viewBox="0 0 24 24" width="24" height="24"><path fill="currentColor" d="M12 23c-3.9 0-7-3.1-7-7 0-3.3 2.1-5.8 3.5-7.8.5-.6 1.5-.4 1.7.3.4 1.5 1.2 3.1 2.3 4.3.2-2.5 1.5-4.8 3-6.8.5-.6 1.5-.4 1.7.3C18.3 9.5 19 12 19 15c0 3.9-3.1 7-7 8z"/></svg>',
    star: '<svg viewBox="0 0 24 24" width="24" height="24"><path fill="currentColor" d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z"/></svg>',
    shield: '<svg viewBox="0 0 24 24" width="24" height="24"><path fill="currentColor" d="M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4z"/></svg>',
    crown: '<svg viewBox="0 0 24 24" width="24" height="24"><path fill="currentColor" d="M5 16L3 5l5.5 5L12 4l3.5 6L21 5l-2 11H5z"/></svg>',
    gem: '<svg viewBox="0 0 24 24" width="24" height="24"><path fill="currentColor" d="M6 2l-4 8 10 13L22 10l-4-8H6zm1.5 2h9L19 9.5 12 20 5 9.5 7.5 4z"/></svg>',
    coins: '<svg viewBox="0 0 24 24" width="24" height="24"><path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8z"/></svg>',
    zap: '<svg viewBox="0 0 24 24" width="24" height="24"><path fill="currentColor" d="M11 21h-1l1-7H7.5c-.58 0-.57-.32-.38-.66.19-.34.05-.08.05-.08L13 3h1l-1 7h3.5c.49 0 .59.32.38.66L11 21z"/></svg>',
    'book-open': '<svg viewBox="0 0 24 24" width="24" height="24"><path fill="currentColor" d="M13 21l-2-1-2 1V7h4v14zm6-18h-4v14l2-1 2 1V3zM7 3H3v14l2-1 2 1V3z"/></svg>',
    'arrow-up': '<svg viewBox="0 0 24 24" width="24" height="24"><path fill="currentColor" d="M4 12l1.41 1.41L11 7.83V20h2V7.83l5.58 5.59L20 12l-8-8-8 8z"/></svg>',
  };
  return icons[icon] || icons.star;
}

subscribe('achievements', renderAchievements);
