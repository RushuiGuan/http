
using System;
using System.Net.Http;

namespace Albatross.Http.Exceptions {
	/// <summary>
	/// A <c>408 Request Timeout</c> HTTP error response. Derives from <see cref="System.TimeoutException"/>
	/// so callers can catch the semantic type; the raw response content and content type are exposed via
	/// <see cref="IHttpException"/>.
	/// </summary>
	public class HttpTimeoutException : System.TimeoutException, IHttpException {
		public const int StatusCode = 408;
		int IHttpException.Status => StatusCode;
		public string Method { get; }
		public string Endpoint { get; }
		public string? ContentType { get; }
		public string? Content { get; }

		public HttpTimeoutException(HttpMethod method, Uri endpoint, HttpResponseContent response)
			: base(IHttpException.BuildMessage(StatusCode, method, endpoint, response)) {
			this.Method = method.ToString();
			this.Endpoint = endpoint.ToString();
			this.ContentType = response.ContentType;
			this.ContentType = response.Content;
		}
	}
}
