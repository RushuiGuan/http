using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.Http.Test {
	public class Widget {
		public string? Name { get; set; }
	}

	public class ErrorBody {
		public string? Message { get; set; }
	}

	/// <summary>
	/// Returns a fixed response so the HttpClientExtensions methods can be exercised without a real server.
	/// </summary>
	internal sealed class StubHttpMessageHandler : HttpMessageHandler {
		readonly HttpStatusCode statusCode;
		readonly string? content;
		readonly string contentType;

		public StubHttpMessageHandler(HttpStatusCode statusCode, string? content, string contentType = "application/json") {
			this.statusCode = statusCode;
			this.content = content;
			this.contentType = contentType;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			var response = new HttpResponseMessage(statusCode) { RequestMessage = request };
			if (content == null) {
				var empty = new StringContent(string.Empty);
				empty.Headers.ContentLength = 0;
				response.Content = empty;
			} else {
				response.Content = new StringContent(content, Encoding.UTF8, contentType);
			}
			return Task.FromResult(response);
		}
	}

	internal static class TestHttp {
		public static readonly JsonSerializerOptions Options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

		public static HttpClient Client(HttpStatusCode status, string? content, string contentType = "application/json")
			=> new HttpClient(new StubHttpMessageHandler(status, content, contentType)) {
				BaseAddress = new Uri("https://example.com/")
			};
	}
}
