using System;
using System.Collections.Generic;

namespace Albatross.Http {
	public static class RequestBuilderExtensions {
		public static RequestBuilder AddQueryStringIfSet(this RequestBuilder builder, string name, string? value) {
			if (!string.IsNullOrEmpty(value)) {
				builder.AddQueryString(name, value);
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
			if(value.HasValue) {
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