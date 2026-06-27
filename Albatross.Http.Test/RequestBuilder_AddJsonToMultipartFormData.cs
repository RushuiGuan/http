using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilder_AddJsonToMultipartFormData {
		[Fact]
		public async Task JsonField_CreatesMultipartContentWithJsonPart() {
			var request = new RequestBuilder().WithRelativeUrl("api")
				.AddJsonToMultipartFormData("widget", new Widget { Name = "gadget" })
				.Build();

			var parts = Assert.IsType<MultipartFormDataContent>(request.Content).ToList();
			Assert.Single(parts);
			Assert.Equal("application/json", parts[0].Headers.ContentType!.MediaType);
			Assert.Equal("{\"name\":\"gadget\"}", await parts[0].ReadAsStringAsync());
		}

		[Fact]
		public void ExistingNonMultipartContent_ThrowsInvalidOperation() {
			var builder = new RequestBuilder().WithRelativeUrl("api").CreateStringRequest("text");
			Assert.Throws<InvalidOperationException>(() => builder.AddJsonToMultipartFormData("widget", new Widget()));
		}
	}
}
