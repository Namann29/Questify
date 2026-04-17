import { api } from './api.js';
import { getState, setState, subscribe } from './state.js';
import { toast, xpPopup } from './notifications.js';

const $ = (s) => document.querySelector(s);

/* ───── Render ───── */
export function renderHabits() {
  const container = $('#habits-list');
  if (!container) return;

  const habits = getState().habits || [];
  if (!habits.length) {
    container.innerHTML = `
      <div class="habits-empty">
        <span class="habits-empty-icon">🔄</span>
        <p>No habits yet. Add your first repeatable habit!</p>
      </div>`;
    return;
  }

  container.innerHTML = habits.map(habitRow).join('');
}

function habitRow(h) {
  const freqLabel = h.frequency === 'Weekly' ? 'Weekly' : 'Daily';
  const freqClass = h.frequency === 'Weekly' ? 'freq-weekly' : 'freq-daily';
  const streakIcon = h.streak >= 7 ? '🔥' : h.streak >= 3 ? '⚡' : '🔄';
  const completedClass = h.isCompletedInCurrentCycle ? 'habit-done' : '';
  const checkClass = h.isCompletedInCurrentCycle ? 'habit-check checked' : 'habit-check';
  const skillColors = { Health: '#2dd4bf', Study: '#7c5cff', Social: '#fbbf24' };
  const skillColor = skillColors[h.skillType] || '#7c5cff';

  return `
    <li class="habit-row ${completedClass}" data-id="${h.id}">
      <div class="habit-main">
        <button class="${checkClass}" data-id="${h.id}" ${h.isCompletedInCurrentCycle ? 'disabled' : ''}
                title="${h.isCompletedInCurrentCycle ? 'Completed this cycle' : 'Mark complete'}"></button>
        <div class="habit-info">
          <p class="habit-title">${esc(h.title)}</p>
          <div class="habit-meta">
            <span class="habit-freq ${freqClass}">${freqLabel}</span>
            <span class="habit-skill" style="color:${skillColor}">${h.skillType}</span>
            <span class="habit-streak">${streakIcon} ${h.streak} day streak</span>
            <span class="habit-best muted">Best: ${h.bestStreak}</span>
          </div>
        </div>
        <button class="habit-delete" data-id="${h.id}" title="Delete habit">✕</button>
      </div>
    </li>`;
}

function esc(s) { return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;'); }

/* ───── Complete Habit ───── */
export async function completeHabit(habitId) {
  try {
    const res = await api(`/api/habits/${habitId}/complete`, { method: 'POST' });

    setState({
      user: res.user,
      coins: res.user.coins,
      gems: res.user.gems,
    });

    if (res.xpAwarded > 0) xpPopup(res.xpAwarded);
    if (res.streakBonus > 0) toast(`Streak bonus: +${res.streakBonus} XP!`, 'success');

    // Refresh full dashboard
    const fresh = await api('/api/user/dashboard');
    setState({
      habits: fresh.habits || [],
      activity: fresh.activity,
      skills: fresh.skills,
      tasks: fresh.tasks,
    });

    renderHabits();

    // Trigger level-up celebrations from dashboard.js (imported there)
    if (res.leveledUp) {
      document.dispatchEvent(new CustomEvent('game:level-up', { detail: { level: res.user.level } }));
    }
  } catch (e) {
    toast(e.message || 'Habit completion failed', 'error');
  }
}

/* ───── Delete Habit ───── */
export async function deleteHabit(habitId) {
  try {
    await api(`/api/habits/${habitId}`, { method: 'DELETE' });
    toast('Habit removed', 'info');

    const fresh = await api('/api/user/dashboard');
    setState({ habits: fresh.habits || [] });
    renderHabits();
  } catch (e) {
    toast(e.message || 'Delete failed', 'error');
  }
}

/* ───── Create Habit ───── */
export async function createHabit(title, frequency, skillType) {
  try {
    await api('/api/habits', {
      method: 'POST',
      body: JSON.stringify({ title, frequency, skillType }),
    });
    toast('Habit created!', 'success');

    const fresh = await api('/api/user/dashboard');
    setState({ habits: fresh.habits || [] });
    renderHabits();
  } catch (e) {
    toast(e.message || 'Failed to create habit', 'error');
  }
}

/* ───── Click Delegation ───── */
document.addEventListener('click', (e) => {
  const checkBtn = e.target.closest('.habit-check:not(:disabled)');
  if (checkBtn) {
    completeHabit(+checkBtn.dataset.id);
    return;
  }

  const delBtn = e.target.closest('.habit-delete');
  if (delBtn) {
    if (confirm('Delete this habit?')) {
      deleteHabit(+delBtn.dataset.id);
    }
    return;
  }
});

/* ───── Habit Modal ───── */
document.addEventListener('click', (e) => {
  if (e.target.closest('#open-habit-modal')) openHabitModal();
  if (e.target.closest('#close-habit-modal')) closeHabitModal();
});

function openHabitModal() {
  const modal = $('#habit-modal');
  if (modal) { modal.hidden = false; document.body.classList.add('modal-open'); }
}

function closeHabitModal() {
  const modal = $('#habit-modal');
  if (modal) { modal.hidden = true; document.body.classList.remove('modal-open'); }
}

$('#habit-form')?.addEventListener('submit', async (e) => {
  e.preventDefault();
  const fd = new FormData(e.target);
  await createHabit(fd.get('title'), fd.get('frequency'), fd.get('skillType'));
  closeHabitModal();
  e.target.reset();
});
