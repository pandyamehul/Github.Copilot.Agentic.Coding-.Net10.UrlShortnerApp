window.urlTrimmerClerk = (() => {
    let clerk = null;
    let initializePromise = null;
    const authStateListeners = new Set();

    function loadScript(src, attributes = {}) {
        return new Promise((resolve, reject) => {
            const existingScript = Array.from(document.scripts).find(script => script.src === src);
            if (existingScript) {
                if (existingScript.dataset.loaded === "true") {
                    resolve();
                    return;
                }

                existingScript.addEventListener("load", () => resolve(), { once: true });
                existingScript.addEventListener("error", () => reject(new Error(`Failed to load ${src}`)), { once: true });
                return;
            }

            const script = document.createElement("script");
            script.src = src;
            script.async = true;
            script.crossOrigin = "anonymous";

            for (const [key, value] of Object.entries(attributes)) {
                if (value != null) {
                    script.setAttribute(key, value);
                }
            }

            script.addEventListener("load", () => {
                script.dataset.loaded = "true";
                resolve();
            }, { once: true });

            script.addEventListener("error", () => reject(new Error(`Failed to load ${src}`)), { once: true });
            document.head.appendChild(script);
        });
    }

    async function initialize(publishableKey) {
        if (!publishableKey) {
            throw new Error("Missing Clerk publishable key.");
        }

        if (initializePromise) {
            return initializePromise;
        }

        initializePromise = (async () => {
            const clerkDomain = atob(publishableKey.split("_")[2]).slice(0, -1);

            await loadScript(`https://${clerkDomain}/npm/@clerk/ui@1/dist/ui.browser.js`);
            await loadScript(`https://${clerkDomain}/npm/@clerk/clerk-js@6/dist/clerk.browser.js`, {
                "data-clerk-publishable-key": publishableKey
            });

            clerk = window.Clerk;

            if (!clerk) {
                throw new Error("Clerk did not initialize on window.");
            }

            await clerk.load({
                ui: { ClerkUI: window.__internal_ClerkUICtor }
            });

            if (typeof clerk.addListener === "function") {
                clerk.addListener(() => {
                    void notifyAuthStateListeners();
                });
            }

            return Boolean(clerk.isSignedIn);
        })();

        return initializePromise;
    }

    async function ensureClerk() {
        if (clerk) {
            return clerk;
        }

        if (!initializePromise) {
            throw new Error("Clerk has not been initialized.");
        }

        await initializePromise;
        return clerk;
    }

    function getDisplayName() {
        const user = clerk?.user;

        if (!user) {
            return "";
        }

        const fullName = [user.firstName, user.lastName].filter(Boolean).join(" ").trim();
        if (fullName) {
            return fullName;
        }

        return user.username ?? user.primaryEmailAddress?.emailAddress ?? "";
    }

    function getAuthState() {
        return {
            isSignedIn: Boolean(clerk?.isSignedIn),
            displayName: getDisplayName()
        };
    }

    async function notifyAuthStateListeners() {
        const currentClerk = await ensureClerk();
        const authState = getAuthState();

        await Promise.all(
            Array.from(authStateListeners).map(listener =>
                listener.invokeMethodAsync("OnAuthStateChanged", authState).catch(() => {
                    authStateListeners.delete(listener);
                })
            )
        );

        return currentClerk;
    }

    return {
        initialize,
        isSignedIn: () => Boolean(clerk?.isSignedIn),
        getAuthState,
        registerAuthStateListener: listener => {
            if (listener) {
                authStateListeners.add(listener);
            }
        },
        unregisterAuthStateListener: listener => {
            if (listener) {
                authStateListeners.delete(listener);
            }
        },
        openSignIn: async () => {
            const currentClerk = await ensureClerk();
            await currentClerk.openSignIn();
            await notifyAuthStateListeners();
        },
        openSignUp: async () => {
            const currentClerk = await ensureClerk();
            await currentClerk.openSignUp();
            await notifyAuthStateListeners();
        },
        signOut: async () => {
            const currentClerk = await ensureClerk();
            await currentClerk.signOut({ redirectUrl: "/" });
            await notifyAuthStateListeners();
        }
    };
})();

const clerkPublishableKey = document.currentScript?.dataset?.clerkPublishableKey;

if (clerkPublishableKey) {
    window.urlTrimmerClerk.initialize(clerkPublishableKey).catch(error => console.error(error));
}