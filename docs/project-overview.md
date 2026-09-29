---
layout: default
title: Project Overview
---

## Project Overview

UrlTrimmer is a full-stack URL-shortening application built with .NET 10. Authenticated users can create compact, shareable links, choose memorable custom codes, and manage the links they own.

### Core functionality

- Accept absolute HTTP and HTTPS URLs.
- Generate a short code automatically.
- Accept an optional custom code.
- Prevent duplicate short codes.
- Redirect visitors from `/u/{code}` to the original URL.
- Associate links with the authenticated Clerk user.
- Display each user's saved links in the Blazor interface.

### Typical user flow

1. A visitor opens the WebApp.
2. The visitor signs in through Clerk.
3. The user submits a long URL and optionally enters a custom code.
4. The WebApp sends the request to the WebApi with the Clerk session token.
5. The API validates the URL, authorizes the user, stores the link, and returns the new short URL.
6. Anyone with the public short URL can follow the redirect.

### Scope

UrlTrimmer currently focuses on link creation, ownership, retrieval, and redirection. It does not implement click analytics, link expiration, soft deletes, or team sharing.

[Back to documentation home](index.md)
