using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Albatross.Http;
using Albatross.Http.Exceptions;
using Xunit;

namespace Albatross.Http.Test {
	public class HttpClientExtensions_Execute {
		[Fact]
		public async Task SuccessWithJsonBody_DeserializesResponse() {
			using var client = TestHttp.Client(HttpStatusCode.OK, "{\"name\":\"gadget\"}");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var result = await client.Execute<Widget>(request, TestHttp.Options, CancellationToken.None);

			Assert.Equal("gadget", result!.Name);
		}

		// requesting string returns the raw response body rather than attempting JSON deserialization
		[Fact]
		public async Task StringResponseType_ReturnsRawBody() {
			using var client = TestHttp.Client(HttpStatusCode.OK, "hello world");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var result = await client.Execute<string>(request, TestHttp.Options, CancellationToken.None);

			Assert.Equal("hello world", result);
		}

		// 204 and zero-length successful responses yield the default value
		[Theory]
		[InlineData(HttpStatusCode.NoContent)]
		[InlineData(HttpStatusCode.OK)]
		public async Task NoResponseBody_ReturnsNull(HttpStatusCode status) {
			using var client = TestHttp.Client(status, null);
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var result = await client.Execute<Widget>(request, TestHttp.Options, CancellationToken.None);

			Assert.Null(result);
		}

		[Fact]
		public async Task MalformedJsonBody_ReturnsDefault() {
			using var client = TestHttp.Client(HttpStatusCode.OK, "this is not json");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var result = await client.Execute<Widget>(request, TestHttp.Options, CancellationToken.None);

			Assert.Null(result);
		}

		// each recognized error status maps to its dedicated semantic exception
		[Theory]
		[InlineData(400, typeof(HttpArgumentException))]
		[InlineData(401, typeof(HttpNotAuthenticatedException))]
		[InlineData(403, typeof(HttpForbiddenException))]
		[InlineData(404, typeof(HttpNotFoundException))]
		[InlineData(408, typeof(HttpTimeoutException))]
		[InlineData(409, typeof(HttpConflictException))]
		[InlineData(412, typeof(HttpPreconditionFailedException))]
		[InlineData(422, typeof(HttpValidationException))]
		[InlineData(501, typeof(HttpNotSupportedException))]
		public async Task RecognizedErrorStatus_ThrowsMatchingSemanticException(int status, Type expectedType) {
			using var client = TestHttp.Client((HttpStatusCode)status, "error");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var ex = await Assert.ThrowsAnyAsync<Exception>(
				() => client.Execute<Widget>(request, TestHttp.Options, CancellationToken.None));

			Assert.IsType(expectedType, ex);
			Assert.Equal(status, ((IHttpException)ex).Status);
		}

		[Theory]
		[InlineData(500)]
		[InlineData(503)]
		public async Task UnrecognizedErrorStatus_ThrowsServiceException(int status) {
			using var client = TestHttp.Client((HttpStatusCode)status, "error");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var ex = await Assert.ThrowsAsync<ServiceException>(
				() => client.Execute<Widget>(request, TestHttp.Options, CancellationToken.None));

			Assert.Equal(status, ((IHttpException)ex).Status);
		}

		// the thrown exception carries the request method, resolved endpoint, and raw error body
		[Fact]
		public async Task ErrorResponse_ExceptionCarriesRequestContextAndBody() {
			using var client = TestHttp.Client(HttpStatusCode.NotFound, "{\"message\":\"missing\"}");
			using var request = new HttpRequestMessage(HttpMethod.Post, "api/widgets/5");

			var ex = await Assert.ThrowsAsync<HttpNotFoundException>(
				() => client.Execute<Widget>(request, TestHttp.Options, CancellationToken.None));

			var context = (IHttpException)ex;
			Assert.Equal(404, context.Status);
			Assert.Equal("POST", context.Method);
			Assert.Equal("https://example.com/api/widgets/5", context.Endpoint);
			Assert.Equal("application/json", context.ContentType);
			Assert.Equal("{\"message\":\"missing\"}", context.Content);
		}
	}
}
