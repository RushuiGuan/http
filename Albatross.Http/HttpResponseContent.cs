using System;

namespace Albatross.Http {
	public record class HttpResponseContent {
		public string? ContentType { get; init; }
		public string? Content { get; init; }

		/// <summary>
		/// True when <see cref="ContentType"/> denotes JSON — either <c>application/json</c> or a structured
		/// suffix such as <c>application/problem+json</c>. Everything after a <c>;</c> is stripped so a full
		/// header value that carries parameters (e.g. <c>application/problem+json; charset=utf-8</c>) is still
		/// recognized, not just the bare media type.
		/// </summary>
		public bool IsJson() {
			var contentType = ContentType;
			if (string.IsNullOrEmpty(contentType)) {
				return false;
			}
			var separator = contentType.IndexOf(';');
			if (separator >= 0) {
				contentType = contentType.Substring(0, separator);
			}
			contentType = contentType.Trim();
			return contentType.Equals("application/json", StringComparison.OrdinalIgnoreCase)
				|| contentType.EndsWith("+json", StringComparison.OrdinalIgnoreCase);
		}
	}
}
