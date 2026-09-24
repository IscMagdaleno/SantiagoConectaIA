(function () {
    window.__pwaDeferredPrompt = window.__pwaDeferredPrompt || null;

    window.addEventListener('beforeinstallprompt', function (event) {
        event.preventDefault();
        window.__pwaDeferredPrompt = event;
        window.dispatchEvent(new CustomEvent('pwa-install-available'));
    });

    window.addEventListener('appinstalled', function () {
        window.__pwaDeferredPrompt = null;
        window.dispatchEvent(new CustomEvent('pwa-app-installed'));
    });

    function isStandalone() {
        return window.matchMedia('(display-mode: standalone)').matches
            || window.matchMedia('(display-mode: fullscreen)').matches
            || window.navigator.standalone === true;
    }

    function isIosDevice() {
        var ua = window.navigator.userAgent || '';
        var iOS = /iPad|iPhone|iPod/.test(ua);
        var iPadOs = window.navigator.platform === 'MacIntel' && window.navigator.maxTouchPoints > 1;
        return iOS || iPadOs;
    }

    var subscriptions = [];

    window.pwaInstall = {
        getStatus: function () {
            return {
                isInstalled: isStandalone(),
                canInstall: !!window.__pwaDeferredPrompt,
                isIos: isIosDevice()
            };
        },
        promptInstall: async function () {
            var promptEvent = window.__pwaDeferredPrompt;
            if (!promptEvent) {
                return 'unavailable';
            }

            promptEvent.prompt();
            var choice = await promptEvent.userChoice;
            window.__pwaDeferredPrompt = null;
            return choice && choice.outcome ? choice.outcome : 'dismissed';
        },
        subscribe: function (dotNetRef) {
            var notifyAvailable = function () {
                dotNetRef.invokeMethodAsync('OnInstallAvailable');
            };
            var notifyInstalled = function () {
                dotNetRef.invokeMethodAsync('OnAppInstalled');
            };

            window.addEventListener('pwa-install-available', notifyAvailable);
            window.addEventListener('pwa-app-installed', notifyInstalled);

            var id = subscriptions.length;
            subscriptions.push({ notifyAvailable: notifyAvailable, notifyInstalled: notifyInstalled });

            if (window.__pwaDeferredPrompt) {
                notifyAvailable();
            }
            if (isStandalone()) {
                notifyInstalled();
            }

            return id;
        },
        unsubscribe: function (id) {
            var subscription = subscriptions[id];
            if (!subscription) {
                return;
            }

            window.removeEventListener('pwa-install-available', subscription.notifyAvailable);
            window.removeEventListener('pwa-app-installed', subscription.notifyInstalled);
            subscriptions[id] = null;
        }
    };
})();
