# Albatross.Http

A companion library for `HttpClient` that simplifies building HTTP requests, executing them with typed error handling, and streaming responses. Designed for use with `IHttpClientFactory` and code-generated HTTP clients.

## Key Features

- **RequestBuilder** - Fluent API for constructing `HttpRequestMessage` with JSON, text, stream, form, and multipart form data content. Includes typed query string helpers for date/time types.
- **Typed Execute methods** - Send requests and deserialize responses with automatic error handling via `ServiceException<T>`. Separate methods for nullable results, guaranteed non-null reference types, and guaranteed value types.
- **Streaming support** - `ExecuteAsStream` for consuming `IAsyncEnumerable` endpoints with true streaming via `HttpCompletionOption.ResponseHeadersRead`.
- **URL utilities** - Query string building, URL batching for large array parameters, and relative URI resolution.
- **Structured logging** - `LoggingHandler` integrates with `ILogger` for HTTP request/response logging.

## Quick Start

Install the package:
```bash
dotnet add package Albatross.Http
```

Build and execute a request:
```csharp
using Albatross.Http;

// Build a request
var request = new RequestBuilder()
    .WithMethod(HttpMethod.Get)
    .WithRelativeUrl("api/items")
    .AddQueryString("category", "books")
    .Build();

// Execute with typed error handling
var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var result = await httpClient.Execute<Item[]>(request, options, cancellationToken);
```

Stream results from an `IAsyncEnumerable` endpoint:
```csharp
var request = new RequestBuilder()
    .WithMethod(HttpMethod.Get)
    .WithRelativeUrl("api/items/stream")
    .Build();

await foreach (var item in httpClient.ExecuteAsStream<Item, string>(request, options, cancellationToken)) {
    Console.WriteLine(item);
}
```

Register the logging handler with `IHttpClientFactory`:
```csharp
services.AddTransient<LoggingHandler>();
services.AddHttpClient("MyApi")
    .AddHttpMessageHandler<LoggingHandler>();
```

## Dependencies

- [Microsoft.Extensions.Http](https://www.nuget.org/packages/Microsoft.Extensions.Http) 8.0.1
- [Microsoft.Extensions.Logging.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions) 8.0.3
- [System.Text.Json](https://www.nuget.org/packages/System.Text.Json) 8.0.6

## Source Code

- [https://github.com/RushuiGuan/http](https://github.com/RushuiGuan/http)

## Nuget Packages

| Name | Version |
| - | - |
| `Albatross.Http` | [![NuGet Version](https://img.shields.io/nuget/v/Albatross.Http)](https://www.nuget.org/packages/Albatross.Http) |
