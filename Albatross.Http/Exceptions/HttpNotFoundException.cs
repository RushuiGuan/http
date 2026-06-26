using System;
using System.Net;
using System.Net.Http;

namespace Albatross.Http.Exceptions {
	/// <summary>
	/// A <c>404 Not Found</c> HTTP error response. Derives from <see cref="Albatross.Exceptions.NotFoundException"/>
	/// so callers can catch the semantic type; the raw response content and content type are exposed via
	/// <see cref="IHttpException"/>.
	/// </summary>
	public class HttpNotFoundException : Albatross.Exceptions.NotFoundException, IHttpException {
		int IHttpException.Status => StatusCode;
		public string Method { get; }
		public string Endpoint { get; }
		public string? ContentType { get; }
		public string? Content { get; }

		public HttpNotFoundException(HttpMethod method, Uri endpoint, HttpResponseContent response)
			: base(IHttpException.BuildMessage(Albatross.Exceptions.NotFoundException.StatusCode, method, endpoint, response)) {
			this.Method = method.ToString();
			this.Endpoint = endpoint.ToString();
			this.ContentType = response.ContentType;
			this.ContentType = response.Content;
		}
	}
}
