using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class HttpResponseContent_IsJson {
		// recognized as JSON: the bare media type, a +json structured suffix, any casing, and full header
		// values carrying parameters after a ';'
		[Theory]
		[InlineData("application/json")]
		[InlineData("application/json; charset=utf-8")]
		[InlineData("APPLICATION/JSON")]
		[InlineData("application/problem+json")]
		[InlineData("application/problem+json; charset=utf-8")]
		[InlineData("application/vnd.api+json")]
		[InlineData("  application/json  ")]
		public void JsonContentTypes_ReturnTrue(string contentType) {
			var content = new HttpResponseContent { ContentType = contentType };
			Assert.True(content.IsJson());
		}

		[Theory]
		[InlineData("text/plain")]
		[InlineData("application/xml")]
		[InlineData("application/jsonx")]
		[InlineData("application/json-patch")]
		[InlineData("json")]
		[InlineData("")]
		[InlineData(null)]
		public void NonJsonContentTypes_ReturnFalse(string? contentType) {
			var content = new HttpResponseContent { ContentType = contentType };
			Assert.False(content.IsJson());
		}
	}
}
