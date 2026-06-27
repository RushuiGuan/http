using System;
using System.Net.Http;
using Albatross.Http.Exceptions;
using Xunit;

namespace Albatross.Http.Test {
	public class MissingRequiredValueException_Constructor {
		[Theory]
		[InlineData(200)]
		[InlineData(204)]
		public void CapturesGivenStatusAndContext(int status) {
			var ex = new MissingRequiredValueException<Widget>(status, HttpMethod.Get,
				new Uri("https://example.com/api/test"));

			Assert.Equal(status, ((IHttpException)ex).Status);
			Assert.Equal("GET", ex.Method);
			Assert.Equal("https://example.com/api/test", ex.Endpoint);
		}

		// a successful response with no body carries no error payload to surface
		[Fact]
		public void ExposesNoErrorContent() {
			var ex = new MissingRequiredValueException<Widget>(200, HttpMethod.Get,
				new Uri("https://example.com/api/test"));

			Assert.Null(ex.ContentType);
			Assert.Null(ex.Content);
		}

		[Fact]
		public void MessageNamesTheExpectedType() {
			var ex = new MissingRequiredValueException<Widget>(200, HttpMethod.Get,
				new Uri("https://example.com/api/test"));

			Assert.Contains(typeof(Widget).ToString(), ex.Message);
		}

		[Fact]
		public void IsCatchableAsSemanticBaseType() {
			var ex = new MissingRequiredValueException<Widget>(200, HttpMethod.Get,
				new Uri("https://example.com/api/test"));

			Assert.IsAssignableFrom<Albatross.Exceptions.MissingRequiredValueException>(ex);
		}
	}
}
