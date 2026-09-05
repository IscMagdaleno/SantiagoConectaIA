window.socialAuth = {
    signInWithGoogle: function (clientId) {
        return new Promise((resolve, reject) => {
            if (!clientId) {
                return reject("El identificador Client ID de Google no está configurado.");
            }

            const handleClient = () => {
                try {
                    const client = google.accounts.oauth2.initTokenClient({
                        client_id: clientId,
                        scope: "email profile openid",
                        callback: async (tokenResponse) => {
                            if (tokenResponse && tokenResponse.access_token) {
                                try {
                                    const res = await fetch("https://www.googleapis.com/oauth2/v3/userinfo", {
                                        headers: { Authorization: "Bearer " + tokenResponse.access_token }
                                    });
                                    const profile = await res.json();
                                    resolve(JSON.stringify({
                                        token: tokenResponse.access_token,
                                        id: profile.sub || "",
                                        email: profile.email || "",
                                        name: profile.name || "",
                                        picture: profile.picture || ""
                                    }));
                                } catch (e) {
                                    resolve(JSON.stringify({
                                        token: tokenResponse.access_token,
                                        id: "",
                                        email: "",
                                        name: "",
                                        picture: ""
                                    }));
                                }
                            } else {
                                reject(tokenResponse ? tokenResponse.error : "Error desconocido al autenticar con Google.");
                            }
                        }
                    });
                    client.requestAccessToken();
                } catch (err) {
                    reject(err.toString());
                }
            };

            if (typeof google === "undefined" || !google.accounts) {
                const script = document.createElement("script");
                script.id = "google-gsi-script";
                script.src = "https://accounts.google.com/gsi/client";
                script.async = true;
                script.onload = handleClient;
                script.onerror = () => reject("No se pudo cargar el SDK de Google.");
                document.head.appendChild(script);
            } else {
                handleClient();
            }
        });
    }
};
