import { api } from './api.js';
import { getState, setState, subscribe } from './state.js';
import { toast, coinPopup } from './notifications.js';

export function renderCurrency() {
  const coinsEl = document.getElementById('coins-val');
  const gemsEl = document.getElementById('gems-val');
  if (coinsEl) coinsEl.textContent = getState().coins || 0;
  if (gemsEl) gemsEl.textContent = getState().gems || 0;
}

export function renderShop() {
  const el = document.getElementById('shop-container');
  if (!el) return;

  const coins = getState().coins || 0;
  const gems = getState().gems || 0;

  const items = [
    { key: 'theme_minimal', name: 'Minimal Light Theme', desc: 'A clean, minimal light theme', cost: 50, currency: 'coins', icon: '🎨' },
    { key: 'theme_cyberpunk', name: 'Cyberpunk Theme', desc: 'Neon-soaked cyberpunk aesthetic', cost: 50, currency: 'coins', icon: '🌆' },
    { key: 'xp_boost', name: '2x XP Boost', desc: 'Double XP on your next task', cost: 30, currency: 'coins', icon: '⚡' },
    { key: 'streak_freeze', name: 'Streak Freeze', desc: 'Protect streak for one missed day', cost: 10, currency: 'gems', icon: '🧊' },
    { key: 'gem_pack', name: 'Gem Pack (5)', desc: '5 premium gems', cost: 25, currency: 'coins', icon: '💎' },
  ];

  el.innerHTML = `
    <div class="shop-balance">
      <div class="shop-balance-item"><span class="coin-icon">🪙</span> <strong id="shop-coins">${coins}</strong> coins</div>
      <div class="shop-balance-item"><span class="gem-icon">💎</span> <strong id="shop-gems">${gems}</strong> gems</div>
    </div>
    <div class="shop-grid">
      ${items.map(item => {
        const canAfford = item.currency === 'coins' ? coins >= item.cost : gems >= item.cost;
        return `
          <div class="shop-item glass-inner ${canAfford ? '' : 'shop-item-locked'}">
            <div class="shop-item-icon">${item.icon}</div>
            <div class="shop-item-info">
              <h4>${item.name}</h4>
              <p class="muted small">${item.desc}</p>
              <span class="shop-price ${item.currency}">${item.cost} ${item.currency}</span>
            </div>
            <button class="btn btn-small btn-complete shop-buy-btn" 
              data-item="${item.key}" 
              ${canAfford ? '' : 'disabled'}>
              Buy
            </button>
          </div>`;
      }).join('')}
    </div>`;

  el.querySelectorAll('.shop-buy-btn').forEach(btn => {
    btn.addEventListener('click', () => buyItem(btn.dataset.item));
  });
}

async function buyItem(itemKey) {
  try {
    const res = await api('/api/user/shop/buy', {
      method: 'POST',
      body: JSON.stringify({ itemKey })
    });
    setState({ coins: res.coins, gems: res.gems, theme: res.theme });
    toast('Purchase successful!', 'success');
    renderShop();
  } catch (e) {
    toast(e.message || 'Purchase failed', 'error');
  }
}

subscribe(['coins', 'gems'], renderCurrency);
