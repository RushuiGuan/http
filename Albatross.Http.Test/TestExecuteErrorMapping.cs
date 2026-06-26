using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Albatross.Http.Exceptions;
using Xunit;

namespace Albatross.Http.Test {
	public class TestExecuteErrorMapping {
		[Theory]
		[InlineData(401, typeof(Albatross.Exceptions.NotAuthenticatedException))]
		[InlineData(403, typeof(Albatross.Exceptions.ForbiddenException))]
		[InlineData(404, typeof(Albatross.Exceptions.NotFoundException))]
		[InlineData(409, typeof(Albatross.Exceptions.ConflictException))]
		[InlineData(412, typeof(Albatross.Exceptions.PreconditionFailedException))]
		[InlineData(422, typeof(Albatross.Exceptions.ValidationException))]
		public async Task RecognizedStatus_ThrowsSemanticException_CatchableAsBaseType(int status, Type expectedBaseType) {
			using var client = TestHttp.Client((HttpStatusCode)status, "error");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var ex = await Assert.ThrowsAnyAsync<Exception>(
				() => client.Execute<string, string>(request, TestHttp.Options, CancellationToken.None));

			Assert.IsAssignableFrom(expectedBaseType, ex);
			var context = Assert.IsAssignableFrom<IHttpException>(ex);
			Assert.Equal(status, context.Status);
		}

		[Theory]
		[InlineData(400)]
		[InlineData(500)]
		[InlineData(503)]
		public async Task UnrecognizedErrorStatus_ThrowsServiceException(int status) {
			using var client = TestHttp.Client((HttpStatusCode)status, "error");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var ex = await Assert.ThrowsAsync<ServiceException<string>>(
				() => client.Execute<string, string>(request, TestHttp.Options, CancellationToken.None));

			Assert.Equal(status, ((IHttpException)ex).Status);
		}

		[Fact]
		public async Task SemanticException_CarriesStatusMethodEndpointAndErrorBody() {
			using var client = TestHttp.Client(HttpStatusCode.NotFound, "{\"message\":\"missing\"}");
			using var request = new HttpRequestMessage(HttpMethod.Post, "api/widgets/5");

			var ex = await Assert.ThrowsAsync<HttpNotFoundException<ErrorBody>>(
				() => client.Execute<string, ErrorBody>(request, TestHttp.Options, CancellationToken.None));

			var context = (IHttpException)ex;
			Assert.Equal(404, context.Status);
			Assert.Equal("POST", context.Method);
			Assert.Equal("https://example.com/api/widgets/5", context.Endpoint);
			Assert.Equal("missing", ex.ErrorObject?.Message);
		}

		[Fact]
		public async Task MalformedErrorBody_StillThrowsSemanticException_WithDefaultErrorObject() {
			using var client = TestHttp.Client(HttpStatusCode.NotFound, "this is not json", "application/json");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var ex = await Assert.ThrowsAsync<HttpNotFoundException<ErrorBody>>(
				() => client.Execute<string, ErrorBody>(request, TestHttp.Options, CancellationToken.None));

			Assert.Null(ex.ErrorObject);
		}
	}
}
