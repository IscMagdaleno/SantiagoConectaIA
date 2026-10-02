// Utilidades de interfaz compartidas por el sitio público (importado por SitioInterop).

export function quitarSeoPorDefecto() {
  document.querySelectorAll('[data-seo-default]').forEach((el) => el.remove());
}

// Espera a que la sección exista (p. ej. tras navegar a la portada) y desplaza
// compensando el header fijo.
export function irASeccion(id, desplazamiento) {
  let intentos = 0;
  const timer = setInterval(() => {
    const el = document.getElementById(id);
    if (el || ++intentos > 50) {
      clearInterval(timer);
      if (el) {
        const top = el.getBoundingClientRect().top + window.scrollY - (desplazamiento || 0);
        window.scrollTo({ top, behavior: 'smooth' });
      }
    }
  }, 100);
}

export function bloquearScroll(bloquear) {
  document.body.style.overflow = bloquear ? 'hidden' : '';
}

// ───────────────────────── Partículas de la portada ─────────────────────────
let limpiarParticulas = null;

// Puntos que flotan, se conectan entre sí y huyen del cursor (o del dedo).
// Un "glow" luminoso sigue al puntero. Se pausa fuera de pantalla y con
// prefers-reduced-motion se dibuja un solo cuadro estático.
export function iniciarParticulas(hero, canvas, glow) {
  detenerParticulas();
  if (!hero || !canvas) return;

  const ctx = canvas.getContext('2d');
  const reducirMovimiento = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  const colores = ['255,255,255', '255,255,255', '255,214,224', '255,183,162'];
  const puntero = { x: -9999, y: -9999 };
  const radioReaccion = 170;
  const distanciaLinea = 115;

  let ancho = 0;
  let alto = 0;
  let particulas = [];
  let animacion = 0;
  let visible = true;

  function crearParticula() {
    return {
      x: Math.random() * ancho,
      y: Math.random() * alto,
      vx: (Math.random() - 0.5) * 0.8,
      vy: (Math.random() - 0.5) * 0.8,
      r: Math.random() * 2.4 + 1,
      color: colores[Math.floor(Math.random() * colores.length)],
      brillo: Math.random() * Math.PI * 2,
    };
  }

  function ajustar() {
    const dpr = Math.min(window.devicePixelRatio || 1, 2);
    ancho = hero.clientWidth;
    alto = hero.clientHeight;
    canvas.width = ancho * dpr;
    canvas.height = alto * dpr;
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

    const cantidad = Math.max(28, Math.min(95, Math.round((ancho * alto) / 13000)));
    while (particulas.length < cantidad) particulas.push(crearParticula());
    particulas.length = cantidad;
    particulas.forEach((p) => {
      p.x = Math.min(p.x, ancho);
      p.y = Math.min(p.y, alto);
    });
  }

  function actualizar(p) {
    const dx = p.x - puntero.x;
    const dy = p.y - puntero.y;
    const distancia = Math.hypot(dx, dy);

    if (distancia < radioReaccion && distancia > 0.01) {
      const fuerza = (radioReaccion - distancia) / radioReaccion;
      p.vx += (dx / distancia) * fuerza * 0.9;
      p.vy += (dy / distancia) * fuerza * 0.9;
    }

    p.vx = (p.vx + (Math.random() - 0.5) * 0.08) * 0.97;
    p.vy = (p.vy + (Math.random() - 0.5) * 0.08) * 0.97;
    p.x += p.vx;
    p.y += p.vy;
    p.brillo += 0.04;

    if (p.x < 0 || p.x > ancho) { p.vx *= -1; p.x = Math.max(0, Math.min(ancho, p.x)); }
    if (p.y < 0 || p.y > alto) { p.vy *= -1; p.y = Math.max(0, Math.min(alto, p.y)); }
  }

  function dibujar(mover) {
    ctx.clearRect(0, 0, ancho, alto);

    for (let i = 0; i < particulas.length; i++) {
      const a = particulas[i];
      if (mover) actualizar(a);

      for (let j = i + 1; j < particulas.length; j++) {
        const b = particulas[j];
        const d = Math.hypot(a.x - b.x, a.y - b.y);
        if (d < distanciaLinea) {
          ctx.beginPath();
          ctx.strokeStyle = `rgba(255,255,255,${0.32 * (1 - d / distanciaLinea)})`;
          ctx.lineWidth = 1;
          ctx.moveTo(a.x, a.y);
          ctx.lineTo(b.x, b.y);
          ctx.stroke();
        }
      }

      // Línea luminosa entre el puntero y las partículas cercanas
      const dp = Math.hypot(a.x - puntero.x, a.y - puntero.y);
      if (dp < radioReaccion * 1.15) {
        ctx.beginPath();
        ctx.strokeStyle = `rgba(255,214,224,${0.55 * (1 - dp / (radioReaccion * 1.15))})`;
        ctx.lineWidth = 1.2;
        ctx.moveTo(a.x, a.y);
        ctx.lineTo(puntero.x, puntero.y);
        ctx.stroke();
      }

      const intensidad = 0.65 + Math.sin(a.brillo) * 0.25;
      ctx.beginPath();
      ctx.arc(a.x, a.y, a.r, 0, Math.PI * 2);
      ctx.fillStyle = `rgba(${a.color},${intensidad})`;
      ctx.shadowColor = `rgba(${a.color},0.9)`;
      ctx.shadowBlur = 8;
      ctx.fill();
      ctx.shadowBlur = 0;
    }
  }

  function ciclo() {
    if (visible && !document.hidden) dibujar(true);
    animacion = requestAnimationFrame(ciclo);
  }

  function alMoverse(e) {
    const rect = hero.getBoundingClientRect();
    puntero.x = e.clientX - rect.left;
    puntero.y = e.clientY - rect.top;
    if (glow) {
      glow.style.left = `${puntero.x}px`;
      glow.style.top = `${puntero.y}px`;
      glow.style.opacity = '1';
    }
  }

  function alSalir() {
    puntero.x = puntero.y = -9999;
    if (glow) glow.style.opacity = '0';
  }

  const observadorTamano = new ResizeObserver(ajustar);
  const observadorVisible = new IntersectionObserver((entradas) => {
    visible = entradas[0].isIntersecting;
  });

  ajustar();
  observadorTamano.observe(hero);
  observadorVisible.observe(hero);
  hero.addEventListener('pointermove', alMoverse);
  hero.addEventListener('pointerleave', alSalir);
  hero.addEventListener('pointercancel', alSalir);

  if (reducirMovimiento) {
    dibujar(false);
  } else {
    animacion = requestAnimationFrame(ciclo);
  }

  limpiarParticulas = () => {
    cancelAnimationFrame(animacion);
    observadorTamano.disconnect();
    observadorVisible.disconnect();
    hero.removeEventListener('pointermove', alMoverse);
    hero.removeEventListener('pointerleave', alSalir);
    hero.removeEventListener('pointercancel', alSalir);
  };
}

