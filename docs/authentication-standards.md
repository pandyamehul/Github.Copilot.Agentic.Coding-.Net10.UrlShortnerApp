# Authentication Standards

## Provider

- **Clerk** is the sole authentication provider. No other auth methods, libraries, or custom solutions are permitted anywhere in this app.

## Protected Routes

- Every page that reads or writes user-specific data requires an authenticated user.
- Enforce auth checks via routing/middleware, not just client-side UI checks.
- Unauthenticated access attempts must redirect into the Clerk sign-in flow.
- **Exception:** `/dashboard` (`Dashboard.razor`) is a public, static feature-overview page. It shows no URL data (no per-user or aggregate stats) and does not call the API, so it does not require sign-in.

## Homepage Behavior

- `/` (`Home.razor`) is auth-aware in place, not a redirect target: it listens for Clerk auth state via `urlTrimmerClerk.registerAuthStateListener`/`getAuthState` JS interop and `[JSInvokable] OnAuthStateChanged`.
- Signed-out users see marketing/feature content and sign-in/sign-up CTAs (via `NavMenu`).
- Signed-in users see the create-short-URL form and their saved links list, rendered inline on `/` — there is no forced redirect to a separate `/dashboard` route.

## Sign-In / Sign-Up UX

- Sign-in and sign-up must always launch as **modals** (e.g. Clerk's `routing="virtual"` or modal mode) — never as separate full-page routes.

## Per-User Data Scoping

- The signed-in Clerk user's id (`clerk.user.id`, exposed as `userId` from `urlTrimmerClerk.getAuthState()`) is the only identifier used to scope a user's short URLs.
- `WebApp` must pass this `ClerkUserId` on every create request (`POST /api/urls`) and every list request (`GET /api/urls?clerkUserId=...`); the API rejects creates with a missing/blank `ClerkUserId`.
- `WebApi` never invents or defaults a user id (no `"anonymous"` placeholder) — every stored `ShortUrl.ClerkUserId` must be the real signed-in Clerk user id supplied by the caller.

## Route Protection & Redirects

- Apply auth enforcement centrally (e.g. a shared layout/base component or routing middleware), not per-page, so no new page can be added unprotected by accident.
- Unauthenticated users hitting any protected route must be redirected into the Clerk sign-in modal/flow, then returned to their originally requested route after signing in.
- `/` itself is not a protected route — it renders different content in place based on Clerk auth state (see Homepage Behavior above) instead of redirecting.
- Redirect checks (for actual protected routes) must run server-side/on render, not only after client-side JS hydration, to avoid a flash of protected content.

## Do and DO NOT

- **Do** rely exclusively on Clerk's SDK/components for sign-in, sign-up, session, and user state.
- **Do** centralize the "is authenticated" check in one reusable place and reuse it across all pages.
- **Do** preserve the originally requested URL when redirecting to sign-in, and return the user there post-login.
- **DO NOT** implement custom cookies, JWT handling, password storage, or any parallel auth mechanism alongside Clerk.
- **DO NOT** gate pages with client-side-only checks (e.g. hiding UI with CSS) without a real server/routing-level guard.
- **DO NOT** use full-page navigation for sign-in/sign-up — always use Clerk's modal mode.
- **DO NOT** hardcode Clerk keys/secrets in source or `appsettings.json`.

## Clerk Integration Checklist

- [ ] Clerk publishable/secret keys are loaded from user secrets/environment variables, never committed.
- [ ] Clerk provider/middleware is registered once at the app root and wraps all routes.
- [ ] Every page/route resolves through the shared auth guard (no page bypasses it).
- [ ] Sign-in and sign-up components are configured with modal routing, not path-based routing.
- [ ] `/dashboard` stays a public, data-free feature page; user-specific data only ever renders on `/` for signed-in users.
- [ ] Every create/list call to `WebApi` passes the real signed-in Clerk `userId`, never a placeholder.
- [ ] Sign-out clears Clerk session and redirects to a safe, non-protected landing state.
- [ ] Environment-specific Clerk instances (dev/prod) are configured correctly per environment.

## Security Best Practices

- Treat Clerk session/user data as the single source of truth for identity — don't cache stale auth state client-side.
- Validate the Clerk session on every server-rendered request; don't trust a client-side "logged in" flag alone.
- Keep Clerk SDK/packages up to date to pick up security patches.
- Scope API calls from `WebApp` to `WebApi` using the authenticated user's context; never allow anonymous access to data endpoints.
- Log auth failures/redirects for monitoring, but never log tokens, session IDs, or secrets.

## Troubleshooting

- **Redirect loop between sign-in and a protected page:** confirm the post-sign-in redirect URL matches an allowed Clerk redirect and isn't itself behind the same guard incorrectly.
- **Modal doesn't open, page navigates instead:** check the sign-in/sign-up component is configured for modal/virtual routing, not default path routing.
- **Saved links list is empty/wrong after signing in:** verify `clerk.user.id` is populated before the list/create call fires and that it's passed as `clerkUserId` on both `GetUrlsAsync` and `CreateAsync`.
- **"Flash" of protected content before redirect:** move the auth check earlier (server-side/layout level) instead of relying on a client-only effect.
- **401/403 calling `WebApi` from `WebApp`:** confirm the Clerk session token is being forwarded correctly via `UrlShortenerApiClient`.

## Checklist

- [ ] Only Clerk used for auth
- [ ] All pages require authentication
- [ ] `/` renders auth-aware content in place (no forced `/dashboard` redirect)
- [ ] Sign-in/sign-up always open as modals
