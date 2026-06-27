using System;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilder_AddQueryString {
		[Fact]
		public void SingleParameter_AppearsInUrl() {
			var request = new RequestBuilder().WithRelativeUrl("api").AddQueryString("id", "1").Build();
			Assert.Equal("api?id=1", request.RequestUri!.ToString());
		}

		[Fact]
		public void MultipleParameters_AppearInOrder() {
			var request = new RequestBuilder().WithRelativeUrl("api")
				.AddQueryString("id", "1")
				.AddQueryString("name", "test")
				.Build();
			Assert.Equal("api?id=1&name=test", request.RequestUri!.ToString());
		}

		// repeating the same key produces repeated query parameters
		[Fact]
		public void RepeatedKey_ProducesRepeatedParameters() {
			var request = new RequestBuilder().WithRelativeUrl("api")
				.AddQueryString("id", "1")
				.AddQueryString("id", "2")
				.Build();
			Assert.Equal("api?id=1&id=2", request.RequestUri!.ToString());
		}

		[Fact]
		public void ReturnsSameBuilderForChaining() {
			var builder = new RequestBuilder();
			Assert.Same(builder, builder.AddQueryString("id", "1"));
		}
	}
}
