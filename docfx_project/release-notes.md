# Release Notes

## 2.0.2

### Target frameworks

- `Albatross.Http` now multi-targets `net8.0` and `net10.0`
- `Microsoft.Extensions.Http`, `Microsoft.Extensions.Logging.Abstractions` and `System.Text.Json` updated to `10.0.12`
- SourceLink is referenced by the package project instead of `Directory.Build.props`, so it no longer leaks into the sample and test projects

### Fixes

- `AddLoggingHandler` uses `TryAddTransient`, so calling it more than once no longer registers duplicate `LoggingHandler` instances
- `Execute` no longer swallows `JsonException` when deserializing a successful response - a malformed body now surfaces as an exception instead of silently returning null
- `LoggingHandler` logs request start and completion at `Information` instead of `Debug`

## 2.0.1

- `Albatross.Exceptions` dependency moved from `1.0.0-rc.21` to the released `1.0.0`

## 2.0.0

Redesign of HTTP error handling around status-driven semantic exceptions. **This release contains breaking changes.**

### Breaking changes

- The `TError` generic parameter is gone from every `HttpClientExtensions` method. `Execute<TResponse, TError>`, `ExecuteOrThrow<TResponse, TError>`, `ExecuteOrThrowStruct<TResponse, TError>`, `ExecuteAsStream<TItem, TError>` and `Send<TError>` become `Execute<TResponse>`, `ExecuteOrThrow<TResponse>`, `ExecuteOrThrowStruct<TResponse>`, `ExecuteAsStream<TItem>` and `Send`
- `ServiceException<T>` has been removed. The library no longer deserializes the error response into a caller-supplied DTO; the raw body and its content type are captured instead and the caller decides how to interpret them
- `ServiceException` moved to the `Albatross.Http.Exceptions` namespace and is now thrown only for status codes that have no dedicated semantic exception
- `ExecuteOrThrow` and `ExecuteOrThrowStruct` throw `MissingRequiredValueException<TResponse>` rather than `ServiceException` when a successful response carries no body

### IHttpException

- Common interface implemented by every exception the library throws, exposing `Status`, `Method`, `Endpoint`, `ContentType` and `Content`
- `IHttpException.BuildMessage` renders the exception message as a JSON document. When the error body is itself a JSON object it is preserved and `status`, `method` and `endpoint` are merged in; a conflicting property in the body is never overwritten - the library's value is added under a numerically suffixed name such as `status2`. Any other body is wrapped in a new object under `content`

### Semantic exceptions

The response status code alone decides the exception type. Each type derives from the framework or `Albatross.Exceptions` type that carries the same meaning, so callers can catch the semantic type without knowing the transport.

| Status | Exception | Base type |
| --- | --- | --- |
| 400 | `HttpArgumentException` | `System.ArgumentException` |
| 401 | `HttpNotAuthenticatedException` | `Albatross.Exceptions.NotAuthenticatedException` |
| 403 | `HttpForbiddenException` | `Albatross.Exceptions.ForbiddenException` |
| 404 | `HttpNotFoundException` | `Albatross.Exceptions.NotFoundException` |
| 408 | `HttpTimeoutException` | `System.TimeoutException` |
| 409 | `HttpConflictException` | `Albatross.Exceptions.ConflictException` |
| 412 | `HttpPreconditionFailedException` | `Albatross.Exceptions.PreconditionFailedException` |
| 422 | `HttpValidationException` | `Albatross.Exceptions.ValidationException` |
| 501 | `HttpNotSupportedException` | `System.NotSupportedException` |
| any other 400+ | `ServiceException` | `System.Exception` |

- `MissingRequiredValueException<TResponse>` derives from `Albatross.Exceptions.MissingRequiredValueException` and is thrown when a successful response returns no content, or content that deserializes to null, where a value was required

### HttpResponseContent

- Record carrying the raw `Content` and `ContentType` of an error response
- `IsJson()` recognizes `application/json` as well as structured suffixes such as `application/problem+json`, and ignores parameters like `; charset=utf-8`

## 1.0.0

Initial release of Albatross.Http, a companion library for `HttpClient` providing extensions and shared types for HTTP client code generation.

### DefaultJsonSerializerOptions

- Static class providing a shared `JsonSerializerOptions` instance with camelCase naming policy, no indentation, and `JsonIgnoreCondition.WhenWritingNull`
- Used as the default by `RequestBuilder`

### RequestBuilder

- Fluent builder for constructing `HttpRequestMessage` with support for:
  - JSON, plain text, stream, and form URL-encoded content
  - Multipart form data with files (byte array and stream), string fields, and JSON fields
  - Query string parameters (accepts nullable strings)
  - Custom `JsonSerializerOptions`
- Automatically resets after `Build()` for reuse

### RequestBuilderExtensions

- `AddQueryString<T>` - generic method for any non-null value
- `AddQueryStringIfSet<T>` (class constraint) - only adds query string if value is not null or empty
- `AddQueryStringIfSet<T>` (struct constraint) - only adds query string if nullable value has a value, with automatic ISO8601 formatting for `DateTime`, `DateOnly`, `TimeOnly`, and `DateTimeOffset`

### HttpClientExtensions

- `Execute<TResponse>` and `Execute<TResponse, TError>` - send a request and deserialize the response, returning nullable result
- `Send<TError>` - fire-and-forget method for requests that don't return a body but still need error handling
- `ExecuteOrThrow<TResponse>` and `ExecuteOrThrow<TResponse, TError>` - guaranteed non-null response for reference types, throws `ServiceException` on empty or null content
- `ExecuteOrThrowStruct<TResponse>` and `ExecuteOrThrowStruct<TResponse, TError>` - guaranteed response for value types, internally deserializes as `Nullable<T>` to detect null JSON values
- `ExecuteAsStream<TItem>` and `ExecuteAsStream<TItem, TError>` - streams the response as `IAsyncEnumerable<TItem?>` for endpoints using `yield return` or `IAsyncEnumerable<T>`, using `HttpCompletionOption.ResponseHeadersRead` for true streaming

### UrlExtensions

- `CreateUrl` - builds URLs with query string parameters from `NameValueCollection`
- `CreateUrlArray` - batches large array query parameters across multiple URLs using repeated keys
- `CreateUrlArrayByDelimitedValue` - batches array parameters using a delimiter within a single key
- `GetFullUri` - resolves relative URIs against a base address
- ISO8601 format constants and extension methods for `DateOnly`, `TimeOnly`, `DateTime`, and `DateTimeOffset`

### ServiceException

- `ServiceException<T>` - typed exception carrying HTTP status code, method, endpoint, and a deserialized error object
- `ServiceException` - convenience subclass defaulting the error type to `string`

### LoggingHandler

- `DelegatingHandler` for structured logging of HTTP requests and responses via `ILogger`
- Logs request start, completion status, error response bodies, and cancellations

### RegistrationExtensions

- `AddLoggingHandler` - registers `LoggingHandler` as a transient service
- `BuildDefault` - configures `IHttpClientBuilder` with sensible defaults: removes built-in HTTP loggers, enables GZip/Deflate/Brotli automatic decompression, auto-redirect, and optional Windows default credentials with pre-authentication

### ContentTypes

- MIME type constants for JSON, text, HTML, CSV, form, Excel, octet-stream, multipart form data, and images
