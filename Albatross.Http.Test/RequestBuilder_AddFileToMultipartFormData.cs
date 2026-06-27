using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilder_AddFileToMultipartFormData {
		[Fact]
		public void ByteArrayFile_CreatesMultipartContentWithFilePart() {
			var request = new RequestBuilder().WithRelativeUrl("api")
				.AddFileToMultipartFormData("file", "a.png", new byte[] { 1, 2, 3 }, "image/png")
				.Build();

			var parts = Assert.IsType<MultipartFormDataContent>(request.Content).ToList();
			Assert.Single(parts);
			Assert.Equal("image/png", parts[0].Headers.ContentType!.MediaType);
		}

		[Fact]
		public void StreamFile_CreatesMultipartContentWithFilePart() {
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes("data"));
			var request = new RequestBuilder().WithRelativeUrl("api")
				.AddFileToMultipartFormData("file", "a.png", stream, "image/png")
				.Build();

			var parts = Assert.IsType<MultipartFormDataContent>(request.Content).ToList();
			Assert.Single(parts);
			Assert.Equal("image/png", parts[0].Headers.ContentType!.MediaType);
		}

		// repeated calls accumulate into a single multipart payload
		[Fact]
		public void MultipleFiles_AreAllAddedToSameMultipartContent() {
			var request = new RequestBuilder().WithRelativeUrl("api")
				.AddFileToMultipartFormData("file1", "a.png", new byte[] { 1 }, "image/png")
				.AddFileToMultipartFormData("file2", "b.png", new byte[] { 2 }, "image/png")
				.Build();

			var parts = Assert.IsType<MultipartFormDataContent>(request.Content).ToList();
			Assert.Equal(2, parts.Count);
		}

		// adding a file when the request already holds non-multipart content is a contract violation
		[Fact]
		public void ExistingNonMultipartContent_ThrowsInvalidOperation() {
			var builder = new RequestBuilder().WithRelativeUrl("api").CreateStringRequest("text");
			Assert.Throws<InvalidOperationException>(
				() => builder.AddFileToMultipartFormData("file", "a.png", new byte[] { 1 }, "image/png"));
		}
	}
}
