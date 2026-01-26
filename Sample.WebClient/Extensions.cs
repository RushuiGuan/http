using Albatross.Config;
using Albatross.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.WebClient {
	public static class Extensions {
		public static IServiceCollection AddSampleWebClient(this IServiceCollection services) {
			services.AddConfig<SampleConfig>();
			services.AddTransient<LoggingHandler>();
			services.AddHttpClient<ValuesClient>((provider, client) => {
				var config = provider.GetRequiredService<SampleConfig>();
				client.BaseAddress = new Uri(config.EndPoint);
			}).RemoveAllLoggers()
			.ConfigurePrimaryHttpMessageHandler(() => {
				return new HttpClientHandler {
					AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.Brotli
				};
			})
			.AddHttpMessageHandler<LoggingHandler>();

			services.AddHttpClient<ExceptionTestCaseClient>((provider, client) => {
				var config = provider.GetRequiredService<SampleConfig>();
				client.BaseAddress = new Uri(config.EndPoint);
			}).RemoveAllLoggers().AddHttpMessageHandler<LoggingHandler>();
			return services;
		}
	}
}
