using System.Net.Http;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilder_Build {
		[Fact]
		public void DefaultsToGetMethod() {
			var request = new RequestBuilder().WithRelativeUrl("api").Build();
			Assert.Equal(HttpMethod.Get, request.Method);
		}

		// CreateUrl leaves a trailing '&' on the query string which Build trims off
		[Fact]
		public void TrailingAmpersand_IsRemovedFromUrl() {
			var request = new RequestBuilder().WithRelativeUrl("api").AddQueryString("id", "1").Build();
			Assert.Equal("api?id=1", request.RequestUri!.ToString());
		}

		// Build resets the builder, so a second Build returns a fresh default request
		[Fact]
		public void ResetsBuilderAfterBuild() {
			var builder = new RequestBuilder()
				.WithMethod(HttpMethod.Post)
				.WithRelativeUrl("api")
				.AddQueryString("id", "1")
				.CreateStringRequest("body");
			builder.Build();

			var second = builder.Build();
			Assert.Equal(HttpMethod.Get, second.Method);
			Assert.Null(second.RequestUri);
			Assert.Null(second.Content);
		}
	}
}
