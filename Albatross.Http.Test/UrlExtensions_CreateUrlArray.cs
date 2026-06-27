using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xunit;

namespace Albatross.Http.Test {
	public class UrlExtensions_CreateUrlArray {
		static NameValueCollection DateQuery() => new NameValueCollection { { "date", "2022-10-10" } };

		// values are emitted as repeated query parameters (id=0&id=1&...), splitting into a new url when the next
		// value would push the total past maxUrlLength
		[Theory]
		[InlineData(45, "api/bar?date=2022-10-10&id=0&", "api/bar?date=2022-10-10&id=1&")]
		[InlineData(49, "api/bar?date=2022-10-10&id=0&", "api/bar?date=2022-10-10&id=1&")]
		[InlineData(50, "api/bar?date=2022-10-10&id=0&id=1&")]
		[InlineData(52, "api/bar?date=2022-10-10&id=0&id=1&")]
		public void SplitsAcrossUrlsAtMaxLength(int maxLength, params string[] expected) {
			var baseUri = new Uri("http://myyyhost");
			var requests = UrlExtensions.CreateUrlArray(baseUri, "api/bar", DateQuery(), maxLength, "id", "0", "1").ToArray();
			Assert.Equal(expected, requests);
		}

		[Fact]
		public void AllValuesFitWithinMaxLength_ReturnsSingleUrl() {
			var baseUri = new Uri("http://myyyhost/mmmyyy-data");
			var ids = Enumerable.Range(0, 50).Select(i => i.ToString()).ToArray();
			var requests = UrlExtensions.CreateUrlArray(baseUri, "api/w9-bar", DateQuery(), 2000, "id", ids);
			Assert.Single(requests);
		}

		[Fact]
		public void ManyValues_SplitIntoMultipleUrls() {
			var baseUri = new Uri("http://myyyhost/mmmyyy-data");
			var ids = Enumerable.Range(0, 1000).Select(i => i.ToString()).ToArray();
			var requests = UrlExtensions.CreateUrlArray(baseUri, "api/w9-bar", DateQuery(), 2000, "id", ids);
			Assert.Equal(4, requests.Count());
		}

		[Theory]
		[InlineData(2)]
		[InlineData(44)]
		public void MaxLengthTooSmallForAnyValue_ThrowsArgumentException(int maxLength) {
			var baseUri = new Uri("http://myyyhost");
			Assert.Throws<ArgumentException>(
				() => UrlExtensions.CreateUrlArray(baseUri, "api/bar", DateQuery(), maxLength, "id", "0", "1"));
		}

		// the first value fits but a later one cannot fit even on a url of its own
		[Fact]
		public void ValueTooLongMidArray_ThrowsArgumentException() {
			var baseUri = new Uri("http://myyyhost");
			var values = new[] { "0", "this-value-is-way-too-long-to-fit-in-the-url" };
			Assert.Throws<ArgumentException>(
				() => UrlExtensions.CreateUrlArray(baseUri, "api/bar", DateQuery(), 50, "id", values).ToArray());
		}
	}
}
