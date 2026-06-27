using System.Threading.Tasks;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilder_CreateStringRequest {
		[Fact]
		public async Task Text_SetsPlainTextContent() {
			var request = new RequestBuilder().WithRelativeUrl("api").CreateStringRequest("hello").Build();

			Assert.Equal("text/plain", request.Content!.Headers.ContentType!.MediaType);
			Assert.Equal("hello", await request.Content.ReadAsStringAsync());
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		public void NullOrEmptyText_ProducesNoContent(string? text) {
			var request = new RequestBuilder().WithRelativeUrl("api").CreateStringRequest(text).Build();
			Assert.Null(request.Content);
		}

		[Fact]
		public void ReturnsSameBuilderForChaining() {
			var builder = new RequestBuilder();
			Assert.Same(builder, builder.CreateStringRequest("x"));
		}
	}
}
