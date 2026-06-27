using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using Albatross.Http.Exceptions;
using Xunit;

namespace Albatross.Http.Test {
	public class IHttpException_BuildMessage {
		static readonly HttpMethod method = HttpMethod.Post;
		static readonly Uri endpoint = new Uri("https://example.com/api/test");

		static JsonObject Build(int status, string? contentType, string? content)
			=> (JsonObject)JsonNode.Parse(
				IHttpException.BuildMessage(status, method, endpoint,
					new HttpResponseContent { ContentType = contentType, Content = content }))!;

		[Fact]
		public void JsonObjectContent_MergesStatusMethodEndpointIntoBody() {
			var json = Build(404, "application/json", "{\"message\":\"missing\"}");

			Assert.Equal("missing", (string?)json["message"]);
			Assert.Equal(404, (int)json["status"]!);
			Assert.Equal("POST", (string?)json["method"]);
			Assert.Equal("https://example.com/api/test", (string?)json["endpoint"]);
		}

		// content type denotes JSON (or anything else) but the body is not a JSON object: the raw body is wrapped
		[Theory]
		[InlineData("text/plain", "boom")]
		[InlineData("application/json", "this is not json")]
		[InlineData("application/json", "[1,2,3]")]
		public void NonJsonObjectContent_WrapsRawContent(string contentType, string content) {
			var json = Build(500, contentType, content);

			Assert.Equal(500, (int)json["status"]!);
			Assert.Equal("POST", (string?)json["method"]);
			Assert.Equal("https://example.com/api/test", (string?)json["endpoint"]);
			Assert.Equal(content, (string?)json["content"]);
		}

		// a +json suffix is still recognized as a JSON object and merged, not wrapped
		[Fact]
		public void ProblemJsonContent_MergesIntoBody() {
			var json = Build(422, "application/problem+json; charset=utf-8", "{\"title\":\"invalid\"}");

			Assert.Equal("invalid", (string?)json["title"]);
			Assert.Equal(422, (int)json["status"]!);
		}

		[Fact]
		public void MatchingStatusInBody_IsNotDuplicated() {
			var json = Build(404, "application/json", "{\"status\":404}");

			Assert.Equal(404, (int)json["status"]!);
			Assert.False(json.ContainsKey("status2"));
		}

		[Fact]
		public void ConflictingStatusInBody_IsPreservedAndOursSuffixed() {
			var json = Build(400, "application/json", "{\"status\":\"already-here\"}");

			Assert.Equal("already-here", (string?)json["status"]);
			Assert.Equal(400, (int)json["status2"]!);
		}

		[Fact]
		public void ConflictingMethodInBody_IsPreservedAndOursSuffixed() {
			var json = Build(400, "application/json", "{\"method\":\"GET\"}");

			Assert.Equal("GET", (string?)json["method"]);
			Assert.Equal("POST", (string?)json["method2"]);
		}

		// content type claims JSON but there is no body: there is nothing to merge, so the (empty) body is wrapped
		[Fact]
		public void EmptyJsonContent_WrapsWithEmptyContentProperty() {
			var json = Build(404, "application/json", "");

			Assert.Equal("", (string?)json["content"]);
			Assert.Equal(404, (int)json["status"]!);
			Assert.Equal("POST", (string?)json["method"]);
			Assert.Equal("https://example.com/api/test", (string?)json["endpoint"]);
		}
	}
}
