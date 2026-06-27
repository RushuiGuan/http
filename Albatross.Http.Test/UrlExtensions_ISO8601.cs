using System;
using Albatross.Http;
using Xunit;

namespace Albatross.Http.Test {
	public class UrlExtensions_ISO8601 {
		[Theory]
		[InlineData(2022, 10, 10, "2022-10-10")]
		[InlineData(2022, 1, 5, "2022-01-05")]
		[InlineData(1999, 12, 31, "1999-12-31")]
		public void DateOnly_FormatsAsYearMonthDay(int year, int month, int day, string expected) {
			var value = new DateOnly(year, month, day);
			Assert.Equal(expected, value.ISO8601());
		}

		// trailing zero-only fractional seconds are dropped, along with the now-orphaned decimal point
		[Theory]
		[InlineData(13, 5, 9, 0, "13:05:09")]
		[InlineData(1, 2, 3, 0, "01:02:03")]
		[InlineData(0, 0, 0, 0, "00:00:00")]
		[InlineData(13, 5, 9, 123, "13:05:09.123")]
		public void TimeOnly_FormatsWithTrimmedFraction(int hour, int minute, int second, int millisecond, string expected) {
			var value = new TimeOnly(hour, minute, second, millisecond);
			Assert.Equal(expected, value.ISO8601());
		}

		[Fact]
		public void DateTime_Utc_IncludesZDesignator() {
			var value = new DateTime(2022, 10, 10, 13, 5, 9, DateTimeKind.Utc);
			Assert.Equal("2022-10-10T13:05:09Z", value.ISO8601());
		}

		[Fact]
		public void DateTime_Unspecified_OmitsOffset() {
			var value = new DateTime(2022, 10, 10, 13, 5, 9, DateTimeKind.Unspecified);
			Assert.Equal("2022-10-10T13:05:09", value.ISO8601());
		}

		[Theory]
		[InlineData(0, "2022-10-10T13:05:09+00:00")]
		[InlineData(2, "2022-10-10T13:05:09+02:00")]
		[InlineData(-5, "2022-10-10T13:05:09-05:00")]
		public void DateTimeOffset_IncludesOffset(int offsetHours, string expected) {
			var value = new DateTimeOffset(2022, 10, 10, 13, 5, 9, TimeSpan.FromHours(offsetHours));
			Assert.Equal(expected, value.ISO8601());
		}
	}
}
