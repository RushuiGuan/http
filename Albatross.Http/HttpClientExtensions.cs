using Albatross.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.Http {
	public static class HttpClientExtensions {
		static Exception ConvertToSemanticException(Exception source) {
			Exception? result = null;
			if (source is IServiceException serviceException) {
				result = (int)serviceException.StatusCode switch {
					NotAuthenticatedException.StatusCode => new NotAuthenticatedException("Not authenticated", source),
					ForbiddenException.StatusCode => new ForbiddenException("Forbidden", source),
					NotFoundException.StatusCode => new NotFoundException("Not found", source),
					ConflictException.StatusCode => new ConflictException("Conflict", source),
					PreconditionFailedException.StatusCode => new PreconditionFailedException("Precondition failed", source),
					ValidationException.StatusCode => new ValidationException("Validation failed", source),
					_ => null,
				};
			}
			return result ?? source;
		}
		static async Task<T?> ReadResponse<T>(HttpResponseMessage response, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken) {
			if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0) {
				return default;
			} else {
				if (typeof(T) == typeof(string)) {
					return (T?)(object)await response.Content.ReadAsStringAsync(cancellationToken);
				} else {
					return await response.Content.ReadFromJsonAsync<T>(serializerOptions, cancellationToken);
				}
			}
		}

		/// <summary>
		/// Sends the HTTP request and deserializes the response, using <see cref="string"/> as the error type.
		/// </summary>
		/// <inheritdoc cref="Execute{TResponse, TError}"/>
		public static Task<TResponse?> Execute<TResponse>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken)
			=> Execute<TResponse, string>(client, request, serializerOptions, cancellationToken);

		/// <summary>
		/// Sends the HTTP request and deserializes the response as <typeparamref name="TResponse"/>.
		/// Returns null for 204 No Content or zero-length responses.
		/// </summary>
		/// <typeparam name="TResponse">The expected response type.</typeparam>
		/// <typeparam name="TError">The error type to deserialize when the response indicates a failure.</typeparam>
		/// <param name="client">The HTTP client.</param>
		/// <param name="request">The HTTP request message to send.</param>
		/// <param name="serializerOptions">The JSON serializer options for deserialization.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <returns>The deserialized response, or null if the response has no content.</returns>
		/// <remarks>
		/// On an error status code (400+), the deserialized <typeparamref name="TError"/> body is wrapped in a
		/// <see cref="ServiceException{TError}"/>. Recognized status codes are then rethrown as a semantic exception
		/// from <c>Albatross.Exceptions</c> carrying that <see cref="ServiceException{TError}"/> as the inner exception;
		/// all other error codes surface the <see cref="ServiceException{TError}"/> directly.
		/// </remarks>
		/// <exception cref="ServiceException{TError}">Thrown for an error status code (400+) that has no dedicated semantic exception.</exception>
		/// <exception cref="NotAuthenticatedException">Thrown on 401 Unauthorized.</exception>
		/// <exception cref="ForbiddenException">Thrown on 403 Forbidden.</exception>
		/// <exception cref="NotFoundException">Thrown on 404 Not Found.</exception>
		/// <exception cref="ConflictException">Thrown on 409 Conflict.</exception>
		/// <exception cref="PreconditionFailedException">Thrown on 412 Precondition Failed.</exception>
		/// <exception cref="ValidationException">Thrown on 422 Unprocessable Entity.</exception>
		public static async Task<TResponse?> Execute<TResponse, TError>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken) {
			using var response = await client.SendAsync(request, cancellationToken);
			if (response.StatusCode >= HttpStatusCode.BadRequest) {
				var errorResult = await ReadResponse<TError>(response, serializerOptions, cancellationToken);
				var exception = new ServiceException<TError>(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), errorResult);
				throw ConvertToSemanticException(exception);
			} else {
				var result = await ReadResponse<TResponse>(response, serializerOptions, cancellationToken);
				return result;
			}
		}

		/// <summary>
		/// Sends the HTTP request and discards the response body, throwing on an error status code (400+).
		/// </summary>
		/// <typeparam name="TError">The error type to deserialize when the response indicates a failure.</typeparam>
		/// <param name="client">The HTTP client.</param>
		/// <param name="request">The HTTP request message to send.</param>
		/// <param name="serializerOptions">The JSON serializer options for deserialization.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <remarks>Error status codes (400+) are handled as described on <see cref="Execute{TResponse, TError}"/>.</remarks>
		/// <exception cref="ServiceException{TError}">Thrown for an error status code (400+) that has no dedicated semantic exception.</exception>
		/// <exception cref="NotAuthenticatedException">Thrown on 401 Unauthorized.</exception>
		/// <exception cref="ForbiddenException">Thrown on 403 Forbidden.</exception>
		/// <exception cref="NotFoundException">Thrown on 404 Not Found.</exception>
		/// <exception cref="ConflictException">Thrown on 409 Conflict.</exception>
		/// <exception cref="PreconditionFailedException">Thrown on 412 Precondition Failed.</exception>
		/// <exception cref="ValidationException">Thrown on 422 Unprocessable Entity.</exception>
		public static async Task Send<TError>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken) {
			using var response = await client.SendAsync(request, cancellationToken);
			if (response.StatusCode >= HttpStatusCode.BadRequest) {
				var errorResult = await ReadResponse<TError>(response, serializerOptions, cancellationToken);
				var exception = new ServiceException<TError>(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), errorResult);
				throw ConvertToSemanticException(exception);
			}
		}

		/// <summary>
		/// Sends the HTTP request and returns a guaranteed non-null response of the specified reference type.
		/// </summary>
		/// <typeparam name="TResponse">The expected response type. Must be a reference type.</typeparam>
		/// <typeparam name="TError">The error type to deserialize when the response indicates a failure.</typeparam>
		/// <param name="client">The HTTP client.</param>
		/// <param name="request">The HTTP request message to send.</param>
		/// <param name="serializerOptions">The JSON serializer options for deserialization.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <returns>A non-null deserialized response of type <typeparamref name="TResponse"/>.</returns>
		/// <remarks>
		/// Error status codes (400+) are handled as described on <see cref="Execute{TResponse, TError}"/>. A successful
		/// response with no body (204 or zero length) or one that deserializes to null is treated as a contract violation.
		/// </remarks>
		/// <exception cref="ServiceException{TError}">Thrown for an error status code (400+) that has no dedicated semantic exception.</exception>
		/// <exception cref="NotAuthenticatedException">Thrown on 401 Unauthorized.</exception>
		/// <exception cref="ForbiddenException">Thrown on 403 Forbidden.</exception>
		/// <exception cref="NotFoundException">Thrown on 404 Not Found.</exception>
		/// <exception cref="ConflictException">Thrown on 409 Conflict.</exception>
		/// <exception cref="PreconditionFailedException">Thrown on 412 Precondition Failed.</exception>
		/// <exception cref="ValidationException">Thrown on 422 Unprocessable Entity.</exception>
		/// <exception cref="MissingRequiredValueException">Thrown when a successful response has no content or deserializes to null.</exception>
		public static async Task<TResponse> ExecuteOrThrow<TResponse, TError>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken) where TResponse : class {
			using var response = await client.SendAsync(request, cancellationToken);
			if (response.StatusCode >= HttpStatusCode.BadRequest) {
				var errorResult = await ReadResponse<TError>(response, serializerOptions, cancellationToken);
				throw ConvertToSemanticException(new ServiceException<TError>(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), errorResult));
			} else {
				if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0) {
					throw new MissingRequiredValueException($"Expected {typeof(TResponse)} but no content was returned", new ServiceException(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), null));
				}
				var result = await ReadResponse<TResponse>(response, serializerOptions, cancellationToken);
				if (result == null) {
					throw new MissingRequiredValueException($"Expected {typeof(TResponse)} but no content was returned", new ServiceException(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), null));
				}
				return result;
			}
		}

		public static Task<TResponse> ExecuteOrThrow<TResponse>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken) where TResponse : class
			=> ExecuteOrThrow<TResponse, string>(client, request, serializerOptions, cancellationToken);

		/// <summary>
		/// Sends the HTTP request and returns a guaranteed response of the specified value type.
		/// Internally deserializes as <see cref="Nullable{T}"/> to detect null JSON values for value types.
		/// </summary>
		/// <typeparam name="TResponse">The expected response type. Must be a value type.</typeparam>
		/// <typeparam name="TError">The error type to deserialize when the response indicates a failure.</typeparam>
		/// <param name="client">The HTTP client.</param>
		/// <param name="request">The HTTP request message to send.</param>
		/// <param name="serializerOptions">The JSON serializer options for deserialization.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <returns>A deserialized response of type <typeparamref name="TResponse"/>.</returns>
		/// <remarks>
		/// Error status codes (400+) are handled as described on <see cref="Execute{TResponse, TError}"/>. A successful
		/// response with no body (204 or zero length) or one that deserializes to null is treated as a contract violation.
		/// </remarks>
		/// <exception cref="ServiceException{TError}">Thrown for an error status code (400+) that has no dedicated semantic exception.</exception>
		/// <exception cref="NotAuthenticatedException">Thrown on 401 Unauthorized.</exception>
		/// <exception cref="ForbiddenException">Thrown on 403 Forbidden.</exception>
		/// <exception cref="NotFoundException">Thrown on 404 Not Found.</exception>
		/// <exception cref="ConflictException">Thrown on 409 Conflict.</exception>
		/// <exception cref="PreconditionFailedException">Thrown on 412 Precondition Failed.</exception>
		/// <exception cref="ValidationException">Thrown on 422 Unprocessable Entity.</exception>
		/// <exception cref="MissingRequiredValueException">Thrown when a successful response has no content or deserializes to null.</exception>
		public static async Task<TResponse> ExecuteOrThrowStruct<TResponse, TError>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken) where TResponse : struct {
			using var response = await client.SendAsync(request, cancellationToken);
			if (response.StatusCode >= HttpStatusCode.BadRequest) {
				var errorResult = await ReadResponse<TError>(response, serializerOptions, cancellationToken);
				throw ConvertToSemanticException(new ServiceException<TError>(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), errorResult));
			} else {
				if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0) {
					throw new MissingRequiredValueException($"Expected {typeof(TResponse)} but no content was returned", new ServiceException(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), null));
				}
				var result = await ReadResponse<TResponse?>(response, serializerOptions, cancellationToken);
				if (result == null) {
					throw new MissingRequiredValueException($"Expected {typeof(TResponse)} but no content was returned", new ServiceException(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), null));
				}
				return result.Value;
			}
		}
		public static Task<TResponse> ExecuteOrThrowStruct<TResponse>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken) where TResponse : struct
			=> ExecuteOrThrowStruct<TResponse, string>(client, request, serializerOptions, cancellationToken);


