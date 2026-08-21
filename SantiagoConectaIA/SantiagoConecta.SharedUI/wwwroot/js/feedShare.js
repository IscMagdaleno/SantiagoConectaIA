export async function canShare() {
  return typeof navigator !== 'undefined' && typeof navigator.share === 'function';

}

export async function share(title, text, url) {
  if (!canShare()) {
    return false;
  }

  try {
    await navigator.share({
      title: title || '',
      text: text || '',
      url: url || undefined
    });
    return true;
  } catch (err) {
    // User cancelled or share failed — not an error for the UI.
    if (err && err.name === 'AbortError') {
      return false;
    }
    console.warn('Web Share failed:', err);
    return false;
  }
}

export function openUrl(url) {
  if (!url) {
    return;
  }
  window.open(url, '_blank', 'noopener,noreferrer');
}
