using System;
using System.Net;
using System.Net.Http;

namespace Albatross.Http.Exceptions {
	/// <summary>
	/// A <c>422</c> HTTP error response. Derives from <see cref="Albatross.Exceptions.ValidationException"/>
	/// so callers can catch the semantic type, while exposing the originating response context and the
	/// deserialized error body of type <typeparamref name="T"/> via <see cref="IHttpException"/>.
	/// </summary>
	/// <typeparam name="T">The type of the deserialized error response body.</typeparam>
	public class HttpValidationException : Albatross.Exceptions.ValidationException, IHttpException {
		int IHttpException.Status => StatusCode;
		public string Method { get; }
		public string Endpoint { get; }
		public string? ContentType { get; }
		public string? Content { get; }

		public HttpValidationException(HttpMethod method, Uri endpoint, HttpResponseContent response)
			: base(IHttpException.BuildMessage(Albatross.Exceptions.ValidationException.StatusCode, method, endpoint, response)) {
			this.Method = method.ToString();
			this.Endpoint = endpoint.ToString();
			this.ContentType = response.ContentType;
			this.ContentType = response.Content;
		}
	}
}
