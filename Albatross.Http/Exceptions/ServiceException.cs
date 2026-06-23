using System;
using System.Net;
using System.Net.Http;

namespace Albatross.Http.Exceptions {
	/// <summary>
	/// Represents an HTTP error response with a deserialized error object of type <typeparamref name="T"/>.
	/// Thrown by the Execute methods in <see cref="HttpClientExtensions"/> when the response status code
	/// indicates a failure (400+).
	/// </summary>
	/// <typeparam name="T">The type of the deserialized error response body.</typeparam>
	public class ServiceException<T> : Exception, IServiceException {
		readonly int statusCode;
		int IServiceException.StatusCode => statusCode;
		public string Method { get; }
		public string Endpoint { get; }
		/// <summary>
		/// The deserialized error response body, or null if deserialization failed or the response was empty.
		/// </summary>
		public T? ErrorObject { get; }

		public ServiceException(int statusCode, HttpMethod method, Uri endpoint, T? errorObject)
			: base(IServiceException.BuildMessage(statusCode, method, endpoint)) {
			this.statusCode = statusCode;
			this.Method = method.ToString();
			this.Endpoint = endpoint.ToString();
			this.ErrorObject = errorObject;
		}
	}
}