---
layout: default
title: Repository Overview
---

## Repository Overview

```text
UrlShortner.sln
├── WebApi/
│   ├── Contracts/    API request and response contracts
│   ├── Data/         EF Core DbContext and database initialization
│   ├── Models/       Persistence entities
│   ├── Services/     Short-code generation
│   └── EndpointMappings.cs
├── WebApp/
│   ├── Components/   Blazor layouts, auth, and pages
│   ├── Models/       UI and API client models
│   ├── Services/     Typed HTTP client for WebApi
│   └── wwwroot/      CSS and Clerk browser integration
├── data/             Reference SQL schema and sample data
├── docs/             GitHub Pages documentation
└── README.md         Developer quick start and repository rules
```

### Project responsibilities

#### WebApi

Owns persistence, URL validation, short-code generation, authentication, authorization, and redirects. It is the only project that accesses `UrlShortenerDbContext`.

#### WebApp

Owns page composition, presentation models, forms, navigation, and browser-facing behavior. It communicates with the API through `UrlShortenerApiClient`.

#### data

Contains reference SQL files for understanding the data shape and sample records. Runtime development uses SQLite initialized by the API.

#### Layering rule

Keep the WebApp and WebApi boundaries intact. The WebApp must not reference WebApi implementation types or access the database directly. Cross-project data must travel through HTTP contracts and the typed API client.

[Back to documentation home](index.md)
