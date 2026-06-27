using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilder_CreateStreamRequest {
		[Fact]
		public async Task Stream_BecomesRequestContent() {
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes("payload"));
			var request = new RequestBuilder().WithRelativeUrl("api").CreateStreamRequest(stream).Build();

			Assert.IsType<StreamContent>(request.Content);
			Assert.Equal("payload", await request.Content!.ReadAsStringAsync());
		}

		[Fact]
		public void ReturnsSameBuilderForChaining() {
			using var stream = new MemoryStream();
			var builder = new RequestBuilder();
			Assert.Same(builder, builder.CreateStreamRequest(stream));
		}
	}
}
