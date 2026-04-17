import { api, setToken } from './api.js';

const form = document.getElementById('auth-form');
const errEl = document.getElementById('auth-error');
const submitBtn = document.getElementById('auth-submit');
const tabs = document.querySelectorAll('.tab');

let mode = 'login';

function setMode(next) {
  mode = next;
  tabs.forEach((t) => {
    const on = t.dataset.mode === mode;
    t.classList.toggle('active', on);
    t.setAttribute('aria-selected', on ? 'true' : 'false');
  });
  submitBtn.querySelector('.btn-label').textContent =
    mode === 'login' ? 'Enter the arena' : 'Create your legend';
}

tabs.forEach((t) => {
  t.addEventListener('click', () => setMode(t.dataset.mode));
});

form.addEventListener('submit', async (e) => {
  e.preventDefault();
  errEl.hidden = true;
  const fd = new FormData(form);
  const username = String(fd.get('username') || '').trim();
  const password = String(fd.get('password') || '');

  submitBtn.disabled = true;
  submitBtn.classList.add('is-loading');

  try {
    const path = mode === 'login' ? '/api/auth/login' : '/api/auth/register';
    const data = await api(path, {
      method: 'POST',
      body: JSON.stringify({ username, password }),
    });
    setToken(data.token);
    window.location.href = '/dashboard.html';
  } catch (ex) {
    errEl.textContent =
      ex.message === 'UNAUTHORIZED'
        ? 'Invalid credentials.'
        : ex.message || 'Something went wrong.';
    errEl.hidden = false;
  } finally {
    submitBtn.disabled = false;
    submitBtn.classList.remove('is-loading');
  }
});
