using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Microsoft.Extensions.Logging;
using Sample.WebClient;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.CommandLine {
	[Verb<ErrorTestCase>("error", Description = "Invoke exception test case endpoints to verify error communication")]
	public class ErrorTestCaseParams {
		[Argument(Description = "The action to invoke: problem, exception, text-http-api-exception, json-http-api-exception, argument-exception, inner-exception, async-enumerable, yield-return")]
		public required string Action { get; init; }
	}

	public class ErrorTestCase : BaseHandler<ErrorTestCaseParams> {
		private readonly ExceptionTestCaseClient client;
		private readonly ILogger<ErrorTestCase> logger;

		public ErrorTestCase(ParseResult result, ErrorTestCaseParams parameters, ExceptionTestCaseClient client, ILogger<ErrorTestCase> logger) : base(result, parameters) {
			this.client = client;
			this.logger = logger;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			switch (parameters.Action) {
				case "async-enumerable":
				case "yield-return":
					var stream = parameters.Action == "async-enumerable"
						? client.ThrowAfterAsyncEnumerable(cancellationToken)
						: client.ThrowAfterYieldReturn(cancellationToken);
					await foreach (var item in stream) {
						Console.WriteLine($"Received: {item}");
					}
					Console.WriteLine("Unexpected: stream completed without error");
					return 1;
				default:
					string? result = parameters.Action switch {
						"problem" => await client.SendViaProblemMethod(cancellationToken),
						"exception" => await client.ThrowException(cancellationToken),
						"text-http-api-exception" => await client.ThrowTextHttpApiException(cancellationToken),
						"json-http-api-exception" => await client.ThrowJsonHttpApiException(cancellationToken),
						"argument-exception" => await client.ThrowArgumentException(cancellationToken),
						"inner-exception" => await client.ThrowExceptionWithInnerException(cancellationToken),
						_ => throw new ArgumentException($"Unknown action: {parameters.Action}"),
					};
					Console.WriteLine($"Unexpected success response: {result}");
					return 1;
			}
		}
	}
}
