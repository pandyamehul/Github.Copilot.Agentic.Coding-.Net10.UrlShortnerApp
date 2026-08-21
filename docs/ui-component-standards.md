# UI Component Standards

## Provider

- **NeoUI has been removed.** This app uses custom Blazor markup (plain HTML elements, Blazor form components like `EditForm`/`InputText`) styled with hand-written CSS in `WebApp/wwwroot/app.css`.
- Do not reintroduce NeoUI or any other third-party component library without explicit sign-off.

## Usage Rules

- Compose pages from plain HTML elements and Blazor's built-in form components (`EditForm`, `InputText`, `DataAnnotationsValidator`, `ValidationSummary`, etc.).
- Keep shared visual patterns (`.hero`, `.hero-single`, `.results-grid`, `.card`, `.form-card`, `.url-list`, etc.) defined once in `app.css` and reused across pages rather than duplicated inline.
- Prefer extending existing CSS classes over introducing new one-off inline styles; keep `app.css` valid, non-nested plain CSS.

## Checklist

- [ ] No NeoUI or other third-party component library references introduced
- [ ] New markup reuses existing shared CSS classes where applicable
- [ ] `app.css` stays valid plain CSS (no nested/malformed rules)
