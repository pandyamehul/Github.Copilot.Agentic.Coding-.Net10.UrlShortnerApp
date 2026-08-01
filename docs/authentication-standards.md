# Authentication Standards

## Provider

- **Clerk** is the sole authentication provider. No other auth methods, libraries, or custom solutions are permitted anywhere in this app.

## Protected Routes

- Every page requires an authenticated user.
- Enforce auth checks via routing/middleware, not just client-side UI checks.
- Unauthenticated access attempts must redirect into the Clerk sign-in flow.

## Homepage Redirect

- Logged-in users hitting `/` must be redirected to `/dashboard`.
- Enforce this at the middleware/routing level.

## Sign-In / Sign-Up UX

- Sign-in and sign-up must always launch as **modals** (e.g. Clerk's `routing="virtual"` or modal mode) — never as separate full-page routes.

## Route Protection & Redirects

- Apply auth enforcement centrally (e.g. a shared layout/base component or routing middleware), not per-page, so no new page can be added unprotected by accident.
- Unauthenticated users hitting any protected route must be redirected into the Clerk sign-in modal/flow, then returned to their originally requested route after signing in.
- Authenticated users hitting `/` must be redirected to `/dashboard` before any homepage content renders.
- Redirect checks must run server-side/on render, not only after client-side JS hydration, to avoid a flash of protected content.

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
- [ ] Homepage (`/`) redirect-to-`/dashboard` logic is verified for logged-in users.
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
- **User stays on `/` after login instead of `/dashboard`:** verify the redirect check runs after Clerk session is fully loaded, not before hydration.
- **"Flash" of protected content before redirect:** move the auth check earlier (server-side/layout level) instead of relying on a client-only effect.
- **401/403 calling `WebApi` from `WebApp`:** confirm the Clerk session token is being forwarded correctly via `UrlShortenerApiClient`.

## Checklist

- [ ] Only Clerk used for auth
- [ ] All pages require authentication
- [ ] `/` redirects logged-in users to `/dashboard`
- [ ] Sign-in/sign-up always open as modals
