using Albatross.Hosting.ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Sample.WebApi.Controllers {
	/// <summary>
	/// Mirrors the throwing scenarios in <see cref="GlobalErrorHandlerController"/>, but catches each exception and
	/// converts it to an <see cref="ActionResult"/> via <see cref="IExceptionHandler"/> with
	/// <c>MaskExceptionDetail</c> off, so the exception detail is returned to the caller.
	/// </summary>
	[Route("api/default-with-detail")]
	[ApiController]
	public class DefaultWithDetailController : ControllerBase {
		private readonly IExceptionHandler errHandler;

		public DefaultWithDetailController(ILogger<DefaultExceptionHandler> logger) {
			this.errHandler = new DefaultExceptionHandler(false, logger);
		}

		[HttpGet("semantic")]
		public ActionResult Semantic() {
			try {
				throw new ArgumentException("This is a test exception");
			} catch (Exception err) {
				return errHandler.Handle(err, null);
			}
		}

		[HttpGet("server")]
		public ActionResult Server() {
			try {
				throw new Exception("This is a test exception");
			} catch (Exception err) {
				return errHandler.Handle(err, null);
			}
		}

		[HttpGet("semantic-with-inner")]
		public ActionResult SemanticWithInner() {
			try {
				throw new ArgumentException("This is a test exception", new InvalidOperationException("This is an inner exception"));
			} catch (Exception err) {
				return errHandler.Handle(err, null);
			}
		}
	}
}
