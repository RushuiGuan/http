using Albatross.Http;
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
			using var request = new RequestBuilder().WithMethod(HttpMethod.Get).WithRelativeUrl("/api/values").AddQueryString("delay", delay.ToString()).Build();
			var result = await client.Execute<string, string>(request, serializerOptions, cancellationToken);
			return result;
		}
	}
}
