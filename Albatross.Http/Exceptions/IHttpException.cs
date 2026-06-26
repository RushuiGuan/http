using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Albatross.Http.Exceptions {
	public interface IHttpException {
		int Status { get; }
		string Method { get; }
		string Endpoint { get; }
		string? ContentType { get;  }
		string? Content { get; }

		readonly static JsonSerializerOptions options = new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		};

		const string StatusProperty = "status";
		const string MethodProperty = "method";
		const string EndpointProperty = "endpoint";
		const string ContentProperty = "content";

		/// <summary>
		/// Builds the exception message as a JSON document. When <paramref name="content"/> is a JSON object, that
		/// object is preserved and the <c>status</c>, <c>method</c> and <c>endpoint</c> properties are merged
		/// in: a property already present with an equal value is left as-is, and one present with a different value
		/// is preserved while ours is added under a free, numerically suffixed name (e.g. <c>status2</c>) so the
		/// original body is never clobbered. Otherwise a new JSON object is created carrying <c>status</c>,
		/// <c>method</c>, <c>endpoint</c> and the raw <c>content</c>.
		/// </summary>
		public static string BuildMessage(int statusCode, HttpMethod method, Uri endpoint, HttpResponseContent content) {
			JsonObject? json = null;
			if (content.IsJson() && !string.IsNullOrEmpty(content.Content)) {
				try {
					json = JsonNode.Parse(content.Content) as JsonObject;
				} catch (JsonException) {
					// content type claimed JSON but the body did not parse as a JSON object; fall back to wrapping it
				}
			}
			if (json != null) {

				SetIntValue(json, StatusProperty, statusCode);
				SetStringValue(json, MethodProperty, method.ToString());
				SetStringValue(json, EndpointProperty, endpoint.ToString());
			} else {
				json = new JsonObject {
					[StatusProperty] = statusCode,
					[MethodProperty] = method.ToString(),
					[EndpointProperty] = endpoint.ToString(),
					[ContentProperty] = content.Content,
				};
			}
			return json.ToJsonString();

			// adds value under name, but never overwrites an existing property that holds a different value
			static void SetIntValue(JsonObject json, string name, int value) {
				if (json.TryGetPropertyValue(name, out var existing)) {
					if (existing is JsonValue existingValue && existingValue.TryGetValue<int>(out var number) && number == value) {
						return;
					}
					var i = 2;
					string alternate;
					do {
						alternate = $"{name}{i++}";
					} while (json.ContainsKey(alternate));
					name = alternate;
				}
				json[name] = value;
			}
			static void SetStringValue(JsonObject json, string name, string value) {
				if (json.TryGetPropertyValue(name, out var existing)) {
					if (existing?.GetValueKind() == JsonValueKind.String && existing.GetValue<string>() == value) {
						return;
					}
					var i = 2;
					string alternate;
					do {
						alternate = $"{name}{i++}";
					} while (json.ContainsKey(alternate));
					name = alternate;
				}
				json[name] = value;
			}
		}
	}
}