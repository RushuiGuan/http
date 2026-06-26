using Albatross.CommandLine;
using Albatross.Http.Exceptions;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Json;
using System;

namespace Sample.CommandLine{
	public class DefaultCommandErrorHandler : ICommandErrorHandler {
		private readonly ICommandContext context;
		private readonly ILogger<DefaultCommandErrorHandler> logger;

		public DefaultCommandErrorHandler(ICommandContext context, ILogger<DefaultCommandErrorHandler> logger) {
			this.context = context;
			this.logger = logger;
		}
		public int? Handle(Exception exception) {
			logger.LogError(exception, $"error executing command {context.Key}");
			if (exception is IHttpException) {
				// the message is already a JSON document built by IHttpException.BuildMessage
				AnsiConsole.Write(new JsonText(exception.Message));
			} else {
				AnsiConsole.MarkupLineInterpolated($"[bold red]Error:[/] {exception.Message}");
			}
			return 1;
		}
	}
}
