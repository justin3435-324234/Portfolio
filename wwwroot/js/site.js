// Loader
window.addEventListener('load', () => {
  setTimeout(() => document.getElementById('loader')?.classList.add('hide'), 450);
});
setTimeout(() => document.getElementById('loader')?.classList.add('hide'), 2500);

// Navbar shrink + mobile toggle
const nav = document.getElementById('mainNav');
window.addEventListener('scroll', () => {
  nav?.classList.toggle('shrink', window.scrollY > 30);
}, { passive: true });
document.getElementById('navToggle')?.addEventListener('click', () =>
  document.getElementById('navLinks')?.classList.toggle('open'));

// Scroll reveal
const io = new IntersectionObserver(entries => {
  entries.forEach(e => { if (e.isIntersecting) { e.target.classList.add('in'); io.unobserve(e.target); } });
}, { threshold: 0.12 });
document.querySelectorAll('.reveal').forEach(el => io.observe(el));

// Magnetic buttons (subtle, disabled on touch / reduced motion)
if (!matchMedia('(prefers-reduced-motion: reduce)').matches && matchMedia('(pointer:fine)').matches) {
  document.querySelectorAll('.magnetic').forEach(btn => {
    btn.addEventListener('mousemove', e => {
      const r = btn.getBoundingClientRect();
      const x = (e.clientX - r.left - r.width / 2) * 0.12;
      const y = (e.clientY - r.top - r.height / 2) * 0.18;
      btn.style.transform = `translate(${x}px,${y}px)`;
    });
    btn.addEventListener('mouseleave', () => btn.style.transform = '');
  });
}

// Starfield particles (lightweight)
(() => {
  if (matchMedia('(prefers-reduced-motion: reduce)').matches) return;
  const c = document.getElementById('stars');
  if (!c) return;
  const ctx = c.getContext('2d');
  let stars = [];
  const resize = () => {
    c.width = innerWidth; c.height = innerHeight;
    const n = innerWidth < 700 ? 60 : 130;
    stars = Array.from({ length: n }, () => ({
      x: Math.random() * c.width, y: Math.random() * c.height,
      r: Math.random() * 1.6 + .3, s: Math.random() * .35 + .08,
      o: Math.random() * .6 + .2
    }));
  };
  resize(); addEventListener('resize', resize);
  (function tick() {
    ctx.clearRect(0, 0, c.width, c.height);
    for (const st of stars) {
      st.y -= st.s; if (st.y < 0) st.y = c.height;
      ctx.globalAlpha = st.o;
      ctx.fillStyle = '#c4b5fd';
      ctx.beginPath(); ctx.arc(st.x, st.y, st.r, 0, 7); ctx.fill();
    }
    ctx.globalAlpha = 1;
    requestAnimationFrame(tick);
  })();
})();

// Contact modal
(() => {
  const modal = document.getElementById('contactModal');
  if (!modal) return;
  const open = () => { modal.hidden = false; };
  const close = () => { modal.hidden = true; };
  document.querySelectorAll('[data-open-contact]').forEach(b => b.addEventListener('click', open));
  modal.querySelectorAll('[data-close-contact]').forEach(b => b.addEventListener('click', close));
  modal.addEventListener('click', e => { if (e.target === modal) close(); });
  document.addEventListener('keydown', e => { if (e.key === 'Escape') close(); });
})();

// Toast auto-hide + confirm dialogs
setTimeout(() => document.getElementById('toast')?.remove(), 3800);
document.querySelectorAll('[data-confirm]').forEach(f => {
  f.addEventListener('submit', e => {
    if (!confirm(f.getAttribute('data-confirm'))) e.preventDefault();
  });
});
