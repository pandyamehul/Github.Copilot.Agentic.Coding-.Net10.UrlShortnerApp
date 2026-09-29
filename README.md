# 🔗 UrlTrimmer

UrlTrimmer is a small URL-shortening application built with ASP.NET Core and Blazor. Authenticated users can create short links, optionally choose a custom short code, and view the links they own. Public short-link requests redirect to the original URL.

> Read the [UrlTrimmer project documentation](docs/index.md), or publish it as a website with GitHub Pages using the steps below.

## Documentation

The full project documentation is split into focused guides:

- [Project overview](docs/project-overview.md): functionality, user flow, and scope.
- [Repository overview](docs/repository-overview.md): folder structure and project responsibilities.
- [Architecture and technical overview](docs/architecture.md): components, data model, and technology stack.
- [API documentation](docs/api.md): routes, requests, authentication, validation, and responses.
- [Setup and operations](docs/operations.md): prerequisites, configuration, local execution, and publishing.

## Publish this documentation with GitHub Pages

The public documentation is in `docs/index.md`, which GitHub Pages can publish directly without a separate documentation build.

1. Push the repository to GitHub.
2. Open the repository on GitHub and select **Settings**.
3. Select **Pages** under **Code and automation**.
4. Under **Build and deployment**, choose **Deploy from a branch**.
5. Select your default branch, choose the `/docs` folder, and select **Save**.
6. Wait for the Pages deployment to finish. GitHub will display the website URL, usually `https://YOUR-USER.github.io/YOUR-REPOSITORY/`.

When `docs/index.md` changes, commit and push the change. GitHub Pages will publish the updated documentation automatically.

### Use a custom documentation URL

To use a hostname such as `docs.example.com`:

1. Add a DNS `CNAME` record for `docs` pointing to `YOUR-USER.github.io`.
2. Open the repository's **Settings > Pages** page and confirm the default branch and `/docs` folder are selected.
3. Enter `docs.example.com` under **Custom domain** and select **Save**.
4. Wait for DNS verification and the TLS certificate, then enable **Enforce HTTPS**.

For an apex domain such as `example.com`, configure GitHub Pages' four `A` records instead:

```text
185.199.108.153
185.199.109.153
185.199.110.153
185.199.111.153
```

GitHub may create a `docs/CNAME` file containing the custom hostname. Keep that file committed so the custom domain remains associated with the Pages site.

## 📁 Project Structure

```text
UrlShortner.sln
├── WebApi/    ASP.NET Core minimal API, authentication, persistence, and URL rules
├── WebApp/    Blazor Web App UI and typed HTTP client
├── data/      Reference SQL schema and sample data
└── docs/      GitHub Pages project documentation
```

## 🏗️ Architecture

The solution is split into two independently running ASP.NET Core applications:

```text
Browser
   |
   v
WebApp (Blazor Web App, :5044)
   |
   | UrlShortenerApiClient
   | Bearer/X-Clerk-Session-Token
   v
WebApi (minimal API, :5043)
   |
   | EF Core
   v
SQLite (UrlShortner.db)
```

### 🖥️ WebApp

- Uses Blazor Web App with Interactive Server rendering.
- Owns page composition, presentation models, and browser-facing behavior.
- Communicates with the API through `UrlShortenerApiClient`.
- Does not reference API internals or access the database directly.
- Uses Clerk client-side authentication to obtain the current user and session token.

### ⚙️ WebApi

- Uses ASP.NET Core minimal APIs.
- Owns URL validation, short-code generation, authorization, persistence, and redirects.
- Validates Clerk JWTs with ASP.NET Core JWT bearer authentication.
- Accepts the Clerk session token through the normal `Authorization: Bearer` header or `X-Clerk-Session-Token`.
- Derives the current user ID from the token `sub` claim and scopes authenticated URL queries to that user.

## 🧰 Technical Stack

