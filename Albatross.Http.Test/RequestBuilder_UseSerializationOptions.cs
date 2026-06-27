using System.Text.Json;
using System.Threading.Tasks;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilder_UseSerializationOptions {
		// the supplied options replace the default camelCase policy for subsequent JSON serialization
		[Fact]
		public async Task CustomOptions_AreUsedForJsonSerialization() {
			var options = new JsonSerializerOptions { PropertyNamingPolicy = null }; // PascalCase
			var request = new RequestBuilder()
				.UseSerializationOptions(options)
				.WithRelativeUrl("api")
				.CreateJsonRequest(new Widget { Name = "gadget" })
				.Build();

			Assert.Equal("{\"Name\":\"gadget\"}", await request.Content!.ReadAsStringAsync());
		}

		[Fact]
		public void ReturnsSameBuilderForChaining() {
			var builder = new RequestBuilder();
			Assert.Same(builder, builder.UseSerializationOptions(new JsonSerializerOptions()));
		}
	}
}
