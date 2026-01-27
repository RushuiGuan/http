# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build the solution
dotnet build http.sln

# Build a specific project
dotnet build Albatross.Http/Albatross.Http.csproj

# Run all tests
dotnet test

# Run a specific test
dotnet test --filter "FullyQualifiedName~TestCreateUrl.WithNullQueryString_ReturnsUrlOnly"

# Create NuGet package
dotnet pack Albatross.Http/Albatross.Http.csproj
```

## Architecture

This is a .NET library (`Albatross.Http`) providing HttpClient extensions for HTTP client code generation.

### Target Frameworks
- `net8.0` for the main library and tests

### Namespace
All types are in the `Albatross.Http` namespace.

### Key Components
- **RequestBuilder**: Fluent builder for creating `HttpRequestMessage` with JSON, string, stream, form, or multipart form content. Supports query string helpers for DateTime, DateOnly, TimeOnly, and DateTimeOffset. Resets automatically after `Build()`.
- **HttpClientExtensions**: Execute methods that handle request/response with typed error handling via `ServiceException<T>`. Includes `ExecuteOrThrow` (reference types), `ExecuteOrThrowStruct` (value types), and `ExecuteAsStream` (streaming `IAsyncEnumerable` responses).
- **UrlExtensions**: Query string building, URL batching for large array parameters, and `GetFullUri` for resolving relative URIs against a base address.
- **ServiceException**: Typed exception carrying HTTP status code, method, endpoint, and a deserialized error object.
- **LoggingHandler**: `DelegatingHandler` for structured logging of HTTP request/response details via `ILogger`.
- **ContentTypes**: MIME type constants.

### Testing
Uses xUnit. Test project: `Albatross.Http.Test`

### Sample Projects
`Sample.WebApi`, `Sample.WebClient`, and `Sample.CommandLine` are for internal testing and demonstration only. Do not add documentation (XML doc comments, README, release notes, etc.) to these projects.
