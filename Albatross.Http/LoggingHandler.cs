using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.Http {
	/// <summary>
	/// A delegating handler that logs HTTP request and response details using structured logging.
	/// </summary>
	/// <remarks>
	/// Register with IHttpClientFactory:
	/// <code>
	/// services.AddTransient&lt;LoggingHandler&gt;();
	/// services.AddHttpClient("MyApi")
	///     .AddHttpMessageHandler&lt;LoggingHandler&gt;();
	/// </code>
	/// </remarks>
	public class LoggingHandler : DelegatingHandler {
		private readonly ILogger<LoggingHandler> logger;

		public LoggingHandler(ILogger<LoggingHandler> logger) {
			this.logger = logger;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			logger.LogDebug("HTTP {method} {uri} started", request.Method, request.RequestUri);
			try {
				var response = await base.SendAsync(request, cancellationToken);
				if ((int)response.StatusCode >= 400) {
					var body = await response.ReadResponseAsText(true);
					logger.LogError("HTTP {method} {uri} completed {statusCode}\n{body}", request.Method, request.RequestUri, (int)response.StatusCode, body);
				} else {
					logger.LogDebug("HTTP {method} {uri} completed {statusCode}", request.Method, request.RequestUri, (int)response.StatusCode);
				}
				return response;
			} catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested) {
				logger.LogInformation("HTTP {method} {uri} cancelled", request.Method, request.RequestUri);
				throw;
			} catch (System.Exception err) {
				logger.LogError(err, "HTTP {method} {uri} failed", request.Method, request.RequestUri);
				throw;
			}
		}
	}
}
