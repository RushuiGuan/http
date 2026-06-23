using System;
using System.Net;
using System.Net.Http;

namespace Albatross.Http.Exceptions {
	/// <summary>
	/// A <c>404</c> HTTP error response. Derives from <see cref="Albatross.Exceptions.NotFoundException"/>
	/// so callers can catch the semantic type, while exposing the originating response context and the
	/// deserialized error body of type <typeparamref name="T"/> via <see cref="IServiceException"/>.
	/// </summary>
	/// <typeparam name="T">The type of the deserialized error response body.</typeparam>
	public class NotFoundException<T> : Albatross.Exceptions.NotFoundException, IServiceException {
		int IServiceException.StatusCode => StatusCode;
		public string Method { get; }
		public string Endpoint { get; }
		/// <summary>
		/// The deserialized error response body, or null if deserialization failed or the response was empty.
		/// </summary>
		public T? ErrorObject { get; }

		public NotFoundException(HttpMethod method, Uri endpoint, T? errorObject)
			: base(IServiceException.BuildMessage(Albatross.Exceptions.NotFoundException.StatusCode, method, endpoint)) {
			this.Method = method.ToString();
			this.Endpoint = endpoint.ToString();
			this.ErrorObject = errorObject;
		}
	}
}
