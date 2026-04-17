import { api, clearToken } from './api.js';
import { getState, setState, subscribe } from './state.js';
import { initTheme, switchTheme } from './themes.js';
import { toast, xpPopup, coinPopup, achievementPopup, levelUpPopup } from './notifications.js';
import { renderAchievements } from './achievements.js';
import { renderSkillTree } from './skillTree.js';
import { renderQuestChains } from './questChain.js';
import { renderCurrency, renderShop } from './currency.js';
import { loadHeatmap } from './heatmap.js';
import { loadIntelligence } from './intelligence.js';
import { showOnboarding } from './onboarding.js';
import { renderHabits } from './habitTracker.js';

const $ = (s) => document.querySelector(s);
const $$ = (s) => document.querySelectorAll(s);

document.addEventListener('DOMContentLoaded', boot);

async function boot() {
  initTheme();
  setupNavigation();
  setupMobileMenu();
  setupThemeSwitcher();
  setupLogout();

  await loadDashboard();

  if (!getState().onboardingDone) {
    setTimeout(() => showOnboarding(), 800);
  }
}

async function loadDashboard() {
  try {
    const data = await api('/api/user/dashboard');
    setState({
      user: data.user,
      skills: data.skills,
      tasks: data.tasks,
      activity: data.activity,
      difficulty: data.difficulty,
      achievements: data.achievements || [],
      questChains: data.questChains || [],
      habits: data.habits || [],
      coins: data.user.coins,
      gems: data.user.gems,
      theme: data.user.theme,
      onboardingDone: data.user.onboardingDone,
    });

    renderUser(data.user);
    renderSkills(data.skills);
    renderTasks(data.tasks);
    renderFeed(data.activity);
    renderDifficulty(data.difficulty);
    renderCurrency();
    renderAchievements();
    renderSkillTree();
    renderQuestChains();
    renderHabits();
    renderShop();

    if (data.user.theme) switchTheme(data.user.theme);
  } catch (e) {
    if (e.status === 401) { clearToken(); location.href = '/login.html'; }
  }
}

/* ───── SPA Navigation ───── */
function setupNavigation() {
  $$('.nav-btn').forEach(btn => {
    btn.addEventListener('click', () => navigateTo(btn.dataset.page));
  });
}

function navigateTo(page) {
  $$('.nav-btn').forEach(b => {
    b.classList.toggle('active', b.dataset.page === page);
    b.removeAttribute('aria-current');
    if (b.dataset.page === page) b.setAttribute('aria-current', 'page');
  });

  $$('.page').forEach(p => p.classList.remove('active'));
  const target = $(`#page-${page}`);
  if (target) target.classList.add('active');

  setState({ currentPage: page });

  if (page === 'analytics') loadCharts();
  if (page === 'intelligence') { loadIntelligence(); loadHeatmap(); }
  if (page === 'shop') renderShop();
  if (page === 'skills') renderSkillTree();
  if (page === 'achievements') renderAchievements();
  if (page === 'chains') renderQuestChains();
  if (page === 'habits') renderHabits();

  $('#sidebar')?.classList.remove('open');
}

function setupMobileMenu() {
  const hamburger = $('#hamburger-btn');
  const sidebar = $('#sidebar');
  if (hamburger && sidebar) {
    hamburger.addEventListener('click', () => sidebar.classList.toggle('open'));
    document.addEventListener('click', (e) => {
      if (sidebar.classList.contains('open') && !sidebar.contains(e.target) && e.target !== hamburger) {
        sidebar.classList.remove('open');
      }
    });
  }
}

function setupThemeSwitcher() {
  $$('.theme-dot').forEach(dot => {
    dot.addEventListener('click', () => {
      switchTheme(dot.dataset.theme);
      $$('.theme-dot').forEach(d => d.classList.remove('active'));
      dot.classList.add('active');
    });
  });
}

function setupLogout() {
  $('#logout-btn')?.addEventListener('click', () => {
    clearToken();
    location.href = '/login.html';
  });
}

