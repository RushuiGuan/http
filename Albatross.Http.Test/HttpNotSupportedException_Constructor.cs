using System;
using System.Net.Http;
using Albatross.Http.Exceptions;
using Xunit;

namespace Albatross.Http.Test {
	public class HttpNotSupportedException_Constructor {
		[Fact]
		public void CapturesResponseContextAndStatus() {
			var ex = new HttpNotSupportedException(HttpMethod.Post,
				new Uri("https://example.com/api/widgets/5"),
				new HttpResponseContent { ContentType = "application/json", Content = "{\"message\":\"nope\"}" });

			Assert.Equal(501, ((IHttpException)ex).Status);
			Assert.Equal("POST", ex.Method);
			Assert.Equal("https://example.com/api/widgets/5", ex.Endpoint);
			Assert.Equal("application/json", ex.ContentType);
			Assert.Equal("{\"message\":\"nope\"}", ex.Content);
		}

		[Fact]
		public void IsCatchableAsSemanticBaseType() {
			var ex = new HttpNotSupportedException(HttpMethod.Get,
				new Uri("https://example.com/api/test"), new HttpResponseContent());

			Assert.IsAssignableFrom<System.NotSupportedException>(ex);
		}
	}
}
