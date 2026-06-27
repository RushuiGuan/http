using System.Collections.Specialized;
using Xunit;

namespace Albatross.Http.Test {
	public class UrlExtensions_CreateUrl {
		[Fact]
		public void NullQueryString_ReturnsUrlUnchanged() {
			var result = "api/items".CreateUrl(null);
			Assert.Equal("api/items", result.ToString());
		}

		[Fact]
		public void EmptyQueryString_ReturnsUrlUnchanged() {
			var result = "api/items".CreateUrl(new NameValueCollection());
			Assert.Equal("api/items", result.ToString());
		}

		// the leading '?' / '&' separator is chosen from whatever the url already ends with
		[Theory]
		[InlineData("api/items", "api/items?id=1&")]
		[InlineData("api/items?", "api/items?id=1&")]
		[InlineData("api/items?existing=param", "api/items?existing=param&id=1&")]
		[InlineData("api/items?existing=param&", "api/items?existing=param&id=1&")]
		[InlineData(null, "?id=1&")]
		public void SingleParameter_AppendsWithCorrectSeparator(string? url, string expected) {
			var queryString = new NameValueCollection { { "id", "1" } };
			var result = url.CreateUrl(queryString);
			Assert.Equal(expected, result.ToString());
		}

		[Fact]
		public void MultipleParameters_AppendsAllInOrder() {
			var queryString = new NameValueCollection {
				{ "id", "123" },
				{ "name", "test" }
			};
			var result = "api/items".CreateUrl(queryString);
			Assert.Equal("api/items?id=123&name=test&", result.ToString());
		}

		[Fact]
		public void MultipleValuesForSameKey_AddsEachSeparately() {
			var queryString = new NameValueCollection {
				{ "id", "1" },
				{ "id", "2" },
				{ "id", "3" }
			};
			var result = "api/items".CreateUrl(queryString);
			Assert.Equal("api/items?id=1&id=2&id=3&", result.ToString());
		}

		[Fact]
		public void SpecialCharactersInKeyAndValue_AreEncoded() {
			var queryString = new NameValueCollection {
				{ "my key", "hello world" },
				{ "filter", "a&b=c" }
			};
			var result = "api/search".CreateUrl(queryString);
			Assert.Equal("api/search?my%20key=hello%20world&filter=a%26b%3Dc&", result.ToString());
		}

		// the returned StringBuilder is the live buffer, so callers can keep appending
		[Fact]
		public void ReturnsMutableStringBuilder() {
			var queryString = new NameValueCollection { { "id", "1" } };
			var result = "api/items".CreateUrl(queryString);
			result.Append("extra=value&");
			Assert.Equal("api/items?id=1&extra=value&", result.ToString());
		}
	}
}
