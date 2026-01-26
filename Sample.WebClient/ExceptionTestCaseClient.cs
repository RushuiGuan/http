using Albatross.Http;
using Albatross.WebClient;
using System.Collections.Specialized;
using System.Text.Json;

namespace Sample.WebClient {
	public class ExceptionTestCaseClient {
		private readonly HttpClient client;
		private readonly JsonSerializerOptions serializerOptions;

		public ExceptionTestCaseClient(HttpClient client) {
			this.client = client;
			this.serializerOptions = new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			};
		}

		public async Task<string?> SendViaProblemMethod(CancellationToken cancellationToken) {
			using var request = RequestExtensions.CreateRequest(HttpMethod.Get, "/api/ExceptionTestCase/send-via-controllerBase.problem-method", new NameValueCollection());
			return await client.Execute<string>(request, serializerOptions, cancellationToken);
		}

		public async Task<string?> ThrowException(CancellationToken cancellationToken) {
			using var request = RequestExtensions.CreateRequest(HttpMethod.Get, "/api/ExceptionTestCase/send-by-throwing-exception", new NameValueCollection());
			return await client.Execute<string>(request, serializerOptions, cancellationToken);
		}

		public async Task<string?> ThrowTextHttpApiException(CancellationToken cancellationToken) {
			using var request = RequestExtensions.CreateRequest(HttpMethod.Get, "/api/ExceptionTestCase/send-by-throwing-text-http-api-exception", new NameValueCollection());
			return await client.Execute<string>(request, serializerOptions, cancellationToken);
		}

		public async Task<string?> ThrowJsonHttpApiException(CancellationToken cancellationToken) {
			using var request = RequestExtensions.CreateRequest(HttpMethod.Get, "/api/ExceptionTestCase/send-by-throwing-json-http-api-exception", new NameValueCollection());
			return await client.Execute<string>(request, serializerOptions, cancellationToken);
		}

		public async Task<string?> ThrowArgumentException(CancellationToken cancellationToken) {
			using var request = RequestExtensions.CreateRequest(HttpMethod.Get, "/api/ExceptionTestCase/send-by-throwing-argument-exception", new NameValueCollection());
			return await client.Execute<string>(request, serializerOptions, cancellationToken);
		}

		public async Task<string?> ThrowExceptionWithInnerException(CancellationToken cancellationToken) {
			using var request = RequestExtensions.CreateRequest(HttpMethod.Get, "/api/ExceptionTestCase/send-by-throwing-exception-with-inner-exception", new NameValueCollection());
			return await client.Execute<string>(request, serializerOptions, cancellationToken);
		}

		public async Task<string?> ThrowAfterAsyncEnumerable(CancellationToken cancellationToken) {
			using var request = RequestExtensions.CreateRequest(HttpMethod.Get, "/api/ExceptionTestCase/throw-after-async-enumerable", new NameValueCollection());
			return await client.Execute<string>(request, serializerOptions, cancellationToken);
		}

		public async Task<string?> ThrowAfterYieldReturn(CancellationToken cancellationToken) {
			using var request = RequestExtensions.CreateRequest(HttpMethod.Get, "/api/ExceptionTestCase/throw-after-yield-return", new NameValueCollection());
			return await client.Execute<string>(request, serializerOptions, cancellationToken);
		}
	}
}
