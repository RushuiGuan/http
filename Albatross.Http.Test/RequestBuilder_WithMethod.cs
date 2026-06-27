using System.Net.Http;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilder_WithMethod {
		[Theory]
		[InlineData("GET")]
		[InlineData("POST")]
		[InlineData("PUT")]
		[InlineData("DELETE")]
		public void SetsRequestMethod(string method) {
			var request = new RequestBuilder().WithMethod(new HttpMethod(method)).WithRelativeUrl("api").Build();
			Assert.Equal(new HttpMethod(method), request.Method);
		}

		[Fact]
		public void ReturnsSameBuilderForChaining() {
			var builder = new RequestBuilder();
			Assert.Same(builder, builder.WithMethod(HttpMethod.Post));
		}
	}
}
