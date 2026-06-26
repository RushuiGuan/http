# Error Handling and the Exception Flow

This article describes how `Albatross.Http` turns an HTTP error response into a typed exception on the
client, and the reasoning behind the design. It covers the full flow from the server's response through
the client to the exception a caller ultimately catches.

## The problem it solves

In a typical HTTP client, the **success path** is well defined — you know the shape of the response and
deserialize it into a strong type. The **error path** rarely gets the same treatment. Past approaches
modeled errors with a typed error DTO shared between client and server, which causes three recurring
problems:

1. **Coupling.** The error DTO must exist on both ends and be kept in lock-step.
2. **Drift.** Over time the two definitions diverge, usually silently.
3. **Silent data loss.** When the body no longer matches the DTO, deserialization often *succeeds* anyway
   — unmatched fields are dropped — so the caller sees a partially-populated object and never knows.

On top of that, error bodies are simply not predictable. A 404 from your API is a tidy
`application/problem+json` document; a 502 from a gateway in front of it is an HTML page; a misconfigured
proxy might return nothing at all. A typed-DTO approach breaks down exactly when the system is most
broken — the worst possible time to lose your error information.

## Design principles

`Albatross.Http` takes a different position:

1. **The HTTP status code determines the exception type.** The status line is the one thing HTTP
   guarantees and that intermediaries set reliably, so it is the basis for the typed exception. This makes
   the client **server-agnostic** — it works against any HTTP server, Albatross-based or not, because it
   depends only on the status code, never on the server's implementation or error shape.
2. **The error body is captured raw, never deserialized into a contract.** The client stores the response
   `Content` (verbatim string) and its `ContentType`. It does not require — or assume — any schema.
3. **Both undefined and well-defined error consumption are supported.** Most callers never look at the
   body; the few that need it can read `Content` and parse it themselves.

The result is *conformity* (every error becomes a predictable, catchable exception) together with
*flexibility* (the original payload is preserved for the callers who want it).

## The semantic exception family

