using Albatross.Exceptions;
using System;
using System.Net;
using System.Net.Http;

namespace Albatross.Http.Exceptions {
	/// <summary>
	/// Thrown when a successful HTTP response returned no content (or content that deserialized to null) where a
	/// <typeparamref name="TResponse"/> value was required. Derives from
	/// <see cref="Albatross.Exceptions.MissingRequiredValueException"/> and exposes the originating response
	/// context via <see cref="IServiceException"/>.
	/// </summary>
	/// <typeparam name="TResponse">The response type that was expected but not returned.</typeparam>
	public class MissingRequiredValueException<TResponse> : MissingRequiredValueException, IServiceException {
		readonly int statusCode;
		int IServiceException.StatusCode => statusCode;
		public string Method { get; }
		public string Endpoint { get; }

		public MissingRequiredValueException(int statusCode, HttpMethod method, Uri endpoint)
			: base($"Expected {typeof(TResponse)} but no content was returned") {
			this.statusCode = statusCode;
			this.Method = method.ToString();
			this.Endpoint = endpoint.ToString();
		}
	}
}