- 🟦 .NET 10 / ASP.NET Core
- 🧩 Blazor Web App with Interactive Server components
- 🔌 ASP.NET Core Minimal APIs
- 🗃️ Entity Framework Core 10
- 💾 SQLite
- 🔐 Clerk for user authentication and JWT-based API authorization
- ✅ Nullable reference types and implicit usings enabled

## 🗂️ Data Model

The `ShortUrls` table stores:

| Column          | Description                            |
| --------------- | -------------------------------------- |
| `Id`            | Integer primary key                    |
| `code`          | Unique short code                      |
| `original_url`  | Absolute HTTP or HTTPS destination URL |
| `clerk_user_id` | Clerk user ID that owns the link       |
| `created_at`    | UTC timestamp with offset              |
| `updated_at`    | UTC timestamp with offset              |

The application deliberately does not implement click tracking or soft deletes. Deletes, if added later, should remain permanent unless the data model and requirements are explicitly changed.

The API runs `DatabaseInitializer` at startup. It creates the SQLite database when needed and performs the small compatibility upgrades required by older local database files.

## 🌐 API Endpoints

| Method | Route              | Auth      | Purpose                                            |
| ------ | ------------------ | --------- | -------------------------------------------------- |
| `GET`  | `/api/health`      | No        | Health check                                       |
| `GET`  | `/api/urls`        | Clerk JWT | List links owned by the current user               |
| `POST` | `/api/urls`        | Clerk JWT | Create a link with an autogenerated or custom code |
| `GET`  | `/api/urls/{code}` | Clerk JWT | Read an owned link                                 |
| `GET`  | `/u/{code}`        | No        | Redirect to the original URL                       |

`POST /api/urls` accepts an `originalUrl` and an optional `customCode`. URLs must be absolute HTTP or HTTPS URLs. Custom codes must be 3 to 32 characters and may contain letters, numbers, `-`, and `_`. Duplicate codes return `409 Conflict`.

## ⚙️ Configuration

### 🔧 Web-Api

`WebApi/appsettings.json` contains non-secret local defaults:

- `ConnectionStrings:UrlShortenerDb`: SQLite connection string
- `Cors:AllowedOrigin`: URL of the WebApp, normally `http://localhost:5044`

**Clerk settings must be supplied through user secrets or environment variables**:

- `Clerk:Authority`
- `Clerk:Audience` when audience validation is enabled

Example user-secrets commands:

```powershell
dotnet user-secrets --project WebApi set "Clerk:Authority" "https://your-clerk-issuer.example.com"
dotnet user-secrets --project WebApi set "Clerk:Audience" "your-api-audience"
```

Use the issuer/authority and audience values configured for the Clerk application. Never commit tokens, API keys, signing secrets, or production connection strings.

### 🖥️ Web-App

`WebApp/appsettings.json` contains the API base URL:

```json
{
  "UrlShortenerApi": {
    "BaseUrl": "http://localhost:5043/"
  }
}
```

## ▶️ Running Locally

Prerequisites:

- .NET 10 SDK
- A Clerk application configured for the local WebApp and API

Restore and build the solution:

```powershell
dotnet restore UrlShortner.sln
dotnet build UrlShortner.sln
```

Start the API in one terminal:

```powershell
dotnet run --project WebApi/UrlTrimmer.WebApi.csproj --urls http://localhost:5043
```

Start the WebApp in another terminal:

```powershell
dotnet run --project WebApp/UrlTrimmer.WebApp.csproj --urls http://localhost:5044
```

Open `http://localhost:5044`. The API health endpoint is available at `http://localhost:5043/api/health`.

## 📌 Development Guidelines

- Keep WebApp and WebApi boundaries intact.
- Put shared cross-layer data in WebApp contracts/models and the typed HTTP client; do not reference WebApi internals from WebApp.
- Keep authentication and per-user filtering enforced by the API, not only by UI behavior.
- Validate all user-provided URLs and custom codes at the API boundary.
- Keep secrets out of source control and `appsettings.json`.
- Run a solution build after changes to ensure both projects remain healthy.

See [AGENTS.md](AGENTS.md) for repository-specific coding standards and the [documentation home](docs/index.md) for the public project guides.
