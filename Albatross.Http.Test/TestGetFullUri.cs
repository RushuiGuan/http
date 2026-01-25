using System;
using System.Net.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class TestGetFullUri {
		[Fact]
		public void WithAbsoluteUri_ReturnsRequestUri() {
			var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/items");
			var result = request.GetFullUri(null);
			Assert.Equal(new Uri("https://example.com/api/items"), result);
		}

		[Fact]
		public void WithRelativeUri_CombinesWithBaseAddress() {
			var request = new HttpRequestMessage(HttpMethod.Get, "api/items");
			var baseAddress = new Uri("https://example.com/");
			var result = request.GetFullUri(baseAddress);
			Assert.Equal(new Uri("https://example.com/api/items"), result);
		}

		[Fact]
		public void WithRelativeUriAndNoBaseAddress_Throws() {
			var request = new HttpRequestMessage(HttpMethod.Get, "api/items");
			Assert.Throws<InvalidOperationException>(() => request.GetFullUri(null));
		}

		[Fact]
		public void WithNullRequestUri_ReturnsBaseAddress() {
			var request = new HttpRequestMessage(HttpMethod.Get, (Uri?)null);
			var baseAddress = new Uri("https://example.com/");
			var result = request.GetFullUri(baseAddress);
			Assert.Equal(baseAddress, result);
		}

		[Fact]
		public void WithNullRequestUriAndNoBaseAddress_Throws() {
			var request = new HttpRequestMessage(HttpMethod.Get, (Uri?)null);
			Assert.Throws<InvalidOperationException>(() => request.GetFullUri(null));
		}

		[Fact]
		public void WithRelativeUriAndBaseAddressWithPath_CombinesCorrectly() {
			var request = new HttpRequestMessage(HttpMethod.Get, "items");
			var baseAddress = new Uri("https://example.com/api/");
			var result = request.GetFullUri(baseAddress);
			Assert.Equal(new Uri("https://example.com/api/items"), result);
		}
	}
}
