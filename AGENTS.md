# Agent Instructions — UrlTrimmer

These are the ground rules for any AI coding agent (or human) working in this repository.
This file is intentionally short. Detailed, topic-specific standards live under [docs/](docs/) —
read the relevant doc(s) before making changes in that area.

## Project Snapshot

- **Solution:** `UrlShortner.sln`
- **`WebApi/`** — ASP.NET Core minimal API (`UrlTrimmer.WebApi`), .NET 10, EF Core + SQLite,
  runs on `http://localhost:5043`. Owns all data access and business rules.
- **`WebApp/`** — Blazor Web App (`UrlTrimmer.WebApp`), .NET 10, runs on `http://localhost:5044`.
  Talks to `WebApi` only through `UrlShortenerApiClient` (typed `HttpClient`). Never accesses the
  database directly.

## Documentation Map

For detailed guidance on specific topics, refer to modular documentation under `docs/` directory. ALWAYS read the relevant .md files / doc(s) BEFORE generating any code or changes.

- [docs/authentication-standards.md](docs/authentication-standards.md) — Clerk-only auth, protected routes, homepage redirect, modal sign-in/sign-up.
- [docs/ui-component-standards.md](docs/ui-component-standards.md) — NeoUI-only components, no custom components.

## Non-Negotiable Rules

1. **Read before you write.** Open the file(s) you intend to change and match existing patterns
   (naming, structure, error handling) before introducing new ones.
2. **Respect layering.** `WebApp` never references `WebApi` types directly or touches
   `UrlShortenerDbContext`. All cross-project data flows through the typed HTTP client and its own
   `WebApp/Models` contracts.
3. **Nullable and implicit usings stay enabled.** Do not add `#nullable disable` or suppress
   warnings to make code compile — fix the actual nullability issue.
4. **No secrets in source or `appsettings.json`.** Connection strings, API keys, and tokens belong
   in user secrets, environment variables, or a secret store — never committed.
5. **Keep changes minimal and scoped.** Don't refactor unrelated code, rename things, or add
   abstractions/features that weren't asked for.
6. **Build must stay green.** After any change, run a build (and tests, if present) for the
   affected project(s) and fix errors/warnings before considering the task complete.
7. **Don't fabricate tech stack details.** `Ref._materials/Tech stack.md` describes aspirational
   direction (Neon/Postgres, DAPR). The current implementation uses SQLite and EF Core — only
   adopt those parts of the aspirational stack when explicitly asked to migrate, and update the
   relevant doc under `docs/` when you do. **NeoUI (UI components) and Clerk (auth) are already
   adopted standards** — see [docs/ui-component-standards.md](docs/ui-component-standards.md) and
   [docs/authentication-standards.md](docs/authentication-standards.md).
8. **Ask before you assume.** If a request is ambiguous, or if you see a conflict between the
   instructions and the docs, flag it and ask for clarification instead of guessing.
9. **Git commit messages must be clear and descriptive.** Always prefix commit message with date YYYYMMDD# followed by a short description. Use the imperative mood, e.g. "Fix bug in URL shortening" or "Add unit tests for auth middleware". Avoid vague messages like "Update code" or "Fix stuff".

## When Instructions Conflict

If a request conflicts with these rules or the docs, flag and ASK for direction on the conflict to the user instead of silently picking one side.
