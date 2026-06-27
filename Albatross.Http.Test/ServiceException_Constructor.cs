using System;
using System.Net.Http;
using Albatross.Http.Exceptions;
using Xunit;

namespace Albatross.Http.Test {
	public class ServiceException_Constructor {
		// ServiceException is the fallback for any status with no dedicated semantic exception, so the status it
		// reports is whatever the caller supplies rather than a fixed constant.
		[Theory]
		[InlineData(400)]
		[InlineData(418)]
		[InlineData(500)]
		[InlineData(503)]
		public void CapturesGivenStatusAndResponseContext(int status) {
			var ex = new ServiceException(status, HttpMethod.Post,
				new Uri("https://example.com/api/widgets/5"),
				new HttpResponseContent { ContentType = "text/plain", Content = "boom" });

			Assert.Equal(status, ((IHttpException)ex).Status);
			Assert.Equal("POST", ex.Method);
			Assert.Equal("https://example.com/api/widgets/5", ex.Endpoint);
			Assert.Equal("text/plain", ex.ContentType);
			Assert.Equal("boom", ex.Content);
		}
	}
}
