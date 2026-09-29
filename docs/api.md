---
layout: default
title: API Documentation
---

## API Documentation

The API runs locally at `http://localhost:5043`.

## Endpoints

| Method | Route              | Authentication | Purpose                                |
| ------ | ------------------ | -------------- | -------------------------------------- |
| `GET`  | `/api/health`      | Public         | Returns API health status.             |
| `GET`  | `/api/urls`        | Clerk JWT      | Lists links owned by the current user. |
| `POST` | `/api/urls`        | Clerk JWT      | Creates a short link.                  |
| `GET`  | `/api/urls/{code}` | Clerk JWT      | Reads an owned link.                   |
| `GET`  | `/u/{code}`        | Public         | Redirects to the original URL.         |

## Create a short link

`POST /api/urls` accepts JSON with an `originalUrl` and optional `customCode`.

```json
{
  "originalUrl": "https://example.com/a/long/path",
  "customCode": "example-link"
}
```

Rules:

- `originalUrl` must be an absolute HTTP or HTTPS URL.
- `customCode` is optional.
- A custom code must be 3 to 32 characters.
- A custom code may contain letters, numbers, `-`, and `_`.
- Duplicate codes return `409 Conflict`.

A successful request returns `201 Created` with the created short-link response.

## Authentication

Private endpoints require a Clerk JWT. The API accepts the token through the standard `Authorization: Bearer <token>` header. It also supports `X-Clerk-Session-Token`, which the API converts to a bearer token for validation.

The API scopes list and individual-link operations to the authenticated user's `sub` claim.

## Responses

- `200 OK`: successful read or health check.
- `201 Created`: short link created.
- `401 Unauthorized`: missing or invalid authentication.
- `404 Not Found`: code does not exist or is not owned by the current user.
- `409 Conflict`: requested short code already exists.
- `422 Unprocessable Entity`: URL or custom-code validation failed.

[Back to documentation home](index.md)
