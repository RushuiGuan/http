using System;
using System.Collections.Generic;

namespace Albatross.Http {
	public static class RequestBuilderExtensions {
		public static RequestBuilder AddQueryString<T>(this RequestBuilder builder, string name, T value) where T : notnull {
			builder.AddQueryString(name, value.ToString());
			return builder;
		}

		public static RequestBuilder AddQueryStringIfSet<T>(this RequestBuilder builder, string name, T? value) where T : class {
			if (value != null) {
				var text = value.ToString();
				if (!string.IsNullOrEmpty(text)) {
					builder.AddQueryString(name, text);
				}
			}
			return builder;
		}

		public static RequestBuilder AddQueryStringIfSet<T>(this RequestBuilder builder, string name, T? value) where T : struct {
			if (value.HasValue) {
				switch (value.Value) {
					case DateTime dt:
						builder.AddQueryString(name, dt.ISO8601());
						break;
					case DateOnly d:
						builder.AddQueryString(name, d.ISO8601());
						break;
					case TimeOnly t:
						builder.AddQueryString(name, t.ISO8601());
						break;
					case DateTimeOffset dto:
						builder.AddQueryString(name, dto.ISO8601());
						break;
					default:
						builder.AddQueryString(name, $"{value.Value}");
						break;
				}
			}
			return builder;
		}
	}
}