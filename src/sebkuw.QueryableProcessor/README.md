# sebkuw.QueryableProcessor

Reusable .NET 10 helpers for dynamic `IQueryable<T>` filtering, sorting, pagination, projection, and Angular table contracts.

## Installation

```powershell
dotnet add package sebkuw.QueryableProcessor
```

## Request contract

The contract matches `toPaginationRequestDto` from `@sebkuw/shared-ui-list` (previously `@netdevs/shared-ui-list`):

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

`SortParam` accepts the preferred Angular form `Property asc|desc` and the legacy form `asc_Property|desc_Property`. Property lookup is case-insensitive and supports nested paths.

Supported operations are `Equal`, `NotEqual`, `GreaterThan`, `LessThan`, `GreaterThanOrEqual`, `LessThanOrEqual`, `Contains`, `StartsWith`, `EndsWith`, `In`, and `NotIn`. Their numeric values 0-10 are part of the cross-language contract.

## Execute a request

```csharp
using sebkuw.QueryableProcessor.Models;
using sebkuw.QueryableProcessor.Solvers;

PaginationResponse<ArticleListItem> response = await dbContext.Articles
    .SolveRequest(
        request,
        article => new ArticleListItem(article.Id, article.Title),
        cancellationToken);
```

The response exposes `Data`, `TotalItems`, `TotalPages`, `PageNumber`, `PageSize`, `HasNextPage`, and `HasPreviousPage` with explicit PascalCase JSON names. Pages are 1-based and page size is limited to 100.

The example assumes an EF Core `DbContext`, a `RequestDto`, and an application-owned `ArticleListItem` DTO. `SolveRequest` requires an EF Core asynchronous query provider; it is not an asynchronous wrapper for an in-memory `List<T>.AsQueryable()`.

Filtering, sorting, count, pagination, and projection remain in the query pipeline. SQL translation depends on the selected EF Core provider and projection. Invalid sort paths leave the source ordering unchanged. No default ordering is added, so supply a stable sort for predictable pagination.

Use `ApplyRequest(request)` when another pipeline, such as CSV export, needs the composed `IQueryable<T>` without immediately producing a pagination response. Pass `includePagination: false` to retain filtering and sorting while exporting all matching records.

Import `sebkuw.QueryableProcessor.Extensions` for `ApplyFilters`, `SortBy`, `Paginate`, and `ApplyRequest`. These helpers compose a query without materializing it. `PaginationOptions` defaults to page 1 with 10 items; filters are combined with logical AND.

## Validation and limitations

- Unknown filter paths, unsupported operations, and invalid pagination raise argument errors. Invalid sort formats or unknown sort paths leave the query unchanged. Malformed GUID text raises `FormatException`.
- `In` and `NotIn` require a collection; string operations require a string property.
- JSON scalar conversion explicitly supports `int`, `long`, `string`, `bool`, `Guid`, `DateTime`, and enums. JSON values for other types, including `decimal`, `double`, and `DateTimeOffset`, require application-side conversion to typed values before filtering.
- Property-path resolution is not a field authorization policy. Restrict client-visible fields and filter complexity in the application before processing requests.
- Nested paths and string operations do not add null guards for LINQ-to-Objects execution. Test nullable navigation and string values with the actual provider.
- `RequestDto.PaginationOptions` must be supplied even when `ApplyRequest` is called with `includePagination: false`.
- Integration tests use EF Core InMemory. Relational SQL translation and collation are not verified by those tests.

See the complete [Angular compatibility contract](https://github.com/sebkuw/dotnet-building-blocks/blob/main/docs/angular-shared-components-contract.md).

## Development

Run the test command from the repository root. Full verification instructions are in the [repository guide](https://github.com/sebkuw/dotnet-building-blocks#verification).

Run the test command from the repository root. Full verification instructions are in the [repository guide](https://github.com/sebkuw/dotnet-building-blocks#verification).

- [Changelog](https://github.com/sebkuw/dotnet-building-blocks/blob/main/src/sebkuw.QueryableProcessor/CHANGELOG.md)
- Tests: `tests/sebkuw.QueryableProcessor.Tests`

```powershell
dotnet test tests/sebkuw.QueryableProcessor.Tests/sebkuw.QueryableProcessor.Tests.csproj
```
