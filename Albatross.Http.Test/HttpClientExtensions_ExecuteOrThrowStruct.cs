using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Albatross.Http;
using Albatross.Http.Exceptions;
using Xunit;

namespace Albatross.Http.Test {
	public class HttpClientExtensions_ExecuteOrThrowStruct {
		[Fact]
		public async Task SuccessWithBody_ReturnsDeserializedValue() {
			using var client = TestHttp.Client(HttpStatusCode.OK, "42");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var result = await client.ExecuteOrThrowStruct<int>(request, TestHttp.Options, CancellationToken.None);

			Assert.Equal(42, result);
		}

		// a required value type with no body (204 / zero-length) is a contract violation
		[Theory]
		[InlineData(HttpStatusCode.NoContent)]
		[InlineData(HttpStatusCode.OK)]
		public async Task NoResponseBody_ThrowsMissingRequiredValue(HttpStatusCode status) {
			using var client = TestHttp.Client(status, null);
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var ex = await Assert.ThrowsAsync<MissingRequiredValueException<int>>(
				() => client.ExecuteOrThrowStruct<int>(request, TestHttp.Options, CancellationToken.None));

			Assert.Equal((int)status, ((IHttpException)ex).Status);
		}

		// a JSON null deserializes to a null nullable, which is treated as a missing value
		[Fact]
		public async Task NullJsonBody_ThrowsMissingRequiredValue() {
			using var client = TestHttp.Client(HttpStatusCode.OK, "null");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			await Assert.ThrowsAsync<MissingRequiredValueException<int>>(
				() => client.ExecuteOrThrowStruct<int>(request, TestHttp.Options, CancellationToken.None));
		}

		[Fact]
		public async Task ErrorStatus_ThrowsSemanticException() {
			using var client = TestHttp.Client(HttpStatusCode.NotFound, "error");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			await Assert.ThrowsAsync<HttpNotFoundException>(
				() => client.ExecuteOrThrowStruct<int>(request, TestHttp.Options, CancellationToken.None));
		}
	}
}
