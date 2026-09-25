---
name: server-actions-instructions
description: Instructions for implementing server-side data mutations in the Blazor PWA and Web API
---
# Server-Side Mutation Guidelines

## Mutation Rules

- ALL data mutations from the Blazor PWA MUST be sent to the Web API through `UrlShortenerApiClient`.
- Razor components MUST use typed C# request and response models for all mutation payloads.
- UI event handlers may initiate mutations, but they MUST NOT access `UrlShortenerDbContext` or execute database queries.
- The Web API is the server-side boundary for validation, authentication/authorization, business rules, and persistence.
- Server-side mutation handlers MUST NOT throw errors as part of normal control flow. They MUST return a typed result object containing an `Error` or `Success` property.

## Result and Error Handling

- Define a typed result contract for each mutation, for example `MutationResult<T>` with `T? Success` and `string? Error` properties.
- Return `Success` only after the mutation completes successfully; return a safe, user-facing `Error` result for validation, authentication, authorization, conflict, or expected persistence failures.
- Do not expose exception messages, SQL, stack traces, secrets, or internal implementation details in `Error`.
- Razor components MUST inspect the result and update their success or error state; they MUST NOT rely on exceptions for expected validation or business-rule failures.

## Validation and Authentication

- ALL request input MUST be validated in the Web API endpoint before any database operation.
- Every mutating API endpoint MUST verify the caller's authenticated identity before accessing the database. Unauthenticated or unauthorized requests MUST be rejected.
- Per-user data MUST be scoped to the authenticated user's Clerk identity; never use a hard-coded or client-supplied identity without verifying it.

## Database Access

- Database operations MUST remain in `WebApi` and use `UrlShortenerDbContext` through the API's data-access boundary.
- Query and persistence helpers belong under `WebApi/Data` when a helper is needed to wrap database operations.
- `WebApp` MUST NOT reference `WebApi` internals, access `UrlShortenerDbContext`, or execute SQL/EF Core queries directly.

## Example Flow

For a create-short-URL mutation, follow this flow:

```razor
@inject UrlShortenerApiClient ApiClient

<EditForm Model="request" OnValidSubmit="CreateShortUrlAsync">
	<InputText @bind-Value="request.Url" />
	<button type="submit">Create</button>
</EditForm>

@code {
	private readonly CreateShortUrlRequest request = new();

	private async Task CreateShortUrlAsync()
	{
		var result = await ApiClient.CreateAsync(request);
		if (result.Error is not null)
		{
			// Display the safe error and keep the form usable.
			return;
		}

		// Update component state from result.Success.
	}
}
```

The API endpoint then authenticates the caller, validates the request, applies business rules, and delegates persistence to the data-access layer. The component does not construct SQL, access EF Core, or decide which user's records may be changed.

## Validation Checklist

- [ ] The Razor component uses a typed C# request model and handles the typed API response.
- [ ] The API validates required fields, limits, URL format, and any business invariants.
- [ ] The API obtains the user identity from the authenticated context rather than trusting a request field.
- [ ] Authentication and authorization failures return an appropriate HTTP error before database access.
- [ ] The mutation returns a typed `Error` or `Success` result instead of throwing for expected failures.
- [ ] API and client failures are handled without exposing secrets, SQL, or internal exception details.

## Database Operations Checklist

- [ ] The operation is implemented in `WebApi`, never directly in `WebApp`.
- [ ] Queries are scoped to the authenticated user where data is user-owned.
- [ ] EF Core operations use `UrlShortenerDbContext` and existing models/contracts.
- [ ] Reusable query or persistence logic is wrapped in a helper under `WebApi/Data`.
- [ ] The operation persists changes through the normal EF Core save path and does not use ad hoc SQL from the Razor component.

## Completion Checklist

- [ ] The mutation travels through `UrlShortenerApiClient`.
- [ ] Input validation occurs before any database operation.
- [ ] Authentication and per-user authorization are enforced by the API.
- [ ] Expected mutation failures are returned through the typed `Error` property, not thrown.
- [ ] No UI-to-database or cross-layer `WebApi` internal reference was introduced.
- [ ] Error, loading, and success states are handled in the Razor component.
- [ ] The affected projects build successfully and relevant tests pass.
