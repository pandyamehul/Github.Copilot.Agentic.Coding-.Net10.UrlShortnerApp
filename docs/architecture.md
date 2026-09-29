---
layout: default
title: Architecture and Technical Overview
---

## Architecture and Technical Overview

### System architecture

```text
Browser
  |
  v
WebApp - Blazor Web App (localhost:5044)
  |
  | UrlShortenerApiClient
  | Authorization: Bearer <Clerk session token>
  v
WebApi - ASP.NET Core Minimal API (localhost:5043)
  |
  | Entity Framework Core
  v
SQLite database
```

### WebApp

- .NET 10 Blazor Web App.
- Interactive Server rendering.
- Owns UI composition and presentation models.
- Uses `UrlShortenerApiClient` for API calls.
- Uses Clerk client-side authentication to obtain the current session.
- Does not access the database directly.

### WebApi

- .NET 10 ASP.NET Core Minimal API.
- Uses Entity Framework Core 10 and SQLite.
- Validates Clerk JWTs with JWT bearer authentication.
- Reads the authenticated user from the `sub` claim.
- Enforces per-user filtering for private URL operations.
- Initializes and upgrades the local database at startup.

### Data model

| Column        | Description                            |
| ------------- | -------------------------------------- |
| `Id`          | Integer primary key                    |
| `Code`        | Unique short code                      |
| `OriginalUrl` | Absolute HTTP or HTTPS destination URL |
| `ClerkUserId` | Clerk user ID that owns the link       |
| `CreatedAt`   | UTC creation timestamp                 |
| `UpdatedAt`   | UTC update timestamp                   |

### Technical stack

- .NET 10 and ASP.NET Core
- Blazor Web App
- ASP.NET Core Minimal APIs
- Entity Framework Core 10
- SQLite
- Clerk authentication and JWT authorization

[Back to documentation home](index.md)
