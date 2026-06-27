using System.Net.Http;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilder_Reset {
		[Fact]
		public void ClearsAllConfiguredState() {
			var builder = new RequestBuilder()
				.WithMethod(HttpMethod.Post)
				.WithRelativeUrl("api")
				.AddQueryString("id", "1")
				.CreateStringRequest("body");

			builder.Reset();

			var request = builder.Build();
			Assert.Equal(HttpMethod.Get, request.Method);
			Assert.Null(request.RequestUri);
			Assert.Null(request.Content);
		}

		[Fact]
		public void ReturnsSameBuilderForChaining() {
			var builder = new RequestBuilder();
			Assert.Same(builder, builder.Reset());
		}
	}
}
