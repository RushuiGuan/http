using System;
using System.Collections.Specialized;
using System.Linq;
using Xunit;

namespace Albatross.Http.Test {
	public class UrlExtensions_CreateUrlArrayByDelimitedValue {
		static NameValueCollection DateQuery() => new NameValueCollection { { "date", "2022-10-10" } };

		// values are joined into a single parameter by the delimiter (id=0,1), splitting into a new url when the
		// next value would push the total past maxUrlLength. The delimiter is url-encoded ('.' stays, ',' becomes %2C).
		[Theory]
		[InlineData(50, ".", "api/bar?date=2022-10-10&id=0.1")]
		[InlineData(52, ".", "api/bar?date=2022-10-10&id=0.1")]
		[InlineData(52, ",", "api/bar?date=2022-10-10&id=0%2C1")]
		[InlineData(48, ",", "api/bar?date=2022-10-10&id=0%2C1")]
		[InlineData(47, ",", "api/bar?date=2022-10-10&id=0", "api/bar?date=2022-10-10&id=1")]
		[InlineData(44, ",", "api/bar?date=2022-10-10&id=0", "api/bar?date=2022-10-10&id=1")]
		public void JoinsWithDelimiterAndSplitsAtMaxLength(int maxLength, string delimiter, params string[] expected) {
			var baseUri = new Uri("http://myyyhost");
			var requests = UrlExtensions.CreateUrlArrayByDelimitedValue(baseUri, "api/bar", DateQuery(), maxLength, "id", delimiter, "0", "1").ToArray();
			Assert.Equal(expected, requests);
		}

		[Theory]
		[InlineData(2)]
		[InlineData(43)]
		public void MaxLengthTooSmallForAnyValue_ThrowsArgumentException(int maxLength) {
			var baseUri = new Uri("http://myyyhost");
			Assert.Throws<ArgumentException>(
				() => UrlExtensions.CreateUrlArrayByDelimitedValue(baseUri, "api/bar", DateQuery(), maxLength, "id", ",", "0", "1"));
		}

		[Fact]
		public void ValueTooLongMidArray_ThrowsArgumentException() {
			var baseUri = new Uri("http://myyyhost");
			var values = new[] { "0", "this-value-is-way-too-long-to-fit-in-the-url" };
			Assert.Throws<ArgumentException>(
				() => UrlExtensions.CreateUrlArrayByDelimitedValue(baseUri, "api/bar", DateQuery(), 50, "id", ",", values).ToArray());
		}
	}
}
