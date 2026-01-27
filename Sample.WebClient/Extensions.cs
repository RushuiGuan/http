using Albatross.Config;
using Albatross.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.WebClient {
	public static class Extensions {
		public static IServiceCollection AddSampleWebClient(this IServiceCollection services) {
			services.AddConfig<SampleConfig>();
			services.AddLoggingHandler();
			services.AddHttpClient<ValuesClient>((provider, client) => {
				var config = provider.GetRequiredService<SampleConfig>();
				client.BaseAddress = new Uri(config.EndPoint);
			}).BuildDefault(false);

			services.AddHttpClient<ExceptionTestCaseClient>((provider, client) => {
				var config = provider.GetRequiredService<SampleConfig>();
				client.BaseAddress = new Uri(config.EndPoint);
			}).BuildDefault(false);
			return services;
		}
	}
}
