using System;
using System.Net;
using System.Net.Http;

namespace Albatross.Http.Exceptions {
	/// <summary>
	/// A <c>401 Unauthorized</c> HTTP error response. Derives from
	/// <see cref="Albatross.Exceptions.NotAuthenticatedException"/> so callers can catch the semantic type; the raw
	/// response content and content type are exposed via <see cref="IHttpException"/>.
	/// </summary>
	public class HttpNotAuthenticatedException : Albatross.Exceptions.NotAuthenticatedException, IHttpException {
		int IHttpException.Status => StatusCode;
		public string Method { get; }
		public string Endpoint { get; }
		public string? ContentType { get; }
		public string? Content { get; }

		public HttpNotAuthenticatedException(HttpMethod method, Uri endpoint, HttpResponseContent response)
			: base(IHttpException.BuildMessage(Albatross.Exceptions.NotAuthenticatedException.StatusCode, method, endpoint, response)) {
			this.Method = method.ToString();
			this.Endpoint = endpoint.ToString();
			this.ContentType = response.ContentType;
			this.Content = response.Content;
		}
	}
}
