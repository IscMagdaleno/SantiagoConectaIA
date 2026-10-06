/**
 * Capturador de posts para Instagram/Facebook (1080x1360) utilizando html2canvas
 */
window.postCapture = {
    captureElementAsBase64: async function (elementId) {
        var element = document.getElementById(elementId);
        if (!element) {
            console.error("Elemento no encontrado para capturar:", elementId);
            return null;
        }

        try {
            // Aseguramos que todas las imágenes internas hayan cargado
            var images = element.querySelectorAll('img');
            var imgPromises = Array.from(images).map(img => {
                if (img.complete) return Promise.resolve();
                return new Promise(resolve => {
                    img.onload = resolve;
                    img.onerror = resolve;
                });
            });
            await Promise.all(imgPromises);

            // Ajuste temporal para captura de alta resolución 1080x1360
            var canvas = await html2canvas(element, {
                useCORS: true,
                allowTaint: false,
                scale: 1, // La tarjeta ya mide exactamente 1080px x 1360px
                width: 1080,
                height: 1360,
                backgroundColor: '#ffffff',
                logging: false,
                imageTimeout: 15000
            });

            return canvas.toDataURL('image/png', 0.95);
        } catch (error) {
            console.error("Error al capturar el elemento:", error);
            throw error;
        }
    }
};
