# Neon MCP — Local development setup

This document describes a minimal local-dev MCP (Neon) client configuration and testing tips.

1) Register the client in Neon (or your Neon-compatible auth server)
  - Client type: Public/native (no client secret) or Public client with PKCE
  - Redirect URIs: add the exact loopback URIs you plan to use, e.g. `http://127.0.0.1:33418/` and `http://127.0.0.1/`
  - Add `https://insiders.vscode.dev/redirect` and `https://vscode.dev/redirect` if using VS Code web flows
  - Allowed scopes: `openid profile email` (minimum)

2) Local config
  - See `.mcp/mcp.neon.local.yaml` for a template. Fill `client_id` with the registered value.
  - Use PKCE for the authorization code flow. Avoid storing a client secret in your repo or dev machine.

3) Security notes
  - Do not expose loopback redirect URIs via public tunnels (ngrok, Gradio `share=True`, etc.). Exposing them publicly is what triggers third-party security alerts.
  - For production use TLS-protected redirect URIs and confidential clients with properly stored secrets.

4) Quick test (manual)
  - Construct an authorization URL (replace placeholders):

```text
https://<NEON_AUTH_HOST>/authorize?response_type=code&client_id=YOUR_CLIENT_ID&redirect_uri=http://127.0.0.1:33418/&scope=openid%20profile%20email&code_challenge=CODE_CHALLENGE&code_challenge_method=S256
```

  - Open that URL in a browser. After consent the auth server will redirect to the loopback URI with `?code=...`.
  - Exchange the code for tokens at the token endpoint using the `code_verifier`.

5) If you want, I can:
  - Fill the template with values you provide and add a small local test script.
  - Attempt a dry-run against a Neon instance if you supply its auth base URL and a test client.
