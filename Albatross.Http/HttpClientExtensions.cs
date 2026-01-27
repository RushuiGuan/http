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
		/// <summary>
		/// Sends the HTTP request and deserializes the response, using <see cref="string"/> as the error type.
		/// </summary>
		/// <inheritdoc cref="Execute{TResponse, TError}"/>
		public static Task<TResponse?> Execute<TResponse>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken)
			=> Execute<TResponse, string>(client, request, serializerOptions, cancellationToken);

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

		public static async Task<TResponse?> Execute<TResponse, TError>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken) {
			using var response = await client.SendAsync(request, cancellationToken);
			if (response.StatusCode >= HttpStatusCode.BadRequest) {
				var errorResult = await ReadResponse<TError>(response, serializerOptions, cancellationToken);
				throw new ServiceException<TError>(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), errorResult);
			} else {
				var result = await ReadResponse<TResponse>(response, serializerOptions, cancellationToken);
				return result;
			}
		}

		/// <summary>
		/// Sends the HTTP request and returns a guaranteed non-null response of the specified reference type.
		/// Throws <see cref="ServiceException{TError}"/> if the response indicates an error,
		/// or <see cref="ServiceException"/> if the response has no content or deserializes to null.
		/// </summary>
		/// <typeparam name="TResponse">The expected response type. Must be a reference type.</typeparam>
		/// <typeparam name="TError">The error type to deserialize when the response indicates a failure.</typeparam>
		/// <param name="client">The HTTP client.</param>
		/// <param name="request">The HTTP request message to send.</param>
		/// <param name="serializerOptions">The JSON serializer options for deserialization.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <returns>A non-null deserialized response of type <typeparamref name="TResponse"/>.</returns>
		/// <exception cref="ServiceException{TError}">Thrown when the response status code indicates an error (400+).</exception>
		/// <exception cref="ServiceException">Thrown when the response has no content or the deserialized result is null.</exception>
		public static async Task<TResponse> ExecuteOrThrow<TResponse, TError>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken) where TResponse : class {
			using var response = await client.SendAsync(request, cancellationToken);
			if (response.StatusCode >= HttpStatusCode.BadRequest) {
				var errorResult = await ReadResponse<TError>(response, serializerOptions, cancellationToken);
				throw new ServiceException<TError>(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), errorResult);
			} else {
				if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0) {
					throw new ServiceException(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), "Expected content but none were returned from the service");
				}
				var result = await ReadResponse<TResponse>(response, serializerOptions, cancellationToken);
				if (result == null) {
					throw new ServiceException(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), "Expected content but none were returned from the service");
				}
				return result;
			}
		}

		/// <summary>
		/// Sends the HTTP request and returns a guaranteed response of the specified value type.
		/// Throws <see cref="ServiceException{TError}"/> if the response indicates an error,
		/// or <see cref="ServiceException"/> if the response has no content or deserializes to null.
		/// Internally deserializes as <see cref="Nullable{T}"/> to detect null JSON values for value types.
		/// </summary>
		/// <typeparam name="TResponse">The expected response type. Must be a value type.</typeparam>
		/// <typeparam name="TError">The error type to deserialize when the response indicates a failure.</typeparam>
		/// <param name="client">The HTTP client.</param>
		/// <param name="request">The HTTP request message to send.</param>
		/// <param name="serializerOptions">The JSON serializer options for deserialization.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <returns>A deserialized response of type <typeparamref name="TResponse"/>.</returns>
		/// <exception cref="ServiceException{TError}">Thrown when the response status code indicates an error (400+).</exception>
		/// <exception cref="ServiceException">Thrown when the response has no content or the deserialized result is null.</exception>
		public static async Task<TResponse> ExecuteOrThrowStruct<TResponse, TError>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken) where TResponse : struct {
			using var response = await client.SendAsync(request, cancellationToken);
			if (response.StatusCode >= HttpStatusCode.BadRequest) {
				var errorResult = await ReadResponse<TError>(response, serializerOptions, cancellationToken);
				throw new ServiceException<TError>(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), errorResult);
			} else {
				if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0) {
					throw new ServiceException(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), "Expected content but none were returned from the service");
				}
				var result = await ReadResponse<TResponse?>(response, serializerOptions, cancellationToken);
				if (result == null) {
					throw new ServiceException(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), "Expected content but none were returned from the service");
				}
				return result.Value;
			}
		}

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
		/// <exception cref="ServiceException{TError}">Thrown when the response status code indicates an error (400+).</exception>
		public static async IAsyncEnumerable<TItem?> ExecuteAsStream<TItem, TError>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, [EnumeratorCancellation] CancellationToken cancellationToken) {
			using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			if (response.StatusCode >= HttpStatusCode.BadRequest) {
				var errorResult = await ReadResponse<TError>(response, serializerOptions, cancellationToken);
				throw new ServiceException<TError>(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), errorResult);
			}
			await foreach (var item in response.Content.ReadFromJsonAsAsyncEnumerable<TItem>(serializerOptions, cancellationToken)) {
				yield return item;
			}
		}
	}
}