/* ───── User / Hero ───── */
function renderUser(u) {
  $('#hero-username').textContent = u.username;
  $('#hero-level-caption').textContent = `Level ${u.level} — progress to next`;
  $('#total-xp-chip').textContent = `${u.totalXp} XP lifetime`;
  $('#level-val').textContent = u.level;
  $('#streak-val').textContent = u.streak;

  const streakM = $('#streak-val-m');
  const levelM = $('#level-val-m');
  if (streakM) streakM.textContent = u.streak;
  if (levelM) levelM.textContent = u.level;

  setXpBar(u.xpIntoCurrentLevel, u.xpRequiredForNextLevel, u.level + 1);
}

function setXpBar(into, need, nextLevel) {
  const pct = need > 0 ? Math.min((into / need) * 100, 100) : 0;
  const fill = $('#xp-bar-fill');
  if (fill) fill.style.width = `${pct}%`;
  const xpInto = $('#xp-into');
  const xpNeed = $('#xp-need');
  const xpNext = $('#xp-next-level');
  const xpBar = $('#xp-bar');
  if (xpInto) xpInto.textContent = into;
  if (xpNeed) xpNeed.textContent = need;
  if (xpNext) xpNext.textContent = nextLevel;
  if (xpBar) xpBar.setAttribute('aria-valuenow', Math.round(pct));
}

/* ───── Skills ───── */
function renderSkills(skills) {
  const grid = $('#skills-grid');
  if (!grid) return;
  grid.innerHTML = skills.map(s => {
    const pct = s.xpRequiredForNextLevel > 0
      ? Math.min((s.xpIntoCurrentLevel / s.xpRequiredForNextLevel) * 100, 100) : 0;
    const colors = {
      Health: { bar: '#2dd4bf', glow: 'rgba(45,212,191,0.4)' },
      Study:  { bar: '#7c5cff', glow: 'rgba(124,92,255,0.4)' },
      Social: { bar: '#fbbf24', glow: 'rgba(251,191,36,0.4)' },
    };
    const c = colors[s.label] || colors.Study;
    return `
      <div class="skill-card" data-skill="${s.label}">
        <div class="skill-head">
          <span class="skill-ico" aria-hidden="true">${
            s.icon === 'heart' ? '❤️' : s.icon === 'book' ? '📖' : s.icon === 'users' ? '👥' : '✨'}</span>
          <div>
            <p class="skill-name">${s.label}</p>
            <p class="skill-lvl">Level ${s.level}</p>
          </div>
        </div>
        <div class="skill-xp-bar">
          <div class="skill-xp-fill" style="width:${pct}%;background:${c.bar};box-shadow:0 0 12px ${c.glow}"></div>
        </div>
        <p class="skill-xp-text muted small">${s.xpIntoCurrentLevel}/${s.xpRequiredForNextLevel} XP</p>
      </div>`;
  }).join('');
}

/* ───── Tasks ───── */
function renderTasks(tasks) {
  const active = tasks.filter(t => !t.completed);
  const done = tasks.filter(t => t.completed);
  const html = [
    ...active.map(taskRow),
    done.length ? `<li class="task-divider"><span>Completed</span></li>` : '',
    ...done.map(taskRow),
  ].join('');

  const list = $('#task-list');
  const listFull = $('#task-list-full');
  if (list) list.innerHTML = html;
  if (listFull) listFull.innerHTML = html;
}

function taskRow(t) {
  const diffColors = { Easy: '#2dd4bf', Medium: '#7c5cff', Hard: '#fb7185' };
  const c = diffColors[t.difficulty] || '#7c5cff';
  return `
    <li class="task-row ${t.completed ? 'done' : ''}" data-id="${t.id}">
      <div class="task-main">
        <button class="task-check ${t.completed ? 'checked' : ''}" data-id="${t.id}" ${t.completed ? 'disabled' : ''}></button>
        <div class="task-info">
          <p class="task-title">${esc(t.title)}</p>
          <div class="task-meta">
            <span class="pill pill-sm" style="border-color:${c}55;color:${c}">${t.difficulty}</span>
            <span class="pill pill-sm">${t.skillType}</span>
            <span class="pill pill-sm">+${t.xpReward} XP</span>
            ${t.coinReward ? `<span class="pill pill-sm coin-pill-sm">+${t.coinReward} coins</span>` : ''}
          </div>
        </div>
      </div>
    </li>`;
}

