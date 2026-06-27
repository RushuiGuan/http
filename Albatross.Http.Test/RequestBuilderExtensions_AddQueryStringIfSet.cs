using System;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class RequestBuilderExtensions_AddQueryStringIfSet {
		static string BuiltUrl(RequestBuilder builder) => Uri.UnescapeDataString(builder.Build().RequestUri!.ToString());

		// reference overload: only sets the parameter when the value is non-null and stringifies to a non-empty value
		[Fact]
		public void ReferenceValue_Set_IsAdded() {
			var url = BuiltUrl(new RequestBuilder().WithRelativeUrl("api").AddQueryStringIfSet("name", "alice"));
			Assert.Equal("api?name=alice", url);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		public void ReferenceValue_NullOrEmpty_IsOmitted(string? value) {
			var url = BuiltUrl(new RequestBuilder().WithRelativeUrl("api").AddQueryStringIfSet("name", value));
			Assert.Equal("api", url);
		}

		// value overload: only sets the parameter when the nullable has a value
		[Fact]
		public void ValueType_Set_IsAdded() {
			var url = BuiltUrl(new RequestBuilder().WithRelativeUrl("api").AddQueryStringIfSet("id", (int?)42));
			Assert.Equal("api?id=42", url);
		}

		[Fact]
		public void ValueType_Null_IsOmitted() {
			var url = BuiltUrl(new RequestBuilder().WithRelativeUrl("api").AddQueryStringIfSet("id", (int?)null));
			Assert.Equal("api", url);
		}

		// date/time value types are formatted using their ISO8601 representation rather than the default ToString()
		[Fact]
		public void DateTimeValue_UsesIso8601() {
			var value = new DateTime(2022, 10, 10, 13, 5, 9, DateTimeKind.Utc);
			var url = BuiltUrl(new RequestBuilder().WithRelativeUrl("api").AddQueryStringIfSet("d", (DateTime?)value));
			Assert.Equal("api?d=2022-10-10T13:05:09Z", url);
		}

		[Fact]
		public void DateOnlyValue_UsesIso8601() {
			var url = BuiltUrl(new RequestBuilder().WithRelativeUrl("api").AddQueryStringIfSet("d", (DateOnly?)new DateOnly(2022, 10, 10)));
			Assert.Equal("api?d=2022-10-10", url);
		}

		[Fact]
		public void TimeOnlyValue_UsesIso8601() {
			var url = BuiltUrl(new RequestBuilder().WithRelativeUrl("api").AddQueryStringIfSet("t", (TimeOnly?)new TimeOnly(13, 5, 9)));
			Assert.Equal("api?t=13:05:09", url);
		}

		[Fact]
		public void DateTimeOffsetValue_UsesIso8601() {
			var value = new DateTimeOffset(2022, 10, 10, 13, 5, 9, TimeSpan.Zero);
			var url = BuiltUrl(new RequestBuilder().WithRelativeUrl("api").AddQueryStringIfSet("dto", (DateTimeOffset?)value));
			Assert.Equal("api?dto=2022-10-10T13:05:09+00:00", url);
		}

		[Fact]
		public void OtherValueType_UsesDefaultFormatting() {
			var url = BuiltUrl(new RequestBuilder().WithRelativeUrl("api").AddQueryStringIfSet("flag", (bool?)true));
			Assert.Equal("api?flag=True", url);
		}
	}
}
