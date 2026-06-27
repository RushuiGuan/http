using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Albatross.Http;
using Albatross.Http.Exceptions;
using Xunit;

namespace Albatross.Http.Test {
	public class HttpClientExtensions_Send {
		// Send discards the body; a successful status simply completes without throwing
		[Theory]
		[InlineData(HttpStatusCode.OK)]
		[InlineData(HttpStatusCode.NoContent)]
		public async Task SuccessStatus_CompletesWithoutThrowing(HttpStatusCode status) {
			using var client = TestHttp.Client(status, "{\"name\":\"ignored\"}");
			using var request = new HttpRequestMessage(HttpMethod.Post, "api/test");

			await client.Send(request, TestHttp.Options, CancellationToken.None);
		}

		[Theory]
		[InlineData(404, typeof(HttpNotFoundException))]
		[InlineData(409, typeof(HttpConflictException))]
		[InlineData(500, typeof(ServiceException))]
		public async Task ErrorStatus_ThrowsSemanticException(int status, Type expectedType) {
			using var client = TestHttp.Client((HttpStatusCode)status, "error");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var ex = await Assert.ThrowsAnyAsync<Exception>(
				() => client.Send(request, TestHttp.Options, CancellationToken.None));

			Assert.IsType(expectedType, ex);
			Assert.Equal(status, ((IHttpException)ex).Status);
		}
	}
}
