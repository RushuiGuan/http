using System;
using System.Net.Http;
using Albatross.Http.Exceptions;
using Xunit;

namespace Albatross.Http.Test {
	public class HttpPreconditionFailedException_Constructor {
		[Fact]
		public void CapturesResponseContextAndStatus() {
			var ex = new HttpPreconditionFailedException(HttpMethod.Post,
				new Uri("https://example.com/api/widgets/5"),
				new HttpResponseContent { ContentType = "application/json", Content = "{\"message\":\"stale\"}" });

			Assert.Equal(412, ((IHttpException)ex).Status);
			Assert.Equal("POST", ex.Method);
			Assert.Equal("https://example.com/api/widgets/5", ex.Endpoint);
			Assert.Equal("application/json", ex.ContentType);
			Assert.Equal("{\"message\":\"stale\"}", ex.Content);
		}

		[Fact]
		public void IsCatchableAsSemanticBaseType() {
			var ex = new HttpPreconditionFailedException(HttpMethod.Get,
				new Uri("https://example.com/api/test"), new HttpResponseContent());

			Assert.IsAssignableFrom<Albatross.Exceptions.PreconditionFailedException>(ex);
		}
	}
}
