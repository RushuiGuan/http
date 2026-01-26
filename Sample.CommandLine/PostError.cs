using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.WebClient;
using Microsoft.Extensions.Logging;
using Sample.WebClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.CommandLine {
	[Verb<PostError>("post-error")]
	public class PostErrorParams {
		[Option(DefaultToInitializer = true)]
		public int StatusCode { get; set; } = 400;

		[Option(DefaultToInitializer = true)]
		public string Body { get; set; } = "test error message";
	}

	public class PostError : IAsyncCommandHandler {
		private readonly PostErrorParams parameters;
		private readonly ValuesClient client;
		private readonly ILogger<PostError> logger;

		public PostError(PostErrorParams parameters, ValuesClient client, ILogger<PostError> logger) {
			this.parameters = parameters;
			this.client = client;
			this.logger = logger;
		}

		public async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Sending POST with status code {statusCode} and body: {body}", parameters.StatusCode, parameters.Body);
			try {
				var result = await client.PostError(parameters.StatusCode, parameters.Body, cancellationToken);
				Console.WriteLine($"Unexpected success response: {result}");
				return 1;
			} catch (ServiceException<string> ex) {
				Console.WriteLine($"StatusCode: {(int)ex.StatusCode}");
				Console.WriteLine($"Method: {ex.Method}");
				Console.WriteLine($"Endpoint: {ex.Endpoint}");
				Console.WriteLine($"ErrorObject: {ex.ErrorObject}");

				if ((int)ex.StatusCode == parameters.StatusCode && ex.ErrorObject == parameters.Body) {
					Console.WriteLine("Verification passed: error communication is correct");
					return 0;
				} else {
					Console.WriteLine("Verification failed:");
					if ((int)ex.StatusCode != parameters.StatusCode) {
						Console.WriteLine($"  Expected status code {parameters.StatusCode}, got {(int)ex.StatusCode}");
					}
					if (ex.ErrorObject != parameters.Body) {
						Console.WriteLine($"  Expected body \"{parameters.Body}\", got \"{ex.ErrorObject}\"");
					}
					return 1;
				}
			}
		}
	}
}
