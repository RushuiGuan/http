using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilder_WithRelativeUrl {
		[Theory]
		[InlineData("api/items")]
		[InlineData("api/widgets/5")]
		[InlineData("")]
		public void SetsRequestUri(string url) {
			var request = new RequestBuilder().WithRelativeUrl(url).Build();
			// an empty relative url produces a null RequestUri
			Assert.Equal(string.IsNullOrEmpty(url) ? null : url, request.RequestUri?.ToString());
		}

		[Fact]
		public void ReturnsSameBuilderForChaining() {
			var builder = new RequestBuilder();
			Assert.Same(builder, builder.WithRelativeUrl("api"));
		}
	}
}
