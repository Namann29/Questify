import { api } from './api.js';
import { getState, setState } from './state.js';

const STEPS = [
  {
    title: 'Welcome to Life as a Game!',
    desc: 'Turn your daily routine into an RPG. Complete tasks, earn XP, level up skills, and keep streaks alive.',
    icon: '🎮',
  },
  {
    title: 'Create Your First Quest',
    desc: 'Click "+ New quest" to add a task. Choose difficulty and skill type to earn XP and coins.',
    icon: '⚔️',
  },
  {
    title: 'Level Up Your Skills',
    desc: 'Complete tasks in Health, Study, and Social to level up each skill track. Unlock the skill tree as you grow!',
    icon: '📈',
  },
  {
    title: 'Earn Coins & Gems',
    desc: 'Every task earns coins. Unlock achievements to earn gems. Spend them in the Shop on themes and boosts!',
    icon: '💰',
  },
  {
    title: 'Keep Your Streak Alive',
    desc: 'Complete at least one task daily to maintain your streak. Use Streak Freeze from the shop if you miss a day.',
    icon: '🔥',
  },
];

let currentStep = 0;

export function showOnboarding() {
  const state = getState();
  if (state.onboardingDone) return;

  const overlay = document.createElement('div');
  overlay.className = 'onboarding-overlay';
  overlay.id = 'onboarding-overlay';
  overlay.innerHTML = `
    <div class="onboarding-card glass">
      <div class="onboarding-progress">
        ${STEPS.map((_, i) => `<div class="onboarding-dot ${i <= 0 ? 'active' : ''}"></div>`).join('')}
      </div>
      <div class="onboarding-icon" id="onboarding-icon">${STEPS[0].icon}</div>
      <h3 id="onboarding-title">${STEPS[0].title}</h3>
      <p id="onboarding-desc">${STEPS[0].desc}</p>
      <div class="onboarding-actions">
        <button class="btn btn-ghost" id="onboarding-skip">Skip tour</button>
        <button class="btn btn-primary btn-glow" id="onboarding-next">
          <span class="btn-label">Next</span>
          <span class="btn-shine" aria-hidden="true"></span>
        </button>
      </div>
    </div>`;
  document.body.appendChild(overlay);
  requestAnimationFrame(() => overlay.classList.add('visible'));

  document.getElementById('onboarding-next').addEventListener('click', nextStep);
  document.getElementById('onboarding-skip').addEventListener('click', finishOnboarding);
}

function nextStep() {
  currentStep++;
  if (currentStep >= STEPS.length) {
    finishOnboarding();
    return;
  }

  const step = STEPS[currentStep];
  document.getElementById('onboarding-icon').textContent = step.icon;
  document.getElementById('onboarding-title').textContent = step.title;
  document.getElementById('onboarding-desc').textContent = step.desc;

  // Update next button label on last step
  if (currentStep === STEPS.length - 1) {
    document.getElementById('onboarding-next').querySelector('.btn-label').textContent = "Let's go!";
  }

  // Update dots
  document.querySelectorAll('.onboarding-dot').forEach((dot, i) => {
    dot.classList.toggle('active', i <= currentStep);
    dot.classList.toggle('done', i < currentStep);
  });
}

async function finishOnboarding() {
  const overlay = document.getElementById('onboarding-overlay');
  if (overlay) {
    overlay.classList.remove('visible');
    overlay.addEventListener('transitionend', () => overlay.remove());
  }

  setState({ onboardingDone: true });

  try {
    await api('/api/user/onboarding', {
      method: 'POST',
      body: JSON.stringify({ done: true })
    });
  } catch (e) {
    console.warn('Onboarding save failed:', e);
  }
}