export function detenerParticulas() {
  if (limpiarParticulas) {
    limpiarParticulas();
    limpiarParticulas = null;
  }
}

// ───────────────────────── Aparición al hacer scroll ─────────────────────────
let limpiarRevelado = null;

// Los elementos con [data-revelar] "caen" en cascada cuando entran a la pantalla.
// El estado oculto lo agrega este script (no el HTML), así que sin JS todo se ve.
export function iniciarRevelado(ancla) {
  detenerRevelado();
  const raiz = ancla && ancla.parentElement;
  if (!raiz || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

  const observador = new IntersectionObserver(
    (entradas) => {
      entradas.forEach((entrada) => {
        if (!entrada.isIntersecting) return;
        entrada.target.classList.remove('rv-pendiente');
        entrada.target.classList.add('rv-visible');
        observador.unobserve(entrada.target);
      });
    },
    { threshold: 0.12, rootMargin: '0px 0px -6% 0px' }
  );

  const preparar = (el) => {
    if (el.dataset.rvListo) return;
    el.dataset.rvListo = '1';
    el.classList.add('rv-pendiente');
    observador.observe(el);
  };

  const escanear = (nodo) => {
    if (nodo.nodeType !== 1) return;
    if (nodo.matches('[data-revelar]')) preparar(nodo);
    nodo.querySelectorAll('[data-revelar]').forEach(preparar);
  };

  escanear(raiz);
  // Las tarjetas de noticias y eventos aparecen cuando terminan de cargar los datos
  const mutaciones = new MutationObserver((lista) =>
    lista.forEach((m) => m.addedNodes.forEach(escanear))
  );
  mutaciones.observe(raiz, { childList: true, subtree: true });

  limpiarRevelado = () => {
    observador.disconnect();
    mutaciones.disconnect();
  };
}

export function detenerRevelado() {
  if (limpiarRevelado) {
    limpiarRevelado();
    limpiarRevelado = null;
  }
}
