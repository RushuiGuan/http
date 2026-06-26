namespace Sample.Core {
	public record class ErrorResult {
		public required string Error { get; init; }
		public required string ErrorDescription { get; init; }
	}
}
