window.santiagoAnalytics = window.santiagoAnalytics || {
    getOrCreateVisitorId: function () {
        try {
            var key = 'scia_visitor_id';
            var id = localStorage.getItem(key);
            if (!id) {
                if (window.crypto && window.crypto.randomUUID) {
                    id = window.crypto.randomUUID();
                } else {
                    id = 'v-' + Date.now().toString(36) + '-' + Math.random().toString(36).slice(2, 10);
                }
                localStorage.setItem(key, id);
            }
            return id;
        } catch (e) {
            return 'anon-' + Date.now().toString(36);
        }
    }
};
