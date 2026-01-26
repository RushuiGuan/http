using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Microsoft.Extensions.Logging;
using Sample.WebClient;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.CommandLine {
	[Verb<Get>("get")]
	public class GetParams {
		[Option(DefaultToInitializer = true)]
		public int Delay { get; set; } = 1000;
	}
	public class Get : IAsyncCommandHandler {
		private readonly GetParams parameters;
		private readonly ValuesClient client;
		private readonly ILogger<Get> logger;

		public Get(GetParams parameters, ValuesClient client, ILogger<Get> logger) {
			this.parameters = parameters;
			this.client = client;
			this.logger = logger;
		}
		public async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			logger.LogDebug("Invoking Get command");
			logger.LogInformation("Starting Get command with delay {delay}ms", parameters.Delay);
			var result = await client.Get(parameters.Delay, cancellationToken);
			System.Console.WriteLine($"Result: {result}");
			return 0;
		}
	}
}
