using System;
using System.Net;
using System.Net.Http;

namespace Albatross.Http.Exceptions {
	/// <summary>
	/// A <c>501 Not Implemented</c> HTTP error response. Derives from <see cref="System.NotSupportedException"/>
	/// so callers can catch the semantic type; the raw response content and content type are exposed via
	/// <see cref="IHttpException"/>.
	/// </summary>
	public class HttpNotSupportedException : System.NotSupportedException, IHttpException {
		public const int StatusCode = 501;
		int IHttpException.Status => StatusCode;
		public string Method { get; }
		public string Endpoint { get; }
		public string? ContentType { get; }
		public string? Content { get; }

		public HttpNotSupportedException(HttpMethod method, Uri endpoint, HttpResponseContent response)
			: base(IHttpException.BuildMessage(StatusCode, method, endpoint, response)) {
			this.Method = method.ToString();
			this.Endpoint = endpoint.ToString();
			this.ContentType = response.ContentType;
			this.Content = response.Content;
		}
	}
}
