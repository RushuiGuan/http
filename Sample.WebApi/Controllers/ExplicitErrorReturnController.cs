using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Sample.WebApi.Controllers {
	/// <summary>
	/// Returns 404 Not Found explicitly as an <see cref="ActionResult"/> (no exception is thrown). The endpoints
	/// differ only in the response body — a ProblemDetails document, a custom JSON object, a plain string, or no
	/// body — so the client's handling of each error content type can be compared.
	/// </summary>
	[Route("api/explicit")]
	[ApiController]
	public class ExplicitErrorReturnController : ControllerBase {
		[HttpGet("problem-details")]
		public ActionResult ProblemData() {
			// RFC problem details body -> application/problem+json
			return Problem(detail: "entity a not found", statusCode: StatusCodes.Status404NotFound, title: "Not Found");
		}

		[HttpGet("custom")]
		public ActionResult Custom() {
			// custom JSON body (not a ProblemDetails) -> application/json
			return NotFound(new { error = "not_found", message = "entity a not found", id = 42 });
		}

		[HttpGet("string")]
		public ActionResult StringData() {
			// plain string body -> text/plain via the string output formatter
			return NotFound("entity a not found");
		}

		[HttpGet("empty")]
		public ActionResult Empty() {
			// no body
			return NotFound();
		}
	}
}
