---
layout: default
title: Setup and Operations
---

## Setup and Operations

### Prerequisites

- .NET 10 SDK.
- A Clerk application configured for the local WebApp and API.

### Build the solution

```powershell
dotnet restore UrlShortner.sln
dotnet build UrlShortner.sln
```

### Configure Clerk

Store Clerk settings as user secrets or environment variables. Do not commit tokens, signing secrets, or production connection strings.

```powershell
dotnet user-secrets --project WebApi set "Clerk:Authority" "https://your-clerk-issuer.example.com"
dotnet user-secrets --project WebApi set "Clerk:Audience" "your-api-audience"
```

### Run locally

Start the API:

```powershell
dotnet run --project WebApi/UrlTrimmer.WebApi.csproj --urls http://localhost:5043
```

Start the WebApp in another terminal:

```powershell
dotnet run --project WebApp/UrlTrimmer.WebApp.csproj --urls http://localhost:5044
```

Open `http://localhost:5044`. The API health check is available at `http://localhost:5043/api/health`.

### Configuration

- `WebApi/appsettings.json` contains the SQLite connection string and allowed WebApp origin.
- `WebApp/appsettings.json` contains the API base URL.
- Clerk authority and audience belong in user secrets or environment variables.

### GitHub Pages publishing

The `docs` folder is configured as a GitHub Pages source. In the repository, open **Settings > Pages**, select **Deploy from a branch**, choose the default branch and `/docs`, then select **Save**. GitHub will publish the site at `https://YOUR-USER.github.io/YOUR-REPOSITORY/`.

#### Publish with a custom domain

To publish the documentation at a domain such as `docs.example.com`:

1. In your DNS provider, add a `CNAME` record for `docs` that points to `YOUR-USER.github.io`.
2. Push the repository to GitHub and configure the `/docs` folder under **Settings > Pages** as described above.
3. In **Settings > Pages > Custom domain**, enter `docs.example.com` and select **Save**.
4. Wait for GitHub to verify the DNS record. Enable **Enforce HTTPS** after the certificate becomes available.
5. Open `https://docs.example.com` to verify the documentation and sidebar links.

For an apex domain such as `example.com`, use these `A` records instead of a `CNAME` record:

```text
185.199.108.153
185.199.109.153
185.199.110.153
185.199.111.153
```

You can also commit a `docs/CNAME` file containing only the custom hostname, for example `docs.example.com`. GitHub creates or updates this file when the custom domain is saved in the Pages settings.

### Development guidelines

- Keep WebApp and WebApi boundaries intact.
- Enforce authentication and user filtering in the API.
- Validate user-provided URLs and custom codes at the API boundary.
- Keep secrets out of source control.
- Run a solution build after code changes.

[Back to documentation home](index.md)
