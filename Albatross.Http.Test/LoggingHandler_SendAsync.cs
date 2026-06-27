using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Albatross.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Albatross.Http.Test {
	public class LoggingHandler_SendAsync {
		static HttpRequestMessage Request() => new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

		static (HttpMessageInvoker invoker, RecordingLogger logger) CreateInvoker(HttpMessageHandler inner) {
			var logger = new RecordingLogger();
			var handler = new LoggingHandler(logger) { InnerHandler = inner };
			return (new HttpMessageInvoker(handler), logger);
		}

		// a successful response is logged at Debug on both start and completion, with no error logged
		[Fact]
		public async Task SuccessResponse_LogsStartAndCompletionAtDebug() {
			var inner = new StubInnerHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });
			var (invoker, logger) = CreateInvoker(inner);

			var response = await invoker.SendAsync(Request(), CancellationToken.None);

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.All(logger.Entries, e => Assert.Equal(LogLevel.Debug, e.Level));
			Assert.Contains(logger.Entries, e => e.Message.Contains("started"));
			Assert.Contains(logger.Entries, e => e.Message.Contains("completed"));
			Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Error);
		}

		// an error status is not thrown by the handler; it is logged at Error with the status code and body, and the
		// response is still returned to the caller
		[Fact]
		public async Task ErrorResponse_LogsErrorWithStatusAndBodyAndReturnsResponse() {
			var inner = new StubInnerHandler(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("boom") });
			var (invoker, logger) = CreateInvoker(inner);

			var response = await invoker.SendAsync(Request(), CancellationToken.None);

			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
			Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error && e.Message.Contains("404") && e.Message.Contains("boom"));
		}

		// when the request is cancelled, the cancellation is logged at Information and the original exception rethrown
		[Fact]
		public async Task Cancellation_LogsInformationAndRethrows() {
			var inner = new StubInnerHandler(new TaskCanceledException());
			var (invoker, logger) = CreateInvoker(inner);
			using var cts = new CancellationTokenSource();
			cts.Cancel();

			await Assert.ThrowsAsync<TaskCanceledException>(() => invoker.SendAsync(Request(), cts.Token));
			Assert.Contains(logger.Entries, e => e.Level == LogLevel.Information && e.Message.Contains("cancelled"));
		}

		// any other failure is logged at Error with the exception attached and rethrown
		[Fact]
		public async Task InnerException_LogsErrorWithExceptionAndRethrows() {
			var failure = new InvalidOperationException("inner failed");
			var inner = new StubInnerHandler(failure);
			var (invoker, logger) = CreateInvoker(inner);

			var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => invoker.SendAsync(Request(), CancellationToken.None));

			Assert.Same(failure, thrown);
			Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error && ReferenceEquals(e.Exception, failure));
		}
	}

	/// <summary>Inner handler that returns a fixed response or throws a fixed exception.</summary>
	internal sealed class StubInnerHandler : HttpMessageHandler {
		readonly Func<Task<HttpResponseMessage>> behavior;
		public StubInnerHandler(HttpResponseMessage response) => behavior = () => Task.FromResult(response);
		public StubInnerHandler(Exception exception) => behavior = () => Task.FromException<HttpResponseMessage>(exception);
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => behavior();
	}

	/// <summary>An ILogger that records every entry so tests can assert on level, rendered message, and exception.</summary>
	internal sealed class RecordingLogger : ILogger<LoggingHandler> {
		public readonly List<(LogLevel Level, string Message, Exception? Exception)> Entries = new();

		public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
			=> Entries.Add((logLevel, formatter(state, exception), exception));

		sealed class NullScope : IDisposable {
			public static readonly NullScope Instance = new();
			public void Dispose() { }
		}
	}
}
