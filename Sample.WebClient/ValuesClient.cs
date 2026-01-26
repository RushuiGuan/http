using Albatross.Http;
using Albatross.WebClient;
using System.Text.Json;

namespace Sample.WebClient {
	public class ValuesClient {
		private readonly HttpClient client;
		private readonly JsonSerializerOptions serializerOptions;

		public ValuesClient(HttpClient client) {
			this.client = client;
			this.serializerOptions = new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			};
		}

		public async Task<string?> Get(int delay, CancellationToken cancellationToken) {
			using var request = RequestExtensions.CreateRequest(HttpMethod.Get, "/api/values", new System.Collections.Specialized.NameValueCollection {
				{ "delay", delay.ToString() }
			});
			var result = await client.Execute<string, string>(request, serializerOptions, cancellationToken);
			return result;
		}

		public async Task<string?> PostError(int statusCode, string body, CancellationToken cancellationToken) {
			using var request = RequestExtensions.CreateRequest(HttpMethod.Post, "/api/values/error", new System.Collections.Specialized.NameValueCollection {
				{ "statusCode", statusCode.ToString() }
			}).CreateStringRequest(body);
			var result = await client.Execute<string, string>(request, serializerOptions, cancellationToken);
			return result;
		}
	}
}
