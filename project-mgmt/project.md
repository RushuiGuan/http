# HTTP Error-Flow Integration Test

status: active
created: 2026-06-26T14:30:00-04:00
updated: 2026-06-26T14:30:00-04:00
----

## Business Requirements

`Albatross.Http` is a shared client library (repo RushuiGuan/http) that maps HTTP error
responses to semantic exceptions. We need confidence in how errors flow end-to-end across
three areas over two systems: an ASP.NET Core Web API → the `Albatross.Http` client → a CLI
program. The deliverable is a working integration-test harness (not unit tests) that lets us
observe, for each way the server can generate an error, exactly what the CLI ultimately prints.

Success: for every error-generation method on the API side, we can invoke it through the real
client and CLI and see the captured error (status, content, content type) in the CLI output, so
design choices in the client can be validated against real behavior.

## Technical Design

Three projects under `C:\app\http`:
- **Sample.WebApi** — the API. One controller per error-*generation method*; each exposes the
  same three endpoints so output is comparable across methods. All error endpoints are
  parameterless HTTP GETs that fail.
- **Sample.WebClient** — registers `ErrorClient`, a thin client whose single method takes the
  controller route + endpoint route as strings and invokes `GET /api/{controller}/{endpoint}`
  via the library's `Send` extension. The library throws the semantic exception.
- **Sample.CommandLine** — `error` verb group that calls `ErrorClient` and prints the resulting
  exception. CLI exception rendering goes through `DefaultCommandErrorHandler`.

**Test matrix axes:**
- *Generation method* (controller group): global exception-handler middleware
  (`GlobalErrorHandlerController`, `api/global`); `IExceptionHandler`→`ActionResult` with detail
  on/off (`DefaultWithDetailController` / `DefaultWithoutDetailController`); explicit
  `Problem()`/return (`ExplicitErrorReturnController`); FluentValidation
  (`FluentValidationErrorController`).
- *Error class* (endpoint, named by class not exception type): `semantic` (4xx), `server` (500),
  `semantic-with-inner` (semantic + inner exception).

**Observed server behavior:** the global middleware, the `IExceptionHandler` ObjectResult path,
and explicit `Problem()` all emit `application/problem+json`. The inner exception is dropped by
the `IExceptionHandler` path (only `err.Message` is used).

**Run mechanics:** projects are published to `c:\app\binary` (`InstallDirectory`); `alias.ps1`
maps `api`→`Sample.WebApi.exe` and `sample`→`Sample.CommandLine.exe`. Run shell commands via the
Bash tool, invoking the built exes directly. CLI talks to the API on `http://localhost:15000`.

## Key Design Decisions

- **Endpoints represent error *classes*, not specific exception types**: named `semantic` /
  `server` / `semantic-with-inner`. The specific exception subtype is irrelevant — semantic→status
  mapping is assumed already unit-tested. Avoids per-exception-type endpoint sprawl.
- **Masking tested via two controllers**: `DefaultWithDetailController` (`MaskExceptionDetail`
  off) and `DefaultWithoutDetailController` (on), each constructing its own `DefaultExceptionHandler`
  instance — because `MaskExceptionDetail` is fixed at construction, two instances are the only way
  to test both states deterministically. No custom handler is used.
- **Client captures raw content + content type; status code decides the exception type; no DTO
  deserialization**: deserializing the error body into a typed DTO on the client has no value — it
  forces a shared DTO on both ends, the DTO drifts, and fields are silently lost even when
  deserialization "succeeds". The semantic exception should carry { StatusCode, Method, Endpoint,
  Content (raw string), ContentType }. Rejected: the current generic `TError` model and
  `ServiceException<T>` family. This was confirmed after observing that `TError=string` yields an
  opaque escaped string and a mismatched DTO silently degrades to raw content anyway.

## Open Questions

- Concrete rename/refactor of `Albatross.Http.HttpClientExtensions` and the
  `Exceptions/*Exception<T>` family to the non-generic, raw-content model — not yet implemented.
  Blast radius includes `Albatross.Http.Test` (`TestExecuteErrorMapping`, `TestExecuteOrThrow`),
  `Sample.WebClient/ErrorClient.cs`, and `Sample.CommandLine/ErrorTestCase.cs`.
- Should `MissingRequiredValueException<TResponse>` stay generic? It concerns the response type, not
  the error body, so it is likely out of scope for the error-capture change.
- The remaining generation-method controllers (`ExplicitErrorReturnController`,
  `FluentValidationErrorController`) are stubs to be filled in.

## Dependencies & Constraints

- `net10.0`; `Albatross.Hosting`, `Albatross.Http`, `Albatross.CommandLine`.
- `Albatross.Http` is a published, shared library — API changes are breaking and must account for
  `Albatross.Http.Test` and external consumers.
- Run shell commands through the Bash tool, not PowerShell.
- API served at `http://localhost:15000`; built artifacts live in `c:\app\binary`.
