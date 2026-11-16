/**
 * Descarga de archivos generados (por ejemplo memes con IA) a partir de un Data URL.
 */
window.descargas = {
    descargarDataUrl: function (dataUrl, nombreBase) {
        var coincidencia = /^data:([^;,]+)[;,]/.exec(dataUrl || '');
        var mime = coincidencia ? coincidencia[1].toLowerCase() : 'image/png';
        var extension = mime === 'image/jpeg' || mime === 'image/jpg' ? 'jpg'
            : mime === 'image/webp' ? 'webp'
            : 'png';

        var enlace = document.createElement('a');
        enlace.href = dataUrl;
        enlace.download = nombreBase + '.' + extension;
        document.body.appendChild(enlace);
        enlace.click();
        document.body.removeChild(enlace);
    }
};
