using System;
using System.Net;
using System.Net.Http;

namespace Albatross.Http.Exceptions {
	public interface IServiceException {
		int StatusCode { get; }
		string Method { get; }
		string Endpoint { get; }

		public static string BuildMessage(int statusCode, HttpMethod method, Uri endpoint)
			=> $"Status:{statusCode}; Method:{method}; Endpoint:{endpoint}";
	}
}