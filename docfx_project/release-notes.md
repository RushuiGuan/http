# Release Notes

## 1.0.0

Initial release of Albatross.Http, a companion library for `HttpClient` providing extensions and shared types for HTTP client code generation.

### RequestBuilder

- Fluent builder for constructing `HttpRequestMessage` with support for:
  - JSON, plain text, stream, and form URL-encoded content
  - Multipart form data with files (byte array and stream), string fields, and JSON fields
  - Query string parameters with typed helpers for `DateTime`, `DateOnly`, `TimeOnly`, and `DateTimeOffset`
  - Custom `JsonSerializerOptions`
- Automatically resets after `Build()` for reuse

### HttpClientExtensions

- `Execute<TResponse>` and `Execute<TResponse, TError>` - send a request and deserialize the response, returning nullable result
- `ExecuteOrThrow<TResponse, TError>` - guaranteed non-null response for reference types, throws `ServiceException` on empty or null content
- `ExecuteOrThrowStruct<TResponse, TError>` - guaranteed response for value types, internally deserializes as `Nullable<T>` to detect null JSON values
- `ExecuteAsStream<TItem, TError>` - streams the response as `IAsyncEnumerable<TItem?>` for endpoints using `yield return` or `IAsyncEnumerable<T>`, using `HttpCompletionOption.ResponseHeadersRead` for true streaming

### UrlExtensions

- `CreateUrl` - builds URLs with query string parameters from `NameValueCollection`
- `CreateUrlArray` - batches large array query parameters across multiple URLs using repeated keys
- `CreateUrlArrayByDelimitedValue` - batches array parameters using a delimiter within a single key
- `GetFullUri` - resolves relative URIs against a base address

### ServiceException

- `ServiceException<T>` - typed exception carrying HTTP status code, method, endpoint, and a deserialized error object
- `ServiceException` - convenience subclass defaulting the error type to `string`

### LoggingHandler

- `DelegatingHandler` for structured logging of HTTP requests and responses via `ILogger`
- Logs request start, completion status, error response bodies, and cancellations

### ContentTypes

- MIME type constants for JSON, text, HTML, CSV, form, Excel, octet-stream, multipart form data, and images
