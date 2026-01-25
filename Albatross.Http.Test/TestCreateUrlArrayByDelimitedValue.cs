using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xunit;

namespace Albatross.Http.Test {
	public class TestCreateUrlArrayByDelimitedValue {
		//[InlineData(45, 2,".", "api/bar?date=2022-10-10&id=0&", "api/bar?date=2022-10-10&id=1&")]
		//[InlineData(46, 2, "api/bar?date=2022-10-10&id=0&", "api/bar?date=2022-10-10&id=1&")]
		//[InlineData(47, 2, "api/bar?date=2022-10-10&id=0&", "api/bar?date=2022-10-10&id=1&")]
		//[InlineData(48, 2, "api/bar?date=2022-10-10&id=0&", "api/bar?date=2022-10-10&id=1&")]
		//[InlineData(49, 2, "api/bar?date=2022-10-10&id=0&", "api/bar?date=2022-10-10&id=1&")]
		[InlineData(50, 2, ".", "api/bar?date=2022-10-10&id=0.1")]
		[InlineData(51, 2, ".", "api/bar?date=2022-10-10&id=0.1")]
		[InlineData(52, 2, ".", "api/bar?date=2022-10-10&id=0.1")]
		[InlineData(52, 2, ",", "api/bar?date=2022-10-10&id=0%2C1")]
		[InlineData(48, 2, ",", "api/bar?date=2022-10-10&id=0%2C1")]
		[InlineData(47, 2, ",", "api/bar?date=2022-10-10&id=0", "api/bar?date=2022-10-10&id=1")]
		[InlineData(44, 2, ",", "api/bar?date=2022-10-10&id=0", "api/bar?date=2022-10-10&id=1")]
		[Theory]
		public void TestNormalCreation(int maxlength, int count, string delimiter, params string[] expected) {
			var baseUri = new Uri("http://myyyhost");
			string path = $"api/bar";
			var queryString = new NameValueCollection {
				{ "date", "2022-10-10" }
			};
			var arrayQueryString = new List<string>();
			for (int i = 0; i < count; i++) {
				arrayQueryString.Add(i.ToString());
			}
			var requests = UrlExtensions.CreateUrlArrayByDelimitedValue(baseUri, path, queryString, maxlength, "id", delimiter, arrayQueryString.ToArray()).ToArray();
			Assert.Equal(expected.Length, requests.Count());
			List<(string expectedUrl, string actualUrl)> list = new List<(string expectedUrl, string actualUrl)>();
			List<Action<(string expectedUrl, string actualUrl)>> actions = new List<Action<(string, string)>>();
			for (int i = 0; i < expected.Length; i++) {
				list.Add((expected[i], requests[i]));
				actions.Add(args => Assert.Equal(args.expectedUrl, args.actualUrl));
			}
			Assert.Collection(list, actions.ToArray());
		}

		[Theory]
		[InlineData(2, 2, ",")]
		[InlineData(43, 2, ",")]
		public void TestInsufficientMaxLengthWithDelimiter(int maxlength, int count, string delimiter) {
			var baseUri = new Uri("http://myyyhost");
			string path = $"api/bar";
			var queryString = new NameValueCollection {
				{ "date", "2022-10-10" }
			};
			var arrayQueryString = new List<string>();
			for (int i = 0; i < count; i++) {
				arrayQueryString.Add(i.ToString());
			}
			Assert.Throws<ArgumentException>(() => UrlExtensions.CreateUrlArrayByDelimitedValue(baseUri, path, queryString, maxlength, "id", delimiter, arrayQueryString.ToArray()));
		}

		[Fact]
		public void ValueTooLongInMiddleOfArray_Throws() {
			var baseUri = new Uri("http://myyyhost");
			string path = "api/bar";
			var queryString = new NameValueCollection {
				{ "date", "2022-10-10" }
			};
			// First value fits, second value is too long to fit on its own
			var arrayValues = new[] { "0", "this-value-is-way-too-long-to-fit-in-the-url" };
			// maxlength allows "0" but not the long value
			int maxlength = 50;

			Assert.Throws<ArgumentException>(() => UrlExtensions.CreateUrlArrayByDelimitedValue(baseUri, path, queryString, maxlength, "id", ",", arrayValues).ToArray());
		}
	}
}
