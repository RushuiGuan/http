using System.Threading.Tasks;
using Albatross.CommandLine;
using Albatross.CommandLine.Defaults;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using Sample.WebClient;

namespace Sample.CommandLine {
	internal class Program {
		static async Task<int> Main(string[] args) {
			Albatross.Logging.Extensions.RemoveLegacySlackSinkOptions();
			await using var host = new CommandHost("Sample Command Line Application");
			host.RegisterServices(RegisterServices)
				.AddCommands()
				.Parse(args, false)
				.WithDefaults()
				.Build();
			return await host.InvokeAsync();
		}
		
		static void RegisterServices(ParseResult result, IServiceCollection services) {
			services.AddSampleWebClient();
			services.RegisterCommands();
		}
	}
}