When a response has a status code of 400 or greater, the client throws a semantic exception selected by
the status code alone. Each exception lives in `Albatross.Http.Exceptions`, derives from a base type so it
can be caught semantically, and implements [`IHttpException`](#ihttpexception).

| Status | Exception | Base type |
| - | - | - |
| 400 | `HttpArgumentException` | `System.ArgumentException` |
| 401 | `HttpNotAuthenticatedException` | `Albatross.Exceptions.NotAuthenticatedException` |
| 403 | `HttpForbiddenException` | `Albatross.Exceptions.ForbiddenException` |
| 404 | `HttpNotFoundException` | `Albatross.Exceptions.NotFoundException` |
| 408 | `HttpTimeoutException` | `System.TimeoutException` |
| 409 | `HttpConflictException` | `Albatross.Exceptions.ConflictException` |
| 412 | `HttpPreconditionFailedException` | `Albatross.Exceptions.PreconditionFailedException` |
| 422 | `HttpValidationException` | `Albatross.Exceptions.ValidationException` |
| 501 | `HttpNotSupportedException` | `System.NotSupportedException` |
| any other 4xx/5xx | `ServiceException` | `System.Exception` |

Because each exception derives from its `Albatross.Exceptions` (or `System`) counterpart, a caller can catch
at whatever granularity it needs — the specific HTTP type, the semantic base type, or the
`IHttpException` interface. The status-to-type mapping mirrors the server-side
`Albatross.Hosting.GlobalExceptionHandler`, so the same semantic meaning travels end to end.

## IHttpException

Every semantic exception implements `IHttpException`, which exposes the structured facts about the failed
response:

```csharp
public interface IHttpException {
    int Status { get; }          // the HTTP status code
    string Method { get; }       // the request method, e.g. "GET"
    string Endpoint { get; }     // the fully-qualified request URI
    string? ContentType { get; } // the response media type; may be null
    string? Content { get; }     // the raw response body; may be empty/null
}
```

`Status`, `Method` and `Endpoint` are always available. `ContentType` and `Content` are best-effort: a
response may legally carry a body with no `Content-Type`, or no body at all (a 204, or an empty error), so
both are nullable and independent of one another.

## The flow

```
HTTP response (status >= 400)
        │
        ▼
ReadResponse(...)            → HttpResponseContent { ContentType, Content }   (raw, no deserialization)
        │
        ▼
BuildSemanticException(status, method, uri, content)
        │   switch on status code
        ▼
new HttpNotFoundException(...) / HttpValidationException(...) / ... / ServiceException(...)
        │   constructor builds the Message via IHttpException.BuildMessage
        ▼
throw   →  caught by the application
```

1. The `Execute` / `Send` / `ExecuteOrThrow` / `ExecuteAsStream` extension methods send the request.
2. On a status code of 400 or greater, `ReadResponse` reads the body as a string and records the media
   type into an `HttpResponseContent`. Nothing is deserialized; a 204 or zero-length response yields an
   empty `HttpResponseContent`.
3. `BuildSemanticException` switches on the status code to construct the matching exception (or
   `ServiceException` for an unmapped code), passing the `HttpResponseContent` through.
4. The exception's constructor builds its `Message` and is thrown.

## The Message versus the Content

The exception `Message` and the response `Content` serve two different audiences. Confusing them is the
most common mistake, so the distinction is deliberate:

- **`Message`** is **for humans and logs.** It is a JSON document synthesized by
  `IHttpException.BuildMessage`, combining the response body with the request context. It is a presentation
  artifact — *do not parse it.*
- **`Content`** (with `ContentType`) is the **source of truth.** It is byte-for-byte what the server sent,
  so a caller that needs the server's structured error detail parses `Content`, not `Message`.

### How BuildMessage composes the Message

`BuildMessage` produces a JSON object so the message is readable and log-friendly:

- **If the body is JSON** (per the `Content-Type`), it is parsed and the `status`, `method` and `endpoint`
  properties are merged in. A property already present with an equal value is left untouched; one present
  with a *different* value is preserved and ours is added under a free, numerically-suffixed name (e.g.
  `status2`) so the original body is never clobbered.
- **Otherwise** (non-JSON, empty, or a JSON-typed body that fails to parse) a new object is created
  carrying `status`, `method`, `endpoint` and the raw `content`.

For example, a `404` whose body is `{ "title": "Not Found", "status": 404 }` produces a message of:

```json
{ "title": "Not Found", "status": 404, "method": "GET", "endpoint": "http://host/api/items/42" }
```

while a `text/plain` body of `entity not found` produces:

```json
{ "status": 404, "method": "GET", "endpoint": "http://host/api/items/42", "content": "entity not found" }
```

Because the message reorders, renames-on-collision, and re-emits JSON, it is *semantically* close to the
body but not identical — another reason to treat `Content`, not `Message`, as the authoritative payload.

## Consuming errors

### The common case — catch and log

Most applications never inspect the body. They catch the semantic type (or the interface) and log:

```csharp
try {
    var item = await client.ExecuteOrThrow<Item>(request, options, cancellationToken);
} catch (NotFoundException) {
    // Albatross.Exceptions.NotFoundException — also catches HttpNotFoundException
    return null;
} catch (IHttpException ex) {
    // any HTTP error: structured facts plus a log-ready message
    logger.LogWarning("HTTP {Status} from {Endpoint}: {Message}", ex.Status, ex.Endpoint, ex.Message);
    throw;
}
```

### The opt-in case — read the body

The few callers that need the server's structured error detail read `Content`, using `ContentType` to
decide how to interpret it:

```csharp
catch (IHttpException ex) when (ex.Content is not null) {
    var contentInfo = new HttpResponseContent { ContentType = ex.ContentType, Content = ex.Content };
    if (contentInfo.IsJson()) {
        var problem = JsonSerializer.Deserialize<ProblemDetails>(ex.Content, options);
        // use problem.Detail, problem.Errors, etc.
    } else {
        // treat ex.Content as plain text
    }
}
```

The coupling to a particular error shape is now **local to this one caller and entirely opt-in**, instead
of a contract every client/server pair must maintain.

## Design rationale and trade-offs

**Why this is a good fit.**

- **It fails well.** Building the exception from the status code means a gateway's HTML 502, an empty 401,
  or a truncated body still produces a correct, catchable exception with the body preserved for diagnosis.
- **Coupling is opt-in, not mandatory.** A caller that wants structured errors still has to know the shape
  to parse it — but that knowledge is localized to that caller, not a shared contract with a silent
  field-loss failure mode.
- **Conformity and flexibility coexist.** A uniform catch surface across every service, with the original
  payload available when it matters.

**Things to keep in mind.**

- **The typed exception is only as semantic as the server's status discipline.** Many real systems
  (OAuth 2.0, ASP.NET Core's default `ValidationProblem`) collapse client errors into `400` and
  discriminate in the body. Against those servers the exception type is coarse — the real distinction
  lives in `Content`, which this design exposes but does not type. The exception reflects the HTTP status,
  not necessarily the domain error.
- **`Message` is not a contract.** It is for humans and logs. Programmatic access goes through `Status`,
  `Content` and `ContentType`.
- **One topology shares the base types — by setup, not by design.** The design itself never assumes
  anything about the server. But when your app happens to be *both* an Albatross-based server (throwing
  `Albatross.Exceptions` types) *and* a client of another service — as this sample is — the two meet at the
  base type: `HttpNotFoundException` derives from `Albatross.Exceptions.NotFoundException`, so
  `catch (NotFoundException)` catches both a locally-thrown not-found and a propagated upstream one. That is
  usually desirable; when you need to distinguish them, `IHttpException` is the discriminator — only the
  client-originated exceptions implement it. Against any other server this overlap simply does not arise,
  and the design stands unchanged.
