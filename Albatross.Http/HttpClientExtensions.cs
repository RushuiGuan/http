using Albatross.Http;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Albatross.WebClient {
	public static class HttpClientExtensions {
		public static async Task<ResponseType?> Execute<ResponseType, ResponseErrorType>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions) {
			var response = await client.SendAsync(request);
			if ((int)response.StatusCode >= 400) {
				var errorResult = await response.ReadResponseAsJson<ResponseErrorType>(serializerOptions);
				throw new ServiceException<ResponseErrorType>(response.StatusCode, request.Method, request.RequestUri, errorResult);
			} else {
				var result = await response.ReadResponseAsJson<ResponseType>(serializerOptions);
				return result;
			}
		}

		public static async Task<ResponseType?> ExecuteMixedContentType<ResponseType, ResponseErrorType>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions) {
			var response = await client.SendAsync(request);
			if ((int)response.StatusCode >= 400) {
				var result = await response.ReadResponseAuto<ResponseErrorType>(serializerOptions);
				throw new ServiceException<ResponseErrorType>(response.StatusCode, request.Method, request.RequestUri, result);
			} else {
				var result = await response.ReadResponseAuto<ResponseType>(serializerOptions);
				return result;
			}
		}
	}
}