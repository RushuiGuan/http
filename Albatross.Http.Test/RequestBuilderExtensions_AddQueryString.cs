using System;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilderExtensions_AddQueryString {
		// the generic overload stringifies any non-null value via ToString() and appends it as a query parameter
		[Theory]
		[InlineData(5, "api?id=5")]
		[InlineData(true, "api?id=True")]
		[InlineData("abc", "api?id=abc")]
		[InlineData(1.5, "api?id=1.5")]
		public void AppendsStringifiedValue<T>(T value, string expected) where T : notnull {
			var request = new RequestBuilder().WithRelativeUrl("api").AddQueryString("id", value).Build();
			Assert.Equal(expected, Uri.UnescapeDataString(request.RequestUri!.ToString()));
		}

		[Fact]
		public void ReturnsSameBuilderForChaining() {
			var builder = new RequestBuilder();
			Assert.Same(builder, builder.AddQueryString("id", 1));
		}
	}
}
