using Albatross.Http;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.WebClient {
	public static class HttpClientExtensions {
		public static Task<ResponseType?> Execute<ResponseType>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken)
			=> Execute<ResponseType, string>(client, request, serializerOptions, cancellationToken);

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

		public static async Task<ResponseType?> Execute<ResponseType, ResponseErrorType>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken) {
			using var response = await client.SendAsync(request, cancellationToken);
			if (response.StatusCode >= HttpStatusCode.BadRequest) {
				var errorResult = await ReadResponse<ResponseErrorType>(response, serializerOptions, cancellationToken);
				throw new ServiceException<ResponseErrorType>(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), errorResult);
			} else {
				var result = await ReadResponse<ResponseType>(response, serializerOptions, cancellationToken);
				return result;
			}
		}
	}
}