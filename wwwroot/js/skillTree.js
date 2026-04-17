import { getState, subscribe } from './state.js';

/**
 * Skill tree: visual branching tree with nodes that unlock as skills level up.
 * Health → branches into Endurance, Vitality
 * Study → branches into Focus, Knowledge  
 * Social → branches into Charisma, Empathy
 */

const TREE = {
  health: {
    label: 'Health',
    color: '#2dd4bf',
    nodes: [
      { id: 'h1', label: 'Health Lv 1', x: 50, y: 85, req: 1 },
      { id: 'h2', label: 'Endurance', x: 25, y: 55, req: 2, parent: 'h1' },
      { id: 'h3', label: 'Vitality', x: 75, y: 55, req: 3, parent: 'h1' },
      { id: 'h4', label: 'Iron Body', x: 25, y: 25, req: 5, parent: 'h2' },
      { id: 'h5', label: 'Peak Form', x: 75, y: 25, req: 5, parent: 'h3' },
    ]
  },
  study: {
    label: 'Study',
    color: '#7c5cff',
    nodes: [
      { id: 's1', label: 'Study Lv 1', x: 50, y: 85, req: 1 },
      { id: 's2', label: 'Focus', x: 25, y: 55, req: 2, parent: 's1' },
      { id: 's3', label: 'Knowledge', x: 75, y: 55, req: 3, parent: 's1' },
      { id: 's4', label: 'Deep Work', x: 25, y: 25, req: 5, parent: 's2' },
      { id: 's5', label: 'Scholar', x: 75, y: 25, req: 5, parent: 's3' },
    ]
  },
  social: {
    label: 'Social',
    color: '#fbbf24',
    nodes: [
      { id: 'so1', label: 'Social Lv 1', x: 50, y: 85, req: 1 },
      { id: 'so2', label: 'Charisma', x: 25, y: 55, req: 2, parent: 'so1' },
      { id: 'so3', label: 'Empathy', x: 75, y: 55, req: 3, parent: 'so1' },
      { id: 'so4', label: 'Leader', x: 25, y: 25, req: 5, parent: 'so2' },
      { id: 'so5', label: 'Diplomat', x: 75, y: 25, req: 5, parent: 'so3' },
    ]
  }
};

export function renderSkillTree() {
  const container = document.getElementById('skill-tree-container');
  if (!container) return;

  const skills = getState().skills || [];
  const skillLevels = {};
  skills.forEach(s => { skillLevels[s.type.toLowerCase()] = s.level; });

  let html = '<div class="skill-trees-row">';

  for (const [key, tree] of Object.entries(TREE)) {
    const level = skillLevels[key] || 1;
    html += `<div class="skill-tree-branch" data-skill="${key}">`;
    html += `<h4 class="tree-title" style="color:${tree.color}">${tree.label} Tree</h4>`;
    html += `<svg viewBox="0 0 100 100" class="tree-svg">`;

    // Draw connections first (behind nodes)
    tree.nodes.forEach(node => {
      if (node.parent) {
        const parent = tree.nodes.find(n => n.id === node.parent);
        if (parent) {
          const unlocked = level >= node.req;
          html += `<line x1="${parent.x}" y1="${parent.y}" x2="${node.x}" y2="${node.y}" 
            stroke="${unlocked ? tree.color : 'rgba(255,255,255,0.1)'}" 
            stroke-width="1.5" stroke-dasharray="${unlocked ? 'none' : '3,3'}"
            class="tree-line ${unlocked ? 'active' : ''}"/>`;
        }
      }
    });

    // Draw nodes
    tree.nodes.forEach(node => {
      const unlocked = level >= node.req;
      const isCurrent = level === node.req;
      html += `
        <g class="tree-node ${unlocked ? 'unlocked' : 'locked'} ${isCurrent ? 'current' : ''}" 
           data-node="${node.id}" data-req="${node.req}">
          <circle cx="${node.x}" cy="${node.y}" r="7" 
            fill="${unlocked ? tree.color : 'rgba(255,255,255,0.05)'}" 
            stroke="${unlocked ? tree.color : 'rgba(255,255,255,0.15)'}" 
            stroke-width="${isCurrent ? 2.5 : 1.5}"
            class="node-circle ${unlocked ? 'glow' : ''}"/>
          <text x="${node.x}" y="${node.y + 15}" text-anchor="middle" 
            fill="${unlocked ? 'var(--text)' : 'var(--muted)'}" 
            font-size="4" class="node-label">${node.label}</text>
        </g>`;
    });

    html += '</svg></div>';
  }

  html += '</div>';
  container.innerHTML = html;

  // Add hover tooltips
  container.querySelectorAll('.tree-node').forEach(node => {
    node.addEventListener('mouseenter', (e) => {
      const req = node.dataset.req;
      const label = node.querySelector('.node-label')?.textContent || '';
      showTooltip(e, `${label} — Requires Skill Level ${req}`);
    });
    node.addEventListener('mouseleave', hideTooltip);
  });
}

function showTooltip(e, text) {
  let tip = document.querySelector('.tree-tooltip');
  if (!tip) {
    tip = document.createElement('div');
    tip.className = 'tree-tooltip';
    document.body.appendChild(tip);
  }
  tip.textContent = text;
  tip.style.left = e.pageX + 12 + 'px';
  tip.style.top = e.pageY - 8 + 'px';
  tip.classList.add('visible');
}

function hideTooltip() {
  const tip = document.querySelector('.tree-tooltip');
  if (tip) tip.classList.remove('visible');
}

subscribe('skills', renderSkillTree);
