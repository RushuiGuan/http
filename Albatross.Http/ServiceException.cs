using System;
using System.Net;
using System.Net.Http;

namespace Albatross.Http {
	public class ServiceException : ServiceException<string> {
		public ServiceException(HttpStatusCode statusCode, HttpMethod method, Uri endpoint, string? errorMsg)
			: base(statusCode, method, endpoint, errorMsg) {
		}
	}
	public class ServiceException<T> : Exception {
		public HttpStatusCode StatusCode { get; private set; }
		public string Method { get; set; }
		public string Endpoint { get; set; }
		public T? ErrorObject { get; private set; }

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