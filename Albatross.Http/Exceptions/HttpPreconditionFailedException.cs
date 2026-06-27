using System;
using System.Net;
using System.Net.Http;

namespace Albatross.Http.Exceptions {
	/// <summary>
	/// A <c>412 Precondition Failed</c> HTTP error response. Derives from
	/// <see cref="Albatross.Exceptions.PreconditionFailedException"/> so callers can catch the semantic type; the raw
	/// response content and content type are exposed via <see cref="IHttpException"/>.
	/// </summary>
	public class HttpPreconditionFailedException : Albatross.Exceptions.PreconditionFailedException, IHttpException {
		int IHttpException.Status => StatusCode;
		public string Method { get; }
		public string Endpoint { get; }
		public string? ContentType { get; }
		public string? Content { get; }

		public HttpPreconditionFailedException(HttpMethod method, Uri endpoint, HttpResponseContent response)
			: base(IHttpException.BuildMessage(Albatross.Exceptions.PreconditionFailedException.StatusCode, method, endpoint, response)) {
			this.Method = method.ToString();
			this.Endpoint = endpoint.ToString();
			this.ContentType = response.ContentType;
			this.Content = response.Content;
		}
	}
}
