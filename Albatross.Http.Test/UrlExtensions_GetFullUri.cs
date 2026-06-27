using System;
using System.Net.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class UrlExtensions_GetFullUri {
		[Fact]
		public void AbsoluteRequestUri_ReturnedAsIs() {
			var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/items");
			var result = request.GetFullUri(null);
			Assert.Equal(new Uri("https://example.com/api/items"), result);
		}

		[Theory]
		[InlineData("https://example.com/", "api/items", "https://example.com/api/items")]
		[InlineData("https://example.com/api/", "items", "https://example.com/api/items")]
		public void RelativeRequestUri_CombinedWithBaseAddress(string baseAddress, string relative, string expected) {
			var request = new HttpRequestMessage(HttpMethod.Get, relative);
			var result = request.GetFullUri(new Uri(baseAddress));
			Assert.Equal(new Uri(expected), result);
		}

		[Fact]
		public void NullRequestUri_ReturnsBaseAddress() {
			var request = new HttpRequestMessage(HttpMethod.Get, (Uri?)null);
			var baseAddress = new Uri("https://example.com/");
			var result = request.GetFullUri(baseAddress);
			Assert.Equal(baseAddress, result);
		}

		[Fact]
		public void RelativeRequestUriWithoutBaseAddress_ThrowsInvalidOperation() {
			var request = new HttpRequestMessage(HttpMethod.Get, "api/items");
			Assert.Throws<InvalidOperationException>(() => request.GetFullUri(null));
		}

		[Fact]
		public void NullRequestUriWithoutBaseAddress_ThrowsInvalidOperation() {
			var request = new HttpRequestMessage(HttpMethod.Get, (Uri?)null);
			Assert.Throws<InvalidOperationException>(() => request.GetFullUri(null));
		}
	}
}
