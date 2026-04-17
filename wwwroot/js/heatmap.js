import { api } from './api.js';
import { getState, setState, subscribe } from './state.js';

/**
 * GitHub-style activity heatmap calendar.
 */

let heatmapData = [];

export async function loadHeatmap() {
  try {
    const intel = await api('/api/user/intelligence');
    heatmapData = intel.heatmap || [];
    setState({ intelligence: intel });
    renderHeatmap();
  } catch (e) {
    console.warn('Heatmap load failed:', e);
  }
}

export function renderHeatmap() {
  const el = document.getElementById('heatmap-container');
  if (!el) return;

  const data = heatmapData;
  if (!data.length) {
    el.innerHTML = '<p class="muted">Activity data will appear as you complete tasks.</p>';
    return;
  }

  const maxCount = Math.max(1, ...data.map(d => d.count));
  const weeks = [];
  let currentWeek = [];

  // Group into weeks (7 days each)
  data.forEach((d, i) => {
    currentWeek.push(d);
    if ((i + 1) % 7 === 0 || i === data.length - 1) {
      weeks.push(currentWeek);
      currentWeek = [];
    }
  });

  const monthLabels = getMonthLabels(data);

  let html = '<div class="heatmap">';
  html += '<div class="heatmap-months">';
  monthLabels.forEach(m => {
    html += `<span class="heatmap-month">${m.label}</span>`;
  });
  html += '</div>';

  html += '<div class="heatmap-grid">';
  // Day labels
  html += '<div class="heatmap-day-labels">';
  ['Mon', '', 'Wed', '', 'Fri', '', 'Sun'].forEach(d => {
    html += `<span class="heatmap-day-label">${d}</span>`;
  });
  html += '</div>';

  html += '<div class="heatmap-weeks">';
  weeks.forEach(week => {
    html += '<div class="heatmap-week">';
    week.forEach(day => {
      const level = getLevel(day.count, maxCount);
      const dateStr = day.date;
      html += `<div class="heatmap-cell level-${level}" 
        data-date="${dateStr}" data-count="${day.count}"
        title="${dateStr}: ${day.count} tasks"></div>`;
    });
    html += '</div>';
  });
  html += '</div></div>';

  // Legend
  html += '<div class="heatmap-legend">';
  html += '<span class="muted small">Less</span>';
  for (let i = 0; i <= 4; i++) {
    html += `<div class="heatmap-cell level-${i}"></div>`;
  }
  html += '<span class="muted small">More</span>';
  html += '</div>';

  html += '</div>';
  el.innerHTML = html;

  // Tooltip on hover
  el.querySelectorAll('.heatmap-cell[data-date]').forEach(cell => {
    cell.addEventListener('mouseenter', (e) => {
      const count = cell.dataset.count;
      const date = cell.dataset.date;
      showHeatmapTooltip(e, `${date}: ${count} task${count !== '1' ? 's' : ''}`);
    });
    cell.addEventListener('mouseleave', hideHeatmapTooltip);
  });
}

function getLevel(count, max) {
  if (count === 0) return 0;
  const pct = count / max;
  if (pct < 0.25) return 1;
  if (pct < 0.5) return 2;
  if (pct < 0.75) return 3;
  return 4;
}

function getMonthLabels(data) {
  const labels = [];
  let lastMonth = '';
  data.forEach(d => {
    const m = d.date.substring(0, 7);
    if (m !== lastMonth) {
      const date = new Date(d.date);
      labels.push({ index: labels.length, label: date.toLocaleDateString('en', { month: 'short' }) });
      lastMonth = m;
    }
  });
  return labels;
}

function showHeatmapTooltip(e, text) {
  let tip = document.querySelector('.heatmap-tooltip');
  if (!tip) {
    tip = document.createElement('div');
    tip.className = 'heatmap-tooltip';
    document.body.appendChild(tip);
  }
  tip.textContent = text;
  tip.style.left = e.pageX + 12 + 'px';
  tip.style.top = e.pageY - 8 + 'px';
  tip.classList.add('visible');
}

function hideHeatmapTooltip() {
  const tip = document.querySelector('.heatmap-tooltip');
  if (tip) tip.classList.remove('visible');
}
