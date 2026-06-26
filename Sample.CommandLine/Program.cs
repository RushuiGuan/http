using Albatross.CommandLine.Defaults;
using Albatross.CommandLine;
using Albatross.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.WebClient;
using Serilog;
using System.CommandLine;
using System.Threading.Tasks;

namespace Sample.CommandLine {
	internal class Program {
		static async Task<int> Main(string[] args) {
			await using var host = new CommandHost("Sample Cli")
			.RegisterServices(RegisterServices)
			.AddCommands()
			.Parse(args)
			.WithConfig()
			.ConfigureHost(builder => {
				builder.UseSerilog();
				builder.ConfigureLogging((context, logging) => {
					var setupSerilog = new SetupSerilog();
					setupSerilog.UseConfigFile(string.Empty, null, args, false);
					setupSerilog.Create();
				});
			})
			.Build();
			return await host.InvokeAsync();
		}

		static void RegisterServices(ParseResult result, IServiceCollection services) {
			services.RegisterCommands();
			services.AddSampleWebClient();
			services.AddSingleton<ICommandErrorHandler, DefaultCommandErrorHandler>();
		}
	}
}