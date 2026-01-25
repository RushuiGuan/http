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
dotnet test --filter "FullyQualifiedName~TestExtensions.CreateUrl_WithNoQueryString_ReturnsUrl"

# Create NuGet package
dotnet pack Albatross.Http/Albatross.Http.csproj
```

## Architecture

This is a .NET library (`Albatross.Http`) providing HttpClient extensions for HTTP client code generation.

### Target Frameworks
- `netstandard2.1` and `net8.0` for the main library
- `net8.0` for tests

### Namespaces
The library uses two namespaces:
- `Albatross.Http` - Core extensions (URL building, request/response handling, content types)
- `Albatross.WebClient` - Service exception and HttpClient execution methods

### Key Components
- **RequestExtensions**: Factory methods for creating `HttpRequestMessage` with JSON, string, stream, or multipart form content
- **ResponseExtensions**: Methods for reading responses as text, JSON, or streams with automatic GZip decompression
- **UrlExtensions**: Query string building and URL batching for large array parameters
- **HttpClientExtensions**: Execute methods that handle request/response with typed error handling via `ServiceException<T>`
- **ContentTypes**: MIME type constants

### Testing
Uses xUnit with FluentAssertions. Test project: `Albatross.Http.Test`
