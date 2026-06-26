using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Sample.WebClient;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.CommandLine {
	[Verb<ErrorTestCase>("error", Description = "todo")]
	public class ErrorTestCaseParams {
		[Argument]
		public required string ControllerRoute { get; init; }

		[Argument]
		public required string EndPointRoute { get; init; }
	}

	public class ErrorTestCase : BaseHandler<ErrorTestCaseParams> {
		private readonly ErrorClient client;

		public ErrorTestCase(ParseResult result, ErrorTestCaseParams parameters, ErrorClient client) : base(result, parameters) {
			this.client = client;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			await client.Invoke(parameters.ControllerRoute, parameters.EndPointRoute, cancellationToken);
			return 0;
		}
	}
}
