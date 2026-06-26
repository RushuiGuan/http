using Albatross.Hosting;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Sample.WebApi.Controllers {
	/// <summary>
	/// Exercises the FluentValidation error flows. Each endpoint validates a deliberately invalid request and
	/// surfaces the failures a different way, so the client/CLI output can be compared across them:
	/// <list type="bullet">
	///   <item><c>has-problem</c> — Albatross.Hosting <see cref="ValidationExtensions.HasProblem"/> turned into an
	///   explicit <c>BadRequest(ValidationProblemDetails)</c> (400).</item>
	///   <item><c>validate-and-throw</c> — <c>ValidateAndThrow</c> lets a <see cref="ValidationException"/> escape
	///   to the global exception handler, which maps it to 422.</item>
	///   <item><c>validation-problem</c> — the MVC built-in <c>ControllerBase.ValidationProblem(ModelState)</c> (400).</item>
	/// </list>
	/// </summary>
	[Route("api/fluent")]
	[ApiController]
	public class FluentValidationErrorController : ControllerBase {
		private readonly IValidator<SaveWidgetRequest> validator = new SaveWidgetRequestValidator();

		// invalid on both rules, so every endpoint produces a multi-error result and can stay a parameterless GET
		private static SaveWidgetRequest InvalidRequest => new SaveWidgetRequest { Name = string.Empty, Quantity = -5 };

		[HttpGet("has-problem")]
		public ActionResult HasProblem() {
			var result = validator.Validate(InvalidRequest);
			if (result.HasProblem(out var details)) {
				return ValidationProblem(details);
			}
			return Ok();
		}

		[HttpGet("validate-and-throw")]
		public ActionResult ValidateAndThrow() {
			validator.ValidateAndThrow(InvalidRequest);
			return Ok();
		}

		[HttpGet("validation-problem")]
		public ActionResult ValidationProblemFromModelState() {
			var result = validator.Validate(InvalidRequest);
			foreach (var error in result.Errors) {
				ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
			}
			return ValidationProblem(ModelState);
		}
	}

	public class SaveWidgetRequest {
		public string Name { get; set; } = string.Empty;
		public int Quantity { get; set; }
	}

	public class SaveWidgetRequestValidator : AbstractValidator<SaveWidgetRequest> {
		public SaveWidgetRequestValidator() {
			RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
			RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be positive");
		}
	}
}
