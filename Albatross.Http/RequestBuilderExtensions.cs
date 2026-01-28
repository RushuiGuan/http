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
				builder.AddQueryString(name, $"{value.Value}");
			}
			return builder;
		}

		public static RequestBuilder AddDateTimeQueryString(this RequestBuilder builder, string name, DateTime? value) {
			if (value.HasValue) {
				builder.AddQueryString(name, value.Value.ToString("o"));
			}
			return builder;
		}

		public static RequestBuilder AddDateOnlyQueryString(this RequestBuilder builder, string name, DateOnly? value) {
			if (value.HasValue) {
				builder.AddQueryString(name, value.Value.ToString("yyyy-MM-dd"));
			}
			return builder;
		}

		public static RequestBuilder AddTimeOnlyQueryString(this RequestBuilder builder, string name, TimeOnly? value) {
			if (value.HasValue) {
				builder.AddQueryString(name, value.Value.ToString("HH:mm:ss.fffffff").TrimEnd('0').TrimEnd('.'));
			}
			return builder;
		}

		public static RequestBuilder AddDateTimeOffsetQueryString(this RequestBuilder builder, string name, DateTimeOffset? value) {
			if (value.HasValue) {
				builder.AddQueryString(name, value.Value.ToString("o"));
			}
			return builder;
		}
	}
}