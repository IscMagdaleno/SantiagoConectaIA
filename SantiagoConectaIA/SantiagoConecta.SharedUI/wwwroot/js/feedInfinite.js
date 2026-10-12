export function observe(element, dotNetHelper) {
  if (!element || !dotNetHelper) {
    return null;
  }

  const observer = new IntersectionObserver(
    (entries) => {
      for (const entry of entries) {
        if (entry.isIntersecting) {
          dotNetHelper.invokeMethodAsync('OnFeedSentinelVisible');
        }
      }
    },
    { root: null, rootMargin: '240px', threshold: 0 }
  );

  observer.observe(element);
  return observer;
}

// IntersectionObserver solo avisa cuando cambia la visibilidad. Si despues de cargar
// una pagina el centinela sigue a la vista (contenido corto, pantalla alta), no habria
// otra notificacion. Volver a observar fuerza una nueva lectura del estado actual.
export function refresh(observer, element) {
  if (!observer || !element) {
    return;
  }

  observer.unobserve(element);
  observer.observe(element);
}

export function disconnect(observer) {
  if (observer) {
    observer.disconnect();
  }
}
