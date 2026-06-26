using Albatross.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Sample.WebApi.Controllers {
	[Route("api/global")]
	[ApiController]
	public class GlobalErrorHandlerController : ControllerBase {
		[HttpGet("semantic")]
		public void SemanticError() {
			throw new NotFoundException("entity a");
		}

		[HttpGet("server")]
		public void ServerError() {
			throw new InvalidOperationException("this shouuld not happen");
		}

		[HttpGet("semantic-with-inner")]
		public void SemanticWithInnerError() {
			throw new NotFoundException("entity a", new InvalidOperationException("not found inner"));
		}
	}
}
