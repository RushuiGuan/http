using System.Threading.Tasks;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilder_CreateJsonRequest {
		[Fact]
		public async Task Object_SetsJsonContentTypeAndCamelCaseBody() {
			var request = new RequestBuilder().WithRelativeUrl("api").CreateJsonRequest(new Widget { Name = "gadget" }).Build();

			Assert.Equal("application/json", request.Content!.Headers.ContentType!.MediaType);
			Assert.Equal("{\"name\":\"gadget\"}", await request.Content.ReadAsStringAsync());
		}

		// the default serializer options omit null properties
		[Fact]
		public async Task ObjectWithNullProperty_OmitsTheProperty() {
			var request = new RequestBuilder().WithRelativeUrl("api").CreateJsonRequest(new Widget { Name = null }).Build();
			Assert.Equal("{}", await request.Content!.ReadAsStringAsync());
		}

		[Fact]
		public void NullObject_ProducesNoContent() {
			var request = new RequestBuilder().WithRelativeUrl("api").CreateJsonRequest<Widget>(null).Build();
			Assert.Null(request.Content);
		}

		[Fact]
		public void ReturnsSameBuilderForChaining() {
			var builder = new RequestBuilder();
			Assert.Same(builder, builder.CreateJsonRequest(new Widget()));
		}
	}
}
