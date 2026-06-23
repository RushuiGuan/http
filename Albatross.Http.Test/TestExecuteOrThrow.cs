using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Albatross.Http.Exceptions;
using Xunit;

namespace Albatross.Http.Test {
	public class TestExecuteOrThrow {
		[Theory]
		[InlineData(HttpStatusCode.NoContent)]
		[InlineData(HttpStatusCode.OK)]
		public async Task NoContent_ThrowsMissingRequiredValue(HttpStatusCode status) {
			using var client = TestHttp.Client(status, null);
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var ex = await Assert.ThrowsAsync<MissingRequiredValueException<Widget>>(
				() => client.ExecuteOrThrow<Widget, string>(request, TestHttp.Options, CancellationToken.None));

			Assert.Equal((int)status, ((IServiceException)ex).StatusCode);
		}

		[Fact]
		public async Task NullBody_ThrowsMissingRequiredValue() {
			using var client = TestHttp.Client(HttpStatusCode.OK, "null");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			await Assert.ThrowsAsync<MissingRequiredValueException<Widget>>(
				() => client.ExecuteOrThrow<Widget, string>(request, TestHttp.Options, CancellationToken.None));
		}

		[Fact]
		public async Task Success_ReturnsDeserializedValue() {
			using var client = TestHttp.Client(HttpStatusCode.OK, "{\"name\":\"gadget\"}");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var result = await client.ExecuteOrThrow<Widget, string>(request, TestHttp.Options, CancellationToken.None);

			Assert.Equal("gadget", result.Name);
		}
	}
}
