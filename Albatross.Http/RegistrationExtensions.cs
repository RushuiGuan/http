using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Albatross.Http {
	public static class RegistrationExtensions {
		public static IServiceCollection AddLoggingHandler(this IServiceCollection services)
			=> services.AddTransient<LoggingHandler>();

		/// <summary>
		/// Configures an <see cref="IHttpClientBuilder"/> with sensible defaults: removes the built-in HTTP loggers
		/// (replaced by <see cref="LoggingHandler"/>), enables GZip/Deflate/Brotli automatic decompression,
		/// allows auto-redirect, and optionally enables Windows default credentials with pre-authentication.
		/// </summary>
		/// <param name="builder">The HTTP client builder to configure.</param>
		/// <param name="useDefaultCredentials">
		/// When true, enables <see cref="HttpClientHandler.UseDefaultCredentials"/> and <see cref="HttpClientHandler.PreAuthenticate"/>
		/// so that the current Windows identity is sent with each request.
		/// </param>
		public static IHttpClientBuilder BuildDefault(this IHttpClientBuilder builder, bool useDefaultCredentials)
			=> builder.RemoveAllLoggers().ConfigurePrimaryHttpMessageHandler(() =>
				new HttpClientHandler {
					UseDefaultCredentials = useDefaultCredentials,
					PreAuthenticate = useDefaultCredentials,
					AllowAutoRedirect = true,
					AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.Brotli
				}
			).AddHttpMessageHandler<LoggingHandler>();
	}
}