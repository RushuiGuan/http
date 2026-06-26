using System;
using System.Net.Http;

namespace Albatross.Http.Exceptions {
	/// <summary>
	/// A generic HTTP error response for status codes that have no dedicated semantic exception. Thrown by the
	/// Execute methods in <see cref="HttpClientExtensions"/> when the response status code indicates a failure
	/// (400+); the raw response content and content type are exposed via <see cref="IHttpException"/>.
	/// </summary>
	public class ServiceException : Exception, IHttpException {
		readonly int statusCode;
		int IHttpException.Status => statusCode;
		public string Method { get; }
		public string Endpoint { get; }
		public string? ContentType { get; }
		public string? Content { get; }

		public ServiceException(int statusCode, HttpMethod method, Uri endpoint, HttpResponseContent response)
			: base(IHttpException.BuildMessage(statusCode, method, endpoint, response)) {
			this.statusCode = statusCode;
			this.Method = method.ToString();
			this.Endpoint = endpoint.ToString();
			this.ContentType = response.ContentType;
			this.Content = response.Content;
		}
	}
}
