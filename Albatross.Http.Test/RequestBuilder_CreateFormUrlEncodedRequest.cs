using System.Collections.Generic;
using System.Threading.Tasks;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilder_CreateFormUrlEncodedRequest {
		[Fact]
		public async Task Values_SetFormUrlEncodedContent() {
			var values = new Dictionary<string, string> { { "a", "1" }, { "b", "2" } };
			var request = new RequestBuilder().WithRelativeUrl("api").CreateFormUrlEncodedRequest(values).Build();

			Assert.Equal("application/x-www-form-urlencoded", request.Content!.Headers.ContentType!.MediaType);
			Assert.Equal("a=1&b=2", await request.Content.ReadAsStringAsync());
		}

		[Fact]
		public void ReturnsSameBuilderForChaining() {
			var builder = new RequestBuilder();
			Assert.Same(builder, builder.CreateFormUrlEncodedRequest(new Dictionary<string, string>()));
		}
	}
}
