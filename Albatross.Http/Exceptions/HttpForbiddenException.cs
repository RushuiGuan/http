using System;
using System.Net;
using System.Net.Http;

namespace Albatross.Http.Exceptions {
	/// <summary>
	/// A <c>403 Forbidden</c> HTTP error response. Derives from <see cref="Albatross.Exceptions.ForbiddenException"/>
	/// so callers can catch the semantic type; the raw response content and content type are exposed via
	/// <see cref="IHttpException"/>.
	/// </summary>
	public class HttpForbiddenException : Albatross.Exceptions.ForbiddenException, IHttpException {
		int IHttpException.Status => StatusCode;
		public string Method { get; }
		public string Endpoint { get; }
		public string? ContentType { get; }
		public string? Content { get; }

		public HttpForbiddenException(HttpMethod method, Uri endpoint, HttpResponseContent response)
			: base(IHttpException.BuildMessage(Albatross.Exceptions.ForbiddenException.StatusCode, method, endpoint, response)) {
			this.Method = method.ToString();
			this.Endpoint = endpoint.ToString();
			this.ContentType = response.ContentType;
			this.Content = response.Content;
		}
	}
}
