using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilder_AddStringToMultipartFormData {
		[Fact]
		public async Task StringField_CreatesMultipartContentWithFieldValue() {
			var request = new RequestBuilder().WithRelativeUrl("api")
				.AddStringToMultipartFormData("name", "alice")
				.Build();

			var parts = Assert.IsType<MultipartFormDataContent>(request.Content).ToList();
			Assert.Single(parts);
			Assert.Equal("alice", await parts[0].ReadAsStringAsync());
		}

		[Fact]
		public void MultipleFields_AreAllAddedToSameMultipartContent() {
			var request = new RequestBuilder().WithRelativeUrl("api")
				.AddStringToMultipartFormData("a", "1")
				.AddStringToMultipartFormData("b", "2")
				.Build();

			var parts = Assert.IsType<MultipartFormDataContent>(request.Content).ToList();
			Assert.Equal(2, parts.Count);
		}

		[Fact]
		public void ExistingNonMultipartContent_ThrowsInvalidOperation() {
			var builder = new RequestBuilder().WithRelativeUrl("api").CreateStringRequest("text");
			Assert.Throws<InvalidOperationException>(() => builder.AddStringToMultipartFormData("name", "alice"));
		}
	}
}
