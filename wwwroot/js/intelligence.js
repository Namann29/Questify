import { api } from './api.js';
import { getState, setState, subscribe } from './state.js';

export async function loadIntelligence() {
  try {
    const intel = await api('/api/user/intelligence');
    setState({ intelligence: intel });
    renderIntelligence();
  } catch (e) {
    console.warn('Intelligence load failed:', e);
  }
}

export function renderIntelligence() {
  const el = document.getElementById('intelligence-container');
  if (!el) return;

  const intel = getState().intelligence;
  if (!intel) {
    el.innerHTML = '<p class="muted">Loading insights...</p>';
    return;
  }

  const dist = intel.skillDistribution || {};
  const totalTasks = Object.values(dist).reduce((a, b) => a + b, 0) || 1;

  el.innerHTML = `
    <div class="intel-grid">
      <div class="intel-card glass-inner">
        <div class="intel-icon">&#128336;</div>
        <div class="intel-info">
          <p class="intel-label">Peak Productivity</p>
          <p class="intel-value">${intel.mostProductiveHour}</p>
          <p class="muted small">When you complete the most tasks</p>
        </div>
      </div>
      <div class="intel-card glass-inner">
        <div class="intel-icon">&#127942;</div>
        <div class="intel-info">
          <p class="intel-label">Top Skill</p>
          <p class="intel-value">${intel.topSkill}</p>
          <p class="muted small">Your strongest category</p>
        </div>
      </div>
      <div class="intel-card glass-inner">
        <div class="intel-icon">&#128200;</div>
        <div class="intel-info">
          <p class="intel-label">14-Day Completion</p>
          <p class="intel-value">${intel.completionRate14d}%</p>
          <div class="intel-mini-bar"><div class="intel-mini-fill" style="width:${intel.completionRate14d}%"></div></div>
        </div>
      </div>
      <div class="intel-card glass-inner">
        <div class="intel-icon">&#128293;</div>
        <div class="intel-info">
          <p class="intel-label">Streak Efficiency</p>
          <p class="intel-value">${intel.streakEfficiency}%</p>
          <p class="muted small">${intel.currentStreak} day streak</p>
        </div>
      </div>
    </div>

    <div class="intel-skill-dist">
      <h4>Skill Distribution</h4>
      <div class="intel-dist-bars">
        ${Object.entries(dist).map(([skill, count]) => {
          const pct = Math.round((count / totalTasks) * 100);
          const color = skill === 'Health' ? 'var(--accent2)' : skill === 'Study' ? 'var(--accent)' : 'var(--sun)';
          return `
            <div class="intel-dist-row">
              <span class="intel-dist-label">${skill}</span>
              <div class="intel-dist-bar"><div class="intel-dist-fill" style="width:${pct}%;background:${color}"></div></div>
              <span class="intel-dist-count">${count} (${pct}%)</span>
            </div>`;
        }).join('')}
      </div>
    </div>

    ${(intel.recommendations && intel.recommendations.length) ? `
    <div class="intel-reco">
      <h4>Smart Recommendations</h4>
      <ul class="intel-reco-list">
        ${intel.recommendations.map(r => `<li class="intel-reco-item">${r}</li>`).join('')}
      </ul>
    </div>` : ''}
  `;
}

subscribe('intelligence', renderIntelligence);
