# sebkuw.QueryableProcessor

Reusable .NET 10 helpers for dynamic `IQueryable<T>` filtering, sorting, pagination, projection, and Angular table contracts.

## Installation

```powershell
dotnet add package sebkuw.QueryableProcessor
```

## Request contract

The contract matches `toPaginationRequestDto` from `@netdevs/shared-ui-list`:

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
PaginationResponse<ArticleListItem> response = await dbContext.Articles
    .SolveRequest(
        request,
        article => new ArticleListItem(article.Id, article.Title),
        cancellationToken);
```

The response exposes `Data`, `TotalItems`, `TotalPages`, `PageNumber`, `PageSize`, `HasNextPage`, and `HasPreviousPage` with explicit PascalCase JSON names. Pages are 1-based and page size is limited to 100.

Filtering, sorting, count, pagination, and projection remain in the query pipeline so EF Core providers can translate them. Invalid sort paths leave the source ordering unchanged.

See the complete [Angular compatibility contract](../../docs/angular-shared-components-contract.md).

## Development

- [Library instructions](AGENTS.md)
- [Development skill](../../.agents/skills/develop-sebkuw-queryable-processor/SKILL.md)
- [Changelog](CHANGELOG.md)
- Tests: `tests/sebkuw.QueryableProcessor.Tests`

```powershell
dotnet test tests/sebkuw.QueryableProcessor.Tests/sebkuw.QueryableProcessor.Tests.csproj
```