function esc(s) { return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;'); }

/* ───── Activity Feed ───── */
function renderFeed(items) {
  const feed = $('#activity-feed');
  if (!feed) return;
  feed.innerHTML = items.map(a => {
    const ico = a.kind === 'LevelUp' ? '⬆️' : a.kind === 'SkillLevelUp' ? '🌟' : a.kind === 'Streak' ? '🔥' : a.kind === 'QuestAdded' ? '📋' : '✅';
    return `
      <li class="feed-item">
        <span class="feed-ico">${ico}</span>
        <div>
          <p>${esc(a.message)}</p>
          <time class="muted small">${timeAgo(a.createdAtUtc)}</time>
        </div>
      </li>`;
  }).join('');
}

function timeAgo(iso) {
  const diff = Date.now() - new Date(iso).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return 'just now';
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  return `${Math.floor(hrs / 24)}d ago`;
}

/* ───── Difficulty ───── */
function renderDifficulty(d) {
  if (!d) return;
  const l = $('#reco-label');
  const r = $('#reco-reason');
  const rate = $('#reco-rate');
  const done = $('#reco-done');
  if (l) l.textContent = d.suggested;
  if (r) r.textContent = d.reason;
  if (rate) rate.textContent = `${d.completionRate}%`;
  if (done) done.textContent = `${d.completedLast7Days}/${d.totalActiveLast7Days}`;
}

/* ───── Task Completion ───── */
document.addEventListener('click', (e) => {
  const btn = e.target.closest('.task-check');
  if (!btn || btn.disabled) return;
  completeTask(+btn.dataset.id);
});

async function completeTask(taskId) {
  try {
    const res = await api(`/api/tasks/${taskId}/complete`, { method: 'POST' });

    setState({ user: res.user, skills: res.skills, coins: res.user.coins, gems: res.user.gems });

    renderUser(res.user);
    renderSkills(res.skills);
    renderCurrency();

    const fresh = await api('/api/user/dashboard');
    setState({
      tasks: fresh.tasks,
      activity: fresh.activity,
      difficulty: fresh.difficulty,
      achievements: fresh.achievements || [],
      questChains: fresh.questChains || [],
      habits: fresh.habits || [],
    });

    renderTasks(fresh.tasks);
    renderFeed(fresh.activity);
    renderDifficulty(fresh.difficulty);
    renderAchievements();
    renderSkillTree();
    renderQuestChains();
    renderHabits();

    if (res.task?.xpReward) xpPopup(res.task.xpReward);
    if (res.coinsEarned) setTimeout(() => coinPopup(res.coinsEarned), 400);

    if (res.newAchievements?.length) {
      res.newAchievements.forEach((a, i) => {
        setTimeout(() => achievementPopup(a.title, a.description, a.icon), 800 + i * 1200);
      });
    }

    if (res.celebrations?.includes('level_up')) {
      fireConfetti(); playLevelUp(); levelUpPopup(res.user.level);
    }
    if (res.celebrations?.includes('skill_up')) fireConfetti();
    if (res.celebrations?.includes('achievement')) fireConfetti();
    if (res.celebrations?.includes('chain_complete')) {
      fireConfetti(); toast('Quest chain completed! Check Quest Chains.', 'success');
    }

    if (res.user.streak > 0) toast(`Streak: ${res.user.streak} days! Keep it up!`, 'info');
  } catch (e) {
    toast(e.message || 'Task completion failed', 'error');
  }
}

/* ───── Confetti ───── */
function fireConfetti() {
  if (typeof confetti === 'undefined') return;
  confetti({ particleCount: 120, spread: 80, origin: { y: 0.6 }, colors: ['#7c5cff','#2dd4bf','#fbbf24','#fb7185'] });
}

/* ───── Sound ───── */
function playLevelUp() {
  try {
    const ctx = new AudioContext();
    const o = ctx.createOscillator();
    const g = ctx.createGain();
    o.connect(g); g.connect(ctx.destination);
    o.type = 'sine';
    o.frequency.setValueAtTime(523, ctx.currentTime);
    o.frequency.exponentialRampToValueAtTime(1047, ctx.currentTime + 0.15);
    g.gain.setValueAtTime(0.18, ctx.currentTime);
    g.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.6);
    o.start(ctx.currentTime);
    o.stop(ctx.currentTime + 0.6);
  } catch (_) {}
}

