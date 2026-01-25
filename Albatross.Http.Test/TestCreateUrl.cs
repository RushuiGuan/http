using System.Collections.Specialized;
using Xunit;

namespace Albatross.Http.Test {
	public class TestCreateUrl {
		[Fact]
		public void WithNullQueryString_ReturnsUrlOnly() {
			var result = "api/items".CreateUrl(null);
			Assert.Equal("api/items", result.ToString());
		}

		[Fact]
		public void WithEmptyQueryString_ReturnsUrlOnly() {
			var result = "api/items".CreateUrl(new NameValueCollection());
			Assert.Equal("api/items", result.ToString());
		}

		[Fact]
		public void WithSingleParameter_AppendsQueryString() {
			var queryString = new NameValueCollection {
				{ "id", "123" }
			};
			var result = "api/items".CreateUrl(queryString);
			Assert.Equal("api/items?id=123&", result.ToString());
		}

		[Fact]
		public void WithMultipleParameters_AppendsAllParameters() {
			var queryString = new NameValueCollection {
				{ "id", "123" },
				{ "name", "test" }
			};
			var result = "api/items".CreateUrl(queryString);
			Assert.Equal("api/items?id=123&name=test&", result.ToString());
		}

		[Fact]
		public void WithSpecialCharacters_EncodesValues() {
			var queryString = new NameValueCollection {
				{ "q", "hello world" },
				{ "filter", "a&b=c" }
			};
			var result = "api/search".CreateUrl(queryString);
			Assert.Equal("api/search?q=hello%20world&filter=a%26b%3Dc&", result.ToString());
		}

		[Fact]
		public void WithSpecialCharactersInKey_EncodesKey() {
			var queryString = new NameValueCollection {
				{ "my key", "value" }
			};
			var result = "api/items".CreateUrl(queryString);
			Assert.Equal("api/items?my%20key=value&", result.ToString());
		}

		[Fact]
		public void WithMultipleValuesForSameKey_AddsEachValueSeparately() {
			var queryString = new NameValueCollection {
				{ "id", "1" },
				{ "id", "2" },
				{ "id", "3" }
			};
			var result = "api/items".CreateUrl(queryString);
			Assert.Equal("api/items?id=1&id=2&id=3&", result.ToString());
		}

		[Fact]
		public void ReturnsStringBuilder_AllowsFurtherModification() {
			var queryString = new NameValueCollection {
				{ "id", "1" }
			};
			var result = "api/items".CreateUrl(queryString);
			result.Append("extra=value&");
			Assert.Equal("api/items?id=1&extra=value&", result.ToString());
		}

		[Fact]
		public void WithUrlEndingWithQuestionMark_AppendsWithoutExtraQuestionMark() {
			var queryString = new NameValueCollection {
				{ "id", "1" }
			};
			var result = "api/items?".CreateUrl(queryString);
			Assert.Equal("api/items?id=1&", result.ToString());
		}

		[Fact]
		public void WithUrlContainingExistingQueryString_AppendsWithAmpersand() {
			var queryString = new NameValueCollection {
				{ "id", "1" }
			};
			var result = "api/items?existing=param".CreateUrl(queryString);
			Assert.Equal("api/items?existing=param&id=1&", result.ToString());
		}

		[Fact]
		public void WithUrlEndingWithAmpersand_AppendsWithoutExtraAmpersand() {
			var queryString = new NameValueCollection {
				{ "id", "1" }
			};
			var result = "api/items?existing=param&".CreateUrl(queryString);
			Assert.Equal("api/items?existing=param&id=1&", result.ToString());
		}
	}
}
