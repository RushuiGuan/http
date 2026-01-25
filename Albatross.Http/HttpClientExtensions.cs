using Albatross.Http;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Albatross.WebClient {
	public static class HttpClientExtensions {
		public static async Task<ResponseType?> ExecuteAsJson<ResponseType, ResponseErrorType>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions) {
			var response = await client.SendAsync(request);
			if (response.StatusCode >= HttpStatusCode.BadRequest) {
				var errorResult = await response.ReadResponseAsJson<ResponseErrorType>(serializerOptions);
				throw new ServiceException<ResponseErrorType>(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), errorResult);
			} else {
				var result = await response.ReadResponseAsJson<ResponseType>(serializerOptions);
				return result;
			}
		}

		public static async Task<ResponseType?> Execute<ResponseType, ResponseErrorType>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions) {
			var response = await client.SendAsync(request);
			if (response.StatusCode >= HttpStatusCode.BadRequest) {
				var result = await response.ReadResponse<ResponseErrorType>(serializerOptions);
				throw new ServiceException<ResponseErrorType>(response.StatusCode, request.Method, request.GetFullUri(client.BaseAddress), result);
			} else {
				var result = await response.ReadResponse<ResponseType>(serializerOptions);
				return result;
			}
		}
	}
}