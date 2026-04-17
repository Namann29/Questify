import { getState, subscribe } from './state.js';

export function renderQuestChains() {
  const el = document.getElementById('quest-chains-container');
  if (!el) return;

  const chains = getState().questChains || [];

  if (!chains.length) {
    el.innerHTML = '<p class="muted quest-chain-empty">No active quest chains. Complete tasks to unlock story quests!</p>';
    return;
  }

  el.innerHTML = chains.map(chain => {
    const totalSteps = chain.steps.length;
    const completedSteps = chain.steps.filter(s => s.isComplete).length;
    const progressPct = totalSteps > 0 ? (completedSteps / totalSteps) * 100 : 0;

    return `
      <div class="quest-chain-card glass-inner ${chain.completed ? 'chain-complete' : ''}">
        <div class="chain-header">
          <div>
            <h4>${esc(chain.title)}</h4>
            <p class="muted small">${esc(chain.description)}</p>
          </div>
          <span class="chain-badge ${chain.completed ? 'badge-complete' : 'badge-active'}">
            ${chain.completed ? 'Complete' : 'Active'}
          </span>
        </div>
        <div class="chain-progress">
          <div class="chain-progress-bar">
            <div class="chain-progress-fill" style="width:${progressPct}%"></div>
          </div>
          <span class="chain-progress-label">${completedSteps}/${totalSteps} steps</span>
        </div>
        <div class="chain-steps">
          ${chain.steps.map((step, i) => {
            const stepPct = step.requiredCount > 0 ? (step.completedCount / step.requiredCount) * 100 : 0;
            return `
              <div class="chain-step ${step.isComplete ? 'step-done' : ''}">
                <div class="step-connector ${i > 0 ? '' : 'first'}"></div>
                <div class="step-node">
                  <div class="step-dot ${step.isComplete ? 'dot-complete' : ''}"></div>
                </div>
                <div class="step-info">
                  <p class="step-title">${esc(step.title)}</p>
                  <div class="step-mini-bar">
                    <div class="step-mini-fill" style="width:${stepPct}%"></div>
                  </div>
                  <p class="muted small">${step.completedCount}/${step.requiredCount} · +${step.xpBonus} XP bonus</p>
                </div>
              </div>`;
          }).join('')}
        </div>
      </div>`;
  }).join('');
}

function esc(s) {
  return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

subscribe('questChains', renderQuestChains);
