# UI Component Standards

## Provider

- **NeoUI** is the sole UI component library for all Blazor UI in this app.
- **DO NOT** create custom components for anything NeoUI already provides (buttons, inputs, forms, dialogs, layout, navigation, etc.).
- Only build a custom component when no equivalent NeoUI component exists, and confirm this before doing so.

## Usage Rules

- Always compose pages/features from NeoUI components rather than raw HTML + CSS.
- Follow NeoUI's documented markup/parameters instead of overriding its styles or behavior with custom CSS/JS.
- Keep styling consistent by using NeoUI's theming/tokens instead of ad-hoc inline styles.

## Checklist

- [ ] No custom component duplicates existing NeoUI functionality
- [ ] Page markup uses NeoUI components, not raw HTML equivalents
- [ ] Styling relies on NeoUI theming, not one-off custom CSS
