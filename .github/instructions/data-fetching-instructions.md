---
description: Instructions for data fetching in the application
---
# Data Fetching Guidelines

## Principles

### 1. Use Server Components for data fetching

- Prefer ALWAYS server components for fetching data to reduce client-side load and improve performance.
- Server components can directly access server-side resources, such as databases and APIs, without exposing sensitive information to the client.
- Use client components only when you need interactivity or access to client-specific APIs, such as the browser's `localStorage` or `navigator` object.
- NEVER use client components for data fetching unless absolutely necessary.

### 2. Data Fetching Methods

- ALWAYS use helper functions for data fetching in /data directory to encapsulate and reuse logic efficiently.
- ALWAYS use `fetch` or a similar data-fetching library to retrieve data from APIs.
- NEVER fetch data directly in client components unless absolutely necessary.
- ALWAYS prefer using server-side fetching in server components to keep sensitive logic and credentials secure.
- For client components, ALWAYS ensure that any data fetching does not expose sensitive information and handles errors gracefully.
- ALWAYS consider caching strategies and revalidation to optimize performance and reduce unnecessary network requests.
- ALWAYS handle errors and loading states appropriately to ensure a smooth user experience.
- ALWAYS document your data fetching logic clearly, including any assumptions, dependencies, and potential side effects, to make it easier for other developers to understand and maintain.
