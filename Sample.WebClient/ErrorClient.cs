using Albatross.Http;
using System.Text.Json;

namespace Sample.WebClient {
	/// <summary>
	/// Drives the error-test endpoints. Every such endpoint is an HTTP GET that takes no parameters and is
	/// expected to fail, so the only variables are the controller route, the endpoint route, and the error body
	/// type <typeparamref name="TError"/> that the resulting semantic exception should carry. Vary
	/// <typeparamref name="TError"/> (e.g. <see cref="string"/> vs <see cref="ProblemDetails"/>) to observe how
	/// the error body is captured for a given generation method.
	/// </summary>
	public class ErrorClient {
		private readonly HttpClient client;
		private readonly JsonSerializerOptions serializerOptions;

		public ErrorClient(HttpClient client) {
			this.client = client;
			this.serializerOptions = new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			};
		}

		/// <summary>
		/// Sends <c>GET /api/{controllerRoute}/{endpointRoute}</c>. The endpoint is expected to return an error
		/// status, which <see cref="HttpClientExtensions.Send{TError}"/> surfaces as the matching semantic
		/// exception carrying a deserialized <typeparamref name="TError"/> error body.
		/// </summary>
		public async Task Invoke(string controllerRoute, string endpointRoute, CancellationToken cancellationToken) {
			using var request = new RequestBuilder()
				.WithMethod(HttpMethod.Get)
				.WithRelativeUrl($"/api/{controllerRoute}/{endpointRoute}")
				.Build();
			await client.Send(request, serializerOptions, cancellationToken);
		}
	}
}
