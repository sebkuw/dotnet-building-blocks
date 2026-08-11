# Angular shared components contract

This document defines the HTTP-facing compatibility boundary between this repository and:

- repository: `C:\A_REPOS\angular-shared-components`
- package: `@netdevs/shared-ui-list`
- reference version: `0.0.1`
- reference models: `projects/shared-ui-list/src/lib/components/dynamic-table/models`

The contract is protected by tests in `NetDevs.QueryableProcessor.Tests` and `NetDevs.ExceptionProcessor.Tests`. Changes to either side require synchronized contract tests, documentation, changelogs, and SemVer analysis.

## Query request

`toPaginationRequestDto` produces the following request shape. Property names are intentionally PascalCase.

```json
{
  "SortParam": "createdAt desc",
  "Filters": [
    {
      "PropertyPath": "name",
      "Operation": 6,
      "Value": "api"
    }
  ],
  "PaginationOptions": {
    "PageNumber": 1,
    "PageSize": 20
  }
}
```

Rules:

- `PaginationOptions` is required by the .NET contract and is always emitted by the Angular adapter.
- `PageNumber` is 1-based and must be at least 1.
- `PageSize` must be between 1 and 100.
- `Filters` and `SortParam` may be absent.
- Property paths may be camelCase or PascalCase and may contain nested segments separated by `.`.
- Unknown property paths leave the query unchanged for sorting and must not enable arbitrary method execution.

## Sort format

The preferred Angular format is:

```text
PropertyPath asc
PropertyPath desc
```

Examples: `name asc`, `createdAt desc`, `category.name asc`.

For backward compatibility, .NET also accepts `asc_PropertyPath` and `desc_PropertyPath`. Removing either format is a breaking change.

## Filter operation values

The numeric values are a serialized cross-language contract:

| Value | Operation |
| ---: | --- |
| 0 | `Equal` |
| 1 | `NotEqual` |
| 2 | `GreaterThan` |
| 3 | `LessThan` |
| 4 | `GreaterThanOrEqual` |
| 5 | `LessThanOrEqual` |
| 6 | `Contains` |
| 7 | `StartsWith` |
| 8 | `EndsWith` |
| 9 | `In` |
| 10 | `NotIn` |

Existing values must never be reordered or reused. Additive operations require coordinated frontend and backend releases.

## Pagination response

The .NET response uses explicit PascalCase JSON names, even when the application configures a global camelCase serializer policy:

```json
{
  "Data": [],
  "TotalItems": 0,
  "TotalPages": 0,
  "PageNumber": 1,
  "PageSize": 20,
  "HasNextPage": false,
  "HasPreviousPage": false
}
```

`@netdevs/shared-ui-list` currently declares all fields except `HasPreviousPage`; the additional field is backward-compatible for JSON consumers. Changing a field name, type, page indexing, or total-page calculation requires a coordinated breaking-change review.

## Error response and correlation

`NetDevs.ExceptionProcessor` returns camelCase error JSON:

```json
{
  "code": "ValidationError",
  "httpCode": 400,
  "mainText": "Invalid input",
  "description": "Validation failed: Email is required",
  "timestamp": "2026-08-11T12:00:00Z",
  "traceId": "client-or-server-correlation-id"
}
```

Rules:

- Accept `X-Correlation-ID` from the request when it is non-empty; otherwise generate one.
- Preserve one value in `HttpContext.Items["CorrelationId"]`, the response header, error body, and structured logs.
- Register `TraceIdMiddleware` before `GlobalExceptionMiddleware`.
- Treat `code` as the stable machine-readable discriminator and `mainText`/`description` as safe display text.
- Never expose secrets, stack traces, database details, internal paths, or unreviewed infrastructure messages.

## Verification checklist

- Compare the current Angular interfaces and enum values with this document.
- Run `NetDevs.QueryableProcessor.Tests` for request, response, filtering, sorting, and pagination changes.
- Run `NetDevs.ExceptionProcessor.Tests` for error and correlation changes.
- Update both repositories when a shared contract changes.
- Record the change and migration impact in the affected package changelogs.
