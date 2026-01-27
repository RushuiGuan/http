using System;
using System.Net;
using System.Net.Http;

namespace Albatross.Http {
	/// <summary>
	/// A <see cref="ServiceException{T}"/> with <see cref="string"/> as the error type,
	/// used when the error response body is plain text or when a specific error type is not needed.
	/// </summary>
	public class ServiceException : ServiceException<string> {
		public ServiceException(HttpStatusCode statusCode, HttpMethod method, Uri endpoint, string? errorMsg)
			: base(statusCode, method, endpoint, errorMsg) {
		}
	}

	/// <summary>
	/// Represents an HTTP error response with a deserialized error object of type <typeparamref name="T"/>.
	/// Thrown by the Execute methods in <see cref="HttpClientExtensions"/> when the response status code
	/// indicates a failure (400+).
	/// </summary>
	/// <typeparam name="T">The type of the deserialized error response body.</typeparam>
	public class ServiceException<T> : Exception {
		public HttpStatusCode StatusCode { get; }
		public string Method { get; }
		public string Endpoint { get; }
		/// <summary>
		/// The deserialized error response body, or null if deserialization failed or the response was empty.
		/// </summary>
		public T? ErrorObject { get; }

		public ServiceException(HttpStatusCode statusCode, HttpMethod method, Uri endpoint, T? errorObject)
			: base(BuildMessage(statusCode, method, endpoint)) {
			this.StatusCode = statusCode;
			this.Method = method.ToString();
			this.Endpoint = endpoint.ToString();
			this.ErrorObject = errorObject;
		}

		static string BuildMessage(HttpStatusCode statusCode, HttpMethod method, Uri endpoint) {
			var msg = $"Status:{(int)statusCode}; Method:{method}; Endpoint:{endpoint}";
			return msg;
		}
	}
}