# Working with URL Length Limits

When passing array values as query parameters, URLs can exceed browser and server length limits (commonly 2048 characters). The `CreateUrlArray` and `CreateUrlArrayByDelimitedValue` methods solve this by automatically splitting values across multiple URLs.

## CreateUrlArray (Standard HTTP)

Use this method for APIs that follow the standard HTTP convention of repeated query parameters:

```
?id=0&id=1&id=2&id=3
```

### Example

```csharp
var baseUri = new Uri("http://api.example.com");
var queryStrings = new NameValueCollection { { "date", "2024-01-15" } };
var ids = new[] { "100", "101", "102", "103", "104" };

var urls = UrlExtensions.CreateUrlArray(
    baseUri,
    "api/items",
    queryStrings,
    maxUrlLength: 100,
    arrayQueryStringKey: "id",
    arrayQueryStringValues: ids
);

// Returns multiple URLs if values exceed maxUrlLength:
// "api/items?date=2024-01-15&id=100&id=101&"
// "api/items?date=2024-01-15&id=102&id=103&"
// "api/items?date=2024-01-15&id=104&"
```

## CreateUrlArrayByDelimitedValue (Non-Standard APIs)

Some APIs expect array values as a single delimited parameter rather than repeated parameters. This method accommodates that non-standard approach:

```
?id=0,1,2,3
```

### Example

```csharp
var baseUri = new Uri("http://api.example.com");
var queryStrings = new NameValueCollection { { "date", "2024-01-15" } };
var ids = new[] { "100", "101", "102", "103", "104" };

var urls = UrlExtensions.CreateUrlArrayByDelimitedValue(
    baseUri,
    "api/items",
    queryStrings,
    maxUrlLength: 100,
    queryStringKey: "id",
    delimiter: ",",
    arrayValues: ids
);

// Returns multiple URLs if values exceed maxUrlLength:
// "api/items?date=2024-01-15&id=100,101,102"
// "api/items?date=2024-01-15&id=103,104"
```

## How It Works

Both methods:

1. Calculate available URL length by subtracting the base URI length from `maxUrlLength`
2. Add array values until the limit would be exceeded
3. Start a new URL and continue with remaining values
4. Throw `ArgumentException` if a single value is too long to fit

## When to Use Each Method

| Scenario | Method |
|----------|--------|
| Standard REST APIs | `CreateUrlArray` |
| APIs expecting comma-separated values | `CreateUrlArrayByDelimitedValue` |
| APIs expecting pipe-separated values | `CreateUrlArrayByDelimitedValue` with `delimiter: "\|"` |