#if NET8_0_OR_GREATER
		/// <summary>
		/// Sends the HTTP request and streams the response as an async enumerable of items, yielding each item as it is
		/// deserialized from the response stream. Designed for endpoints that use <c>yield return</c> or return
		/// <see cref="IAsyncEnumerable{T}"/>. Uses <see cref="HttpCompletionOption.ResponseHeadersRead"/> to begin
		/// processing before the full response is received.
		/// </summary>
		/// <typeparam name="TItem">The type of each item in the streamed array.</typeparam>
		/// <typeparam name="TError">The error type to deserialize when the response indicates a failure.</typeparam>
		/// <param name="client">The HTTP client.</param>
		/// <param name="request">The HTTP request message to send.</param>
		/// <param name="serializerOptions">The JSON serializer options for deserialization.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <returns>An async enumerable of deserialized items of type <typeparamref name="TItem"/>. Items may be null if the JSON array contains null elements.</returns>
		/// <remarks>
		/// The status code is checked before streaming begins; error handling matches <see cref="Execute{TResponse, TError}"/>.
		/// </remarks>
		/// <exception cref="ServiceException{TError}">Thrown for an error status code (400+) that has no dedicated semantic exception.</exception>
		/// <exception cref="NotAuthenticatedException">Thrown on 401 Unauthorized.</exception>
		/// <exception cref="ForbiddenException">Thrown on 403 Forbidden.</exception>
		/// <exception cref="NotFoundException">Thrown on 404 Not Found.</exception>
		/// <exception cref="ConflictException">Thrown on 409 Conflict.</exception>
		/// <exception cref="PreconditionFailedException">Thrown on 412 Precondition Failed.</exception>
		/// <exception cref="ValidationException">Thrown on 422 Unprocessable Entity.</exception>
		public static async IAsyncEnumerable<TItem?> ExecuteAsStream<TItem, TError>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, [EnumeratorCancellation] CancellationToken cancellationToken) {
			using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			if (response.StatusCode >= HttpStatusCode.BadRequest) {
				var errorResult = await ReadResponse<TError>(response, serializerOptions, cancellationToken);
				throw ConvertToSemanticException(new ServiceException<TError>(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), errorResult));
			}
			await foreach (var item in response.Content.ReadFromJsonAsAsyncEnumerable<TItem>(serializerOptions, cancellationToken)) {
				yield return item;
			}
		}
		public static IAsyncEnumerable<TItem?> ExecuteAsStream<TItem>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken)
			=> ExecuteAsStream<TItem, string>(client, request, serializerOptions, cancellationToken);
#endif
	}
}