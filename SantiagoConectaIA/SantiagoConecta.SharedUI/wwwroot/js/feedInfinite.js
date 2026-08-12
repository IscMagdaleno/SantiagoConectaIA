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

export function disconnect(observer) {
  if (observer) {
    observer.disconnect();
  }
}
