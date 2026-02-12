# Release Notes

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