/* ───── Quest Modal ───── */
document.addEventListener('click', (e) => {
  if (e.target.closest('#open-quest-modal') || e.target.closest('#open-quest-modal-2')) openQuestModal();
  if (e.target.closest('#close-quest-modal') || e.target.closest('[data-close]')) closeQuestModal();
});

function openQuestModal() {
  $('#quest-modal').hidden = false;
  document.body.classList.add('modal-open');
  $('#quest-form')?.querySelector('input')?.focus();
}
function closeQuestModal() {
  $('#quest-modal').hidden = true;
  document.body.classList.remove('modal-open');
}

$('#quest-form')?.addEventListener('submit', async (e) => {
  e.preventDefault();
  const fd = new FormData(e.target);
  try {
    await api('/api/tasks', {
      method: 'POST',
      body: JSON.stringify({ title: fd.get('title'), difficulty: fd.get('difficulty'), skillType: fd.get('skillType') }),
    });
    closeQuestModal();
    e.target.reset();
    toast('Quest added!', 'success');
    loadDashboard();
  } catch (err) {
    const errEl = $('#quest-error');
    if (errEl) { errEl.textContent = err.message || 'Failed to create quest'; errEl.hidden = false; }
  }
});

/* ───── Charts ───── */
let weeklyChart, skillChart;

async function loadCharts() {
  try {
    const data = await api('/api/user/analytics');
    renderCharts(data);
  } catch (_) {}
}

function renderCharts(data) {
  const cs = getComputedStyle(document.documentElement);
  const chartText = cs.getPropertyValue('--chart-text').trim() || 'rgba(236,242,255,0.78)';
  const chartGrid = cs.getPropertyValue('--chart-grid').trim() || 'rgba(255,255,255,0.06)';

  const common = {
    responsive: true,
    animation: { duration: 800, easing: 'easeOutQuart' },
    plugins: { legend: { labels: { color: chartText, font: { family: 'DM Sans' } } } },
    scales: {
      x: { ticks: { color: chartText }, grid: { color: chartGrid } },
      y: { ticks: { color: chartText }, grid: { color: chartGrid }, beginAtZero: true },
    },
  };

  const wCtx = $('#chart-weekly')?.getContext('2d');
  if (wCtx) {
    if (weeklyChart) weeklyChart.destroy();
    weeklyChart = new Chart(wCtx, {
      type: 'bar',
      data: {
        labels: data.weekLabels,
        datasets: [{
          label: 'XP earned',
          data: data.xpPerDay,
          backgroundColor: 'rgba(124,92,255,0.55)',
          borderColor: '#7c5cff',
          borderWidth: 1,
          borderRadius: 6,
        }, {
          label: 'Cumulative XP',
          data: data.cumulativeTotalXpPerDay,
          type: 'line',
          borderColor: '#2dd4bf',
          backgroundColor: 'rgba(45,212,191,0.12)',
          fill: true,
          tension: 0.35,
          pointRadius: 4,
          pointBackgroundColor: '#2dd4bf',
        }],
      },
      options: common,
    });
  }

  const sCtx = $('#chart-skills')?.getContext('2d');
  if (sCtx) {
    const skillColors = { Health: '#2dd4bf', Study: '#7c5cff', Social: '#fbbf24' };
    if (skillChart) skillChart.destroy();
    skillChart = new Chart(sCtx, {
      type: 'line',
      data: {
        labels: data.weekLabels,
        datasets: Object.entries(data.skillXpPerDay).map(([name, vals]) => ({
          label: name,
          data: vals,
          borderColor: skillColors[name] || '#7c5cff',
          backgroundColor: (skillColors[name] || '#7c5cff') + '22',
          fill: true,
          tension: 0.35,
          pointRadius: 3,
        })),
      },
      options: common,
    });
  }